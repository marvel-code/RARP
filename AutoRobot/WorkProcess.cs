using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using Ecng.Collections;
using Ecng.Common;
using Ecng.Xaml;

using StockSharp.Algo.Candles;
using StockSharp.Algo.Strategies;
using StockSharp.BusinessEntities;
using StockSharp.Quik;

using System.ServiceModel;
using System.ServiceModel.Channels;
using System.ServiceModel.Description;

using SharedProject;

namespace AutoRobot
{
    public partial class WorkProcess : TimeFrameStrategy, IDisposable
    {
        //
        // Threads
        //

        volatile bool doStopConnectionReaffirmationThread;
        Thread threadConnectionReaffirmation;

        //
        // init
        //

        IWorkService proxy;
        ServerDataObject serverDataObj = new ServerDataObject();
        TradeState tradeState;
        PartnerDataObject clientDataObj;

        public MainWindow mw = MainWindow.Instance;

        public bool is_Trade = false;
        public bool is_Work = false;
        
        CandleManager _candleManager;

        public DateTime startDateTime { get; private set; }

        //
        // Exchange variables
        //

        private Decimal _position;
        
        public List<int> tfPeriods;
        public List<Candle>[] transmittedCandles;
        public List<Trade> transmittedTrades;

        public WorkProcess(CandleManager candleManager, QuikTrader quikTrader, Portfolio portfolio, Security secutirty) : base(TimeSpan.FromSeconds(0.1)) // Период стратегии ?
        {
            // -- init proxy

            clientDataObj = new PartnerDataObject();

            WSHttpBinding binding = new WSHttpBinding(SecurityMode.None, true);
            EndpointAddress address = new EndpointAddress("http://185.158.153.217:8010/WorkService");
            //EndpointAddress address = new EndpointAddress("http://127.0.0.1:8010/WorkService");

            proxy = ChannelFactory<IWorkService>.CreateChannel(binding, address);
            
            var errorMessage = proxy.InitConnection("TestUser");
            if (errorMessage != null)
            {
                throw new Exception(errorMessage);
            }

            // Reaffirmation thread
            threadConnectionReaffirmation = new Thread(() =>
            {
                doStopConnectionReaffirmationThread = false;
                while(false && !doStopConnectionReaffirmationThread)
                {
                    ReaffirmConnection();
                    Thread.Sleep(5 * 1000);
                }
            });
            threadConnectionReaffirmation.Start();

            // -- init local

            _candleManager = candleManager;
            Interval = TimeSpan.FromSeconds(1);
            Trader = quikTrader;
            Portfolio = portfolio;
            Security = secutirty;

            PropertyChanged += Process_PropertyChanged;
            OrderFailed += Process_OrderFailed;
            StopOrderFailed += Process_StopOrderFailed;

            // - Timeframe registration
            tfPeriods = proxy.GetTimeFramePeriods();
            transmittedCandles = new List<Candle>[tfPeriods.Count];
            foreach (var tf_per in tfPeriods)
            {
                if (!_candleManager.IsTimeFrameCandlesRegistered(Security, TimeSpan.FromSeconds(tf_per))) _candleManager.RegisterTimeFrameCandles(Security, TimeSpan.FromSeconds(tf_per));
            }
        }
        public void MyDoDispose()
        {
            TerminateMyConnection();
        }

        //
        // Main
        //
        
        private bool ReaffirmConnection()
        {
            try
            {
                //proxy.ReaffirmConnection();
                return true;
            }
            catch (Exception ex)
            {
                addLogMessage("Попытка подключения к серверу..");
                return false;
            }
        }
        private void TerminateMyConnection()
        {
            try
            {
                doStopConnectionReaffirmationThread = true;
                proxy.TerminateConnection();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Не удалось закрыть соединение с сервером:\n\n" + ex.Message);
            }
        }
         
        // Start robot
        protected override void OnStarting()
        {
            base.OnStarting();

            addLogMessage("Старт робота.\n");
            mw.tb_status.Background = mw.Select_Color(MyColors.Yellow);

            ///
            /// ЭТО ОТДЕЛЬНЫЙ ПОТОК МОНИТОРИНГА СВЕЧ
            ///
            //_candleManager.CandlesChanged += (token, candles) => proxy.Process_Token(token, candles);

            // -- Synchronization
            Thread loadingThread = new Thread(() =>
            {
                double interval_sec;
                double intervalInitial_sec = (TM.terminalDateTime - TM.MySecurity.LastTrade.Time).TotalSeconds;
                Boolean is_LoadCandles = false;

                while (!is_Work)
                {
                    // - Обработка загрузки робота
                    interval_sec = (TM.terminalDateTime - TM.MySecurity.LastTrade.Time).TotalSeconds;

                    if (is_LoadCandles)
                    {   // - Finalizer of loading
                        if (TM.terminalDateTime > startDateTime)
                        {
                            //Process_CandlesFill();

                            addLogMessage("Загрузка исторических данных закончена");

                            is_Work = true;

                            if (TM.trade_cfg.is_Test)
                                addLogMessage("РЕЖИМ ТЕСТИРОВАНИЯ");
                            else
                                addLogMessage("РЕЖИМ ТОРГОВЛИ");

                            mw.updateRobotStatus();
                        }
                        else
                        {
                            int Seconds = (int)Math.Round((startDateTime - TM.terminalDateTime).TotalSeconds);
                            // Countdown
                            mw.setTextboxText(mw.tb_status, string.Format("{0}с до старта", Seconds < 0 ? 0 : Seconds));
                        }
                    }
                    else
                    {   // Loading..
                        // Loading status
                        double Percents = Math.Round(100 * (1 - interval_sec / intervalInitial_sec), 1, MidpointRounding.AwayFromZero);
                        mw.setTextboxText(mw.tb_status, string.Format("{0}%", Percents < 0 ? 0 : Percents));
                        // Check synchronization
                        if (interval_sec < 2)
                        {   // Synchronized
                            startDateTime = DateTime.Now.AddSeconds(4);
                            is_LoadCandles = true;
                        }
                    }
                }
            });
            loadingThread.Start();
        }
        // Refresh signal
        Object lockObj_OnProcess = new Object();
        protected override ProcessResults OnProcess()
        {
            lock (lockObj_OnProcess)
            {
                if (!is_Work)
                    return ProcessResults.Continue;

                // Stop work
                if (ProcessState == ProcessStates.Stopping)
                {
                    addLogMessage("Стратегия остановлена");
                    return ProcessResults.Stop;
                }

                if
                (
                    TM.trade_cfg.is_Test
                    ||
                    // Enter, if QUIK process orders without errors
                    (TM.last_EnterOrder == null || TM.last_EnterOrder.State != OrderStates.Failed)
                    &&
                    (TM.last_ExitOrder == null || TM.last_ExitOrder.State != OrderStates.Failed)
                    &&
                    (TM.last_StopOrder == null || TM.last_StopOrder.State != OrderStates.Failed)
                )
                {
                    /**
                     * Update everything
                     **/

                    // Indicators
                    /*
                    Process_UpdateIndicators();

                    try
                    { }
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обновления индикаторов: " + ex.Message);
                        return Process_Robot_Wait();
                    }
                    */

                    // TradeManager

                    try { TM.updateValues(); }
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обновления TradeManager: " + ex.Message);
                        return Process_Robot_Wait();
                    }
                    // Current position

                    try { _position = TM.Current_Position; }
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обновления TradeManager: " + ex.Message);
                        return Process_Robot_Wait();
                    }
                    // Current need action
                    NeedAction needAction = NeedAction.LongOrShortOpen;
                    if (_position > 0)
                        needAction = NeedAction.LongClose;
                    else if (_position < 0)
                        needAction = NeedAction.ShortClose;
                    // PNL

                    try { updatePnlDisplays(); } 
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обновления PNL: " + ex.Message);
                        return Process_Robot_Wait();
                    }

                    try { if (is_Trade) Process_PnlLimits(); }
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обработки лимитов PNL: " + ex.Message);
                        return Process_Robot_Wait();
                    }
                    // Trading state
                    serverDataObj.NewCandles = getNewCandles();
                    serverDataObj.NewTrades = getNewTrades();
                    serverDataObj.TerminalTime = TM.terminalDateTime;
                    if (TM.last_EnterOrder != null)
                        serverDataObj.LastEnterTime = TM.last_EnterOrder.Time;
                    if (TM.last_ExitOrder != null)
                        serverDataObj.LastExitTime = TM.last_ExitOrder.Time;

                    // Client Data Object
                    clientDataObj.securitiesData = TM.MyTrader.Securities.Select(x => new SecuritiesRow { code = x.Code }).ToList();
                    clientDataObj.derivativePortfolioData = new List<DerivativePortfolioRow>() { new DerivativePortfolioRow { beginAmount = TM.MyPortfolio.BeginAmount.Value, variationMargin = TM.MyPortfolio.VariationMargin.Value } };
                    clientDataObj.derivativePositionsData = new List<DerivativePositionsRow>() { new DerivativePositionsRow { currentPosition = (int)(TM.MyTrader.GetPosition(TM.MyPortfolio, TM.MySecurity) ?? new Position { CurrentValue = 0 }).CurrentValue } };
                    clientDataObj.tradesData = TM.MyTrader.MyTrades.Select(x => new TradeData { id = x.Trade.Id, orderNumber = x.Order.Id, price = x.Trade.Price, volume = (int)x.Trade.Volume, dateTime = x.Trade.Time.ToString(@"YYYY/MM/dd HH:mm:ss") }).ToList();
                    clientDataObj.ordersData = TM.MyTrader.Orders.Except(TM.MyTrader.StopOrders).Select(x => new OrderData { id = x.Id, price = x.Price, volume = (int)x.Volume, balance = (int)x.Balance, dateTime = x.Time.ToString(@"YYYY/MM/dd HH:mm:ss"), secCode = x.Security.Code, derivedOrderId = (x.DerivedOrder ?? new Order() { Id = 0}).Id, side = x.Direction.ToString(), state = x.State.ToString(), comment = x.Comment }).ToList();
                    clientDataObj.stopOrdersData = TM.MyTrader.StopOrders.Select(x => new StopOrderData { id = x.Id, price = x.Price, stopPrice = (decimal)x.StopCondition.Parameters["StopPrice"], volume = (int)x.Volume, balance = (int)x.Balance, dateTime = x.Time.ToString(@"YYYY/MM/dd HH:mm:ss"), secCode = x.Security.Code, type = x.StopCondition.Parameters["Type"].ToString(), side = x.Direction.ToString(), state = x.State.ToString(), comment = x.Comment }).ToList();
                    
                    try
                    {
                        tradeState = proxy.GetTradeState(clientDataObj, serverDataObj, needAction);
                    }
                    catch (Exception ex)
                    {
                        mw.addLogSpoiler("Ошибка обновления состояния торговли", ex.Message);
                        return ProcessResults.Continue;
                        return Process_Robot_Wait();
                    }
                    // Monitoring
                    
                    try
                    { updateMonitorValues(); }
                    catch (Exception ex)
                    {
                        mw.addLogSpoiler("Ошибка обновления мониторинга: ", ex.Message);
                        return Process_Robot_Wait();
                    }

                    /**
                     * TRADING
                     **/

                    if (is_Trade)
                    {
                        if (!TM.is_Position)
                        {
                            if (tradeState.LongOpen && tradeState.LongClose || tradeState.ShortOpen && tradeState.ShortClose)
                            {
                                addLogMessage(string.Format("Замечено OPEN&CLOSE. Выход из торговли. (LO-LC SO-SC):({0}-{1} {2}-{3})", tradeState.LongOpen, tradeState.LongClose, tradeState.ShortOpen, tradeState.ShortClose));
                                mw.stopTrading();
                                return ProcessResults.Continue;
                            }
                            // LONG
                            if (tradeState.LongOpen && (TM.last_EnterOrder == null || TM.last_EnterOrder.State == OrderStates.Done))
                            {
                                // Order
                                TM.Register.Order(
                                    tradeState.RuleId, 
                                    TM.trade_cfg.Order_Volume, 
                                    OrderDirections.Buy, 
                                    OrderType.Enter
                                );
                                // Stop order
                                TM.Register.StopOrder(
                                    TM.trade_cfg.Order_Volume, 
                                    OrderDirections.Sell, 
                                    QuikStopConditionTypes.TakeProfitStopLimit
                                );
                                return ProcessResults.Continue;
                            }
                            // SHORT
                            if (tradeState.ShortOpen && (TM.last_EnterOrder == null || TM.last_EnterOrder.State == OrderStates.Done))
                            {
                                // Order
                                TM.Register.Order(
                                    tradeState.RuleId, 
                                    TM.trade_cfg.Order_Volume, 
                                    OrderDirections.Sell, 
                                    OrderType.Enter
                                );
                                // Stop order
                                TM.Register.StopOrder(
                                    TM.trade_cfg.Order_Volume, 
                                    OrderDirections.Buy, 
                                    QuikStopConditionTypes.TakeProfitStopLimit
                                );
                                return ProcessResults.Continue;
                            }
                        }
                        else if (TM.is_Position)
                        {
                            if (TM.is_Exit_From_Stop_Order())
                                return ProcessResults.Continue;

                            if (TM.last_EnterOrder.Direction == OrderDirections.Buy)
                            {
                                // SELL
                                if (tradeState.LongClose && (TM.last_ExitOrder == null || TM.last_ExitOrder.State == OrderStates.Done))
                                {
                                    // Exit order
                                    TM.Register.ExitOrder(
                                        tradeState.RuleId
                                    );
                                    return ProcessResults.Continue;
                                }
                            }
                            else
                            {
                                // COVER
                                if (tradeState.ShortClose && (TM.last_ExitOrder == null || TM.last_ExitOrder.State == OrderStates.Done))
                                {
                                    // Exit order
                                    TM.Register.ExitOrder(
                                        tradeState.RuleId
                                    );
                                    return ProcessResults.Continue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Обработка недопустимого кол-ва неисполненных заявок
                    if (TM.Exceptions_Count >= TM.trade_cfg.Max_Exceptions_Count)
                    {
                        addLogMessage("Кол-во исключений превысило максимально допустимый порог");
                        mw.stopTrading();
                        return ProcessResults.Continue;
                    }
                    // Заново регистрируем неудачные заявки
                    if (TM.last_EnterOrder != null && TM.last_EnterOrder.State == OrderStates.Failed)
                        TM.Register.FailedOrder(TM.last_EnterOrder, OrderType.Enter);
                    if (TM.last_ExitOrder != null && TM.last_ExitOrder.State == OrderStates.Failed)
                        TM.Register.FailedOrder(TM.last_ExitOrder, OrderType.Exit);
                    if (TM.last_StopOrder != null && TM.last_StopOrder.State == OrderStates.Failed)
                        TM.Register.FailedOrder(TM.last_StopOrder, OrderType.Stop);
                }

                return ProcessResults.Continue;
            }
        }

        //
        // Refresh robot state
        //

        // Refresh monitor values
        DateTime lastRefreshTime = DateTime.Now;
        private void updateMonitorValues()
        {
            mw.mw.Dispatcher.Invoke(new Action(() =>
            {
                mw.mw.tb_refreshInterval.Text = string.Format("{0:0}c", DateTime.Now.Subtract(lastRefreshTime).TotalSeconds);
                lastRefreshTime = DateTime.Now;

                if (tradeState.AdditionalData.message != "") addLogMessage(tradeState.AdditionalData.message);

                // COMMON
                mw.mw.tb_startTime.Text = (tradeState.AdditionalData.Open_Time).ToString(@"yyyy/MM/dd HH:mm:ss");
                mw.mw.tb_stopTime.Text = (tradeState.AdditionalData.Close_Time).ToString(@"yyyy/MM/dd HH:mm:ss");
                mw.mw.tb_allTrades_startTime.Text = (tradeState.AdditionalData.Open_Trades_Time).ToString(@"yyyy/MM/dd HH:mm:ss");
                mw.mw.tb_allTrades_stopTime.Text = (tradeState.AdditionalData.Close_Trades_Time).ToString(@"yyyy/MM/dd HH:mm:ss");

                decimal progressValue = transmitStartIndex / TM.MySecurity.Trader.Trades.Count();
                progressValue = Math.Ceiling(progressValue * 100);
                progressValue = progressValue > 100 ? 100 : progressValue;
                mw.mw.tb_progressLoading.Text = string.Format("{0}%", progressValue);
                mw.mw.tb_progressLoading.Background = progressValue == 100 ? Brushes.PaleGreen : Brushes.PaleVioletRed;

                mw.mw.tb_candlesCount.Text = tradeState.AdditionalData.Candles_N.ToString();
                mw.mw.tb_allTradesCount.Text = tradeState.AdditionalData.AllTrades_N.ToString();

                mw.mw.tb_lastUpdateTime.Text = TM.terminalDateTime.ToString(@"HH:mm:ss");

                // ADX
                mw.setTextboxText(mw.mw.tb_dx, tradeState.AdditionalData.adx_val.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_dip, tradeState.AdditionalData.adx_dip.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_dim, tradeState.AdditionalData.adx_dim.Round(MidpointRounding.AwayFromZero).ToString());

                mw.setTextboxText(mw.mw.tb_dx_p, tradeState.AdditionalData.adx_val_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_dip_p, tradeState.AdditionalData.adx_dip_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_dim_p, tradeState.AdditionalData.adx_dim_p.Round(MidpointRounding.AwayFromZero).ToString());

                // Volume
                mw.setTextboxText(mw.mw.tb_total, tradeState.AdditionalData.total.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_total_p, tradeState.AdditionalData.total_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_buy, tradeState.AdditionalData.buy.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_buy_p, tradeState.AdditionalData.buy_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_sell, tradeState.AdditionalData.sell.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_sell_p, tradeState.AdditionalData.sell_p.Round(MidpointRounding.AwayFromZero).ToString());

                /*
                // BBW
                mw.setTextboxText(mw.mw.tb_bbw, tf[0].bbw[0].val.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_bbw_p, tf[0].bbw[0].val_p.Round(MidpointRounding.AwayFromZero).ToString());
                // ROC
                mw.setTextboxText(mw.mw.tb_roc, tf[0].roc[0].val.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_roc_p, tf[0].roc[0].val_p.Round(MidpointRounding.AwayFromZero).ToString());
                // KAMA
                mw.setTextboxText(mw.mw.tb_kama, tf[0].kama[0].val.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_kama_p, tf[0].kama[0].val_p.Round(MidpointRounding.AwayFromZero).ToString());
                // Oscilators
                mw.setTextboxText(mw.mw.tb_vo, tf[0].volume[0].vo.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_vo_p, tf[0].volume[0].vo_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_bo, tf[0].volume[0].bo.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_bo_p, tf[0].volume[0].bo_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_so, tf[0].volume[0].so.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_so_p, tf[0].volume[0].so_p.Round(MidpointRounding.AwayFromZero).ToString());
                // Velocities
                mw.setTextboxText(mw.mw.tb_bv, tf[0].volume[0].bv.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_bv_p, tf[0].volume[0].bv_p.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_sv, tf[0].volume[0].sv.Round(MidpointRounding.AwayFromZero).ToString());
                mw.setTextboxText(mw.mw.tb_sv_p, tf[0].volume[0].sv_p.Round(MidpointRounding.AwayFromZero).ToString());*/
            }));
        }
        // Refresh PNL displays
        private void updatePnlDisplays()
        {
            mw.setTextboxTextAndBackgroundByValue(mw.tb_day_pnl, TM.Day_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.tb_session_pnl, TM.Session_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.tb_position_pnl, TM.Position_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.mw.tb_max_ppnl, TM.Max_Position_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.mw.tb_min_ppnl, TM.Min_Position_PNL);
        }
        // Finalizer of PnL limit reaching
        private void Process_PnlLimits()
        {
            if (TM.trade_cfg.Max_Day_Profit != 0 && TM.Session_PNL >= TM.trade_cfg.Max_Day_Profit)
            {
                addLogMessage("Лимит прибыли достигнут! УРА! :)");
                mw.stopTrading();
            }
            else if (TM.trade_cfg.Max_Day_Loss != 0 && TM.Session_PNL <= -TM.trade_cfg.Max_Day_Loss)
            {
                addLogMessage("Лимит убытка достигнут! За тучей идёт солнце!");
                mw.stopTrading();
            }
        }

        //
        // Error functions
        //
        private ProcessResults Process_Robot_Wait()
        {
            mw.Dispatcher.Invoke(() => {
                is_Work = false;
                addLogMessage("Робот в ожидании.\n");
                mw.tb_status.Text = "В ожидании";
                mw.tb_status.Background = mw.Select_Color(MyColors.Yellow);
            });
            return ProcessResults.Continue;
        }
        private void Process_OrderFailed(OrderFail orderFail)
        {
            addLogMessage(string.Format("Ошибка регистрации заявки №{0}. Сообщение об ошибке: {1}", orderFail.Order.Id, orderFail.Error.Message));
        }
        private void Process_StopOrderFailed(OrderFail orderFail)
        {
            addLogMessage(string.Format("Ошибка регистрации стоп-заявки №{0}. Сообщение об ошибке: {1}", orderFail.Order.Id, orderFail.Error.Message));
        }

        //
        // Others
        //

        // Refresh status display
        private void Process_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            mw.updateRobotStatus();
        }
        // Add log message
        private void addLogMessage(String message, params object[] obj)
        {
            mw.addLogMessage(message, obj);
        }

        //
        // Server preprocessing
        //

        private Candle[][] getNewCandles()
        {
            List<Candle>[] result = new List<Candle>[transmittedCandles.Length];

            for (int i = 0; i < tfPeriods.Count; i++)
            {
                result[i] = new List<Candle>();

                if (transmittedCandles[i] == null)
                {
                    transmittedCandles[i] = new List<Candle>();
                }

                var candles = _candleManager.GetTimeFrameCandles(Security, TimeSpan.FromSeconds(tfPeriods[i]), 1000).ToArray();
                for (int p = 0; p < candles.Length; p++)
                {
                    int k = candles.Length - 1 - p;
                    if (!transmittedCandles[i].Contains(candles[k]) || p == 0)
                    {
                        result[i].Add(candles[k]);
                    }
                }
                
                result[i].Reverse();
                transmittedCandles[i].AddRange(result[i]);
            }

            return result.Select(l => l.ToArray()).ToArray();
        }

        /// SETTINGS
        /// **
        // Limit trades count for transmit
        int maxTransmitTradesCount = 5000;
        /// **
        int transmitStartIndex = 0;
        private Trade[] getNewTrades()
        {
            List<Trade> result = new List<Trade>();
            var AllTrades = TM.MySecurity.Trader.Trades.ToArray();
            int nonTransmittedTradesCount = AllTrades.Length - transmitStartIndex;
            result.AddRange(AllTrades.Range(transmitStartIndex, nonTransmittedTradesCount > maxTransmitTradesCount ? maxTransmitTradesCount : nonTransmittedTradesCount));
            transmitStartIndex += result.Count;

            return result.ToArray();
        }
    }
}