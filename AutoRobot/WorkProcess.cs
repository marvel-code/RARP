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

using transportDataParrern;

namespace AutoRobot
{
    public partial class WorkProcess : TimeFrameStrategy, IDisposable
    {
        /// Threads

        private volatile bool doStopConnectionReaffirmationThread;
        private Thread threadConnectionReaffirmation;
        private Thread threadLoading;
        
        /// Settings

        const string USERNAME = "vz#1999";
        //const string IP = "185.158.153.217";
        const string IP = "127.0.0.1";
        const int PORT = 8020;
        const int maxServerExceptionCount = 5;

        /// 

        private int serverExceptionCount;
        private volatile bool isServerConnectionEstablished;

        /// init

        private MainWindow mw = MainWindow.Instance;
        private WSHttpBinding binding;
        private EndpointAddress address;
        private IWorkService _proxy;
        private List<int> _tfPeriods;
        private CandleManager _candleManager;

        public DateTime StartDateTime { get; private set; }


        public WorkProcess(CandleManager candleManager, QuikTrader quikTrader, Portfolio portfolio, Security secutirty) : base(TimeSpan.FromSeconds(0.1)) // Период стратегии ?
        {
            // -- init proxy
            binding = new WSHttpBinding(SecurityMode.None, true);
            address = new EndpointAddress(string.Format("http://{0}:{1}/WorkService", IP, PORT));
            //address = new EndpointAddress(string.Format("http://127.0.0.1:{0}/WorkService", PORT));
            threadConnectionReaffirmation = new Thread(() =>
            {
                int delaySeconds = 5;

                doStopConnectionReaffirmationThread = false;
                while (!doStopConnectionReaffirmationThread)
                {
                    reaffirmConnection();
                    Thread.Sleep(delaySeconds * 1000);
                }
            });
            initConnection();

            // -- init local
            _candleManager = candleManager;
            Interval = TimeSpan.FromSeconds(1);
            Trader = quikTrader;
            Portfolio = portfolio;
            Security = secutirty;

            // - Timeframe registration
            _tfPeriods = _proxy.GetTimeFramePeriods();
            transmittedCandlesCount = new int[_tfPeriods.Count];
            foreach (var tf_per in _tfPeriods)
            {
                if (!_candleManager.IsTimeFrameCandlesRegistered(Security, TimeSpan.FromSeconds(tf_per))) _candleManager.RegisterTimeFrameCandles(Security, TimeSpan.FromSeconds(tf_per));
            }
        }


        public void MyDoDispose()
        {
            terminateConnection();
        }


        /// Main
        private bool wasEnterFalse;

        public bool isTrade = false;
        public bool isWork = false;
        private Object lockObj_OnProcess = new Object();
         
        protected override void OnStarting()
        {
            base.OnStarting();

            addLogMessage("Старт робота.\n");
            mw.tb_status.Background = mw.Select_Color(MyColors.Yellow);
            
            // -- Synchronization
            threadLoading = new Thread(() =>
            {
                double interval_sec;
                double intervalInitial_sec = (TM.terminalDateTime - TM.MySecurity.LastTrade.Time).TotalSeconds;
                Boolean is_LoadCandles = false;

                while (!isWork)
                {
                    interval_sec = (TM.terminalDateTime - TM.MySecurity.LastTrade.Time).TotalSeconds;

                    if (is_LoadCandles)
                    {   // - Finalizer of loading
                        if (TM.terminalDateTime > StartDateTime)
                        {
                            addLogMessage("Загрузка исторических данных закончена");
                            if (TM.tradeСfg.is_Test) addLogMessage("РЕЖИМ ТЕСТИРОВАНИЯ");
                            else addLogMessage("РЕЖИМ ТОРГОВЛИ");

                            isWork = true;
                            mw.updateRobotStatus();
                        }
                        else
                        {
                            int Seconds = (int)Math.Round((StartDateTime - TM.terminalDateTime).TotalSeconds);
                            // Countdown
                            mw.setTextboxText(mw.tb_status, string.Format("{0}с до старта", Seconds < 0 ? 0 : Seconds));
                        }
                    }
                    else
                    {   // - Loading..
                        // Loading status
                        double Percents = Math.Round(100 * (1 - interval_sec / intervalInitial_sec), 1, MidpointRounding.AwayFromZero);
                        mw.setTextboxText(mw.tb_status, string.Format("{0}%", Percents < 0 ? 0 : Percents));
                        // Check for synchronization
                        if (interval_sec < 2)
                        {   // Synchronized
                            StartDateTime = DateTime.Now.AddSeconds(4);
                            is_LoadCandles = true;
                        }
                    }
                }
            });
            threadLoading.Start();
        }


        protected override ProcessResults OnProcess()
        {
            lock (lockObj_OnProcess)
            {
                if (getLoadPercent() < 50)
                    wasEnterFalse = false;

                if (!isServerConnectionEstablished)
                    return ProcessResults.Continue;

                if (ProcessState == ProcessStates.Stopping)
                {
                    addLogMessage("Стратегия остановлена");
                    return ProcessResults.Stop;
                }

                if (!isWork)
                    return ProcessResults.Continue;
                
                if
                (
                    TM.tradeСfg.is_Test
                    ||
                    // Enter, if QUIK processes orders without errors
                    (TM.last_EnterOrder == null || TM.last_EnterOrder.State != OrderStates.Failed)
                    &&
                    (TM.last_ExitOrder == null || TM.last_ExitOrder.State != OrderStates.Failed)
                    &&
                    (TM.last_StopOrder == null || TM.last_StopOrder.State != OrderStates.Failed)
                )
                {
                    // Update TradeManager
                    try { TM.updateValues(); }
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обновления TradeManager: " + ex.Message);
                        return ProcessResults.Continue;
                    }

                    // Update current position
                    decimal currentPosition;
                    try { currentPosition = TM.Current_Position; }
                    catch (Exception ex)
                    {
                        addLogMessage("Ошибка обновления текущей позиции: " + ex.Message);
                        return ProcessResults.Continue;
                    }

                    // Update need action
                    NeedAction needAction = NeedAction.LongOrShortOpen;
                    if (currentPosition > 0)
                        needAction = NeedAction.LongClose;
                    else if (currentPosition < 0)
                        needAction = NeedAction.ShortClose;
                    // PNL
                    updatePnlDisplays();
                    if (isTrade) processPnlLimits();

                    // Set quik data 
                    ServerDataObject quikData = new ServerDataObject();
                    quikData.NewCandles = getNewCandles();
                    quikData.NewTrades = getNewTrades();
                    quikData.TerminalTime = TM.terminalDateTime;
                    if (TM.last_EnterOrder != null) quikData.LastEnterTime = TM.last_EnterOrder.Time;
                    if (TM.last_ExitOrder != null) quikData.LastExitTime = TM.last_ExitOrder.Time;

                    // Set client data 
                    PartnerDataObject partnerData = new PartnerDataObject();
                    partnerData.Position_PNL = TM.Position_PNL;
                    partnerData.Position_PNL_MAX = TM.Max_Position_PNL;
                    partnerData.Is_Trading = isTrade;
                    partnerData.lastEnterDirection = TM.last_EnterOrder == null ? "null" : TM.last_EnterOrder.Direction.ToString();
                    partnerData.securitiesData = TM.MyTrader.Securities.Select(x => new SecuritiesRow { code = x.Code }).ToList();
                    partnerData.derivativePortfolioData = new List<DerivativePortfolioRow>() { new DerivativePortfolioRow { beginAmount = TM.MyPortfolio.BeginAmount.Value, variationMargin = TM.MyPortfolio.VariationMargin.Value } };
                    partnerData.derivativePositionsData = new List<DerivativePositionsRow>() { new DerivativePositionsRow { currentPosition = (int)(TM.MyTrader.GetPosition(TM.MyPortfolio, TM.MySecurity) ?? new Position { CurrentValue = 0 }).CurrentValue } };
                    partnerData.tradesData = TM.MyTrader.MyTrades.Select(x => new TradeData { id = x.Trade.Id, orderNumber = x.Order.Id, price = x.Trade.Price, volume = (int)x.Trade.Volume, dateTime = x.Trade.Time.ToString(@"yyyy/MM/dd HH:mm:ss") }).ToList();
                    partnerData.ordersData = TM.MyTrader.Orders.Except(TM.MyTrader.StopOrders).Select(x => new OrderData { id = x.Id, price = x.Price, volume = (int)x.Volume, balance = (int)x.Balance, dateTime = x.Time.ToString(@"yyyy/MM/dd HH:mm:ss"), secCode = x.Security.Code, derivedOrderId = (x.DerivedOrder ?? new Order() { Id = 0}).Id, side = x.Direction.ToString(), state = x.State.ToString(), comment = x.Comment }).ToList();
                    partnerData.stopOrdersData = TM.MyTrader.StopOrders.Select(x => new StopOrderData { id = x.Id, price = x.Price, stopPrice = (decimal)x.StopCondition.Parameters["StopPrice"], volume = (int)x.Volume, balance = (int)x.Balance, dateTime = x.Time.ToString(@"yyyy/MM/dd HH:mm:ss"), secCode = x.Security.Code, type = x.StopCondition.Parameters["Type"].ToString(), side = x.Direction.ToString(), state = x.State.ToString(), comment = x.Comment }).ToList();

                    // Get trade state
                    TradeState tradeData;
                    try
                    {
                        tradeData = _proxy.GetTradeState(partnerData, quikData, needAction);
                    }
                    catch (Exception ex)
                    {
                        serverExceptionCount++;
                        mw.addLogSpoiler(string.Format("[{0}] Ошибка обновления состояния торговли", serverExceptionCount), ex.Message);

                        if (serverExceptionCount >= maxServerExceptionCount)
                            isServerConnectionEstablished = false;

                        return ProcessResults.Continue;
                    }

                    // Update monitor values
                    updateMonitorValues(tradeData);
                    
                    // Trading:
                    if (isTrade && getLoadPercent() > 95)
                    {
                        if (!wasEnterFalse && (needAction != NeedAction.LongOrShortOpen || !tradeData.LongOpen && !tradeData.ShortOpen))
                        {
                            wasEnterFalse = true;
                        }

                        if (!TM.is_Position && wasEnterFalse)
                        {

                            // Check for loop existence 
                            if (tradeData.LongOpen && tradeData.LongClose || tradeData.ShortOpen && tradeData.ShortClose)
                            {
                                string message = string.Format("Замечено OPEN&CLOSE. Выход из торговли. (LO-LC SO-SC):({0}-{1} {2}-{3})", tradeData.LongOpen, tradeData.LongClose, tradeData.ShortOpen, tradeData.ShortClose);
                                addLogMessage(message);
                                _proxy.LogMessage(message);
                                mw.stopTrading();

                                _proxy.LogMessage(message);
                                return ProcessResults.Continue;
                            }

                            // Long
                            if (tradeData.LongOpen && (TM.last_EnterOrder == null || TM.last_EnterOrder.State == OrderStates.Done))
                            {
                                // Order
                                TM.Register.Order(
                                    tradeData.RuleId, 
                                    TM.tradeСfg.Order_Volume, 
                                    OrderDirections.Buy, 
                                    OrderType.Enter
                                );
                                // Stop order
                                TM.Register.StopOrder(
                                    TM.tradeСfg.Order_Volume, 
                                    OrderDirections.Sell, 
                                    QuikStopConditionTypes.TakeProfitStopLimit
                                );

                                _proxy.LogTrade("LONG", TM.tradeСfg.Order_Volume, TM.Day_PNL, tradeData.RuleId, Security.LastTrade.Price, TM.tradeСfg.Order_Shift);
                                return ProcessResults.Continue;
                            }

                            // Short
                            if (tradeData.ShortOpen && (TM.last_EnterOrder == null || TM.last_EnterOrder.State == OrderStates.Done))
                            {
                                // Order
                                TM.Register.Order(
                                    tradeData.RuleId, 
                                    TM.tradeСfg.Order_Volume, 
                                    OrderDirections.Sell, 
                                    OrderType.Enter
                                );
                                // Stop order
                                TM.Register.StopOrder(
                                    TM.tradeСfg.Order_Volume, 
                                    OrderDirections.Buy, 
                                    QuikStopConditionTypes.TakeProfitStopLimit
                                );
                                
                                _proxy.LogTrade("SHORT", TM.tradeСfg.Order_Volume, TM.Day_PNL, tradeData.RuleId, Security.LastTrade.Price, TM.tradeСfg.Order_Shift);
                                return ProcessResults.Continue;
                            }
                        }
                        else if (TM.is_Position)
                        {
                            if (TM.is_Exit_From_Stop_Order())
                            {
                                _proxy.LogMessage(string.Format("Закрытие позиции по стопу {0}", Security.LastTrade.Price));
                                return ProcessResults.Continue;
                            }

                            if (TM.last_EnterOrder.Direction == OrderDirections.Buy)
                            {
                                // SELL
                                if (tradeData.LongClose && (TM.last_ExitOrder == null || TM.last_ExitOrder.State == OrderStates.Done))
                                {
                                    _proxy.LogTrade("SELL", TM.tradeСfg.Order_Volume, TM.Day_PNL, tradeData.RuleId, Security.LastTrade.Price, TM.tradeСfg.Order_Shift, TM.Position_PNL, TM.Min_Position_PNL, TM.Max_Position_PNL);

                                    // Exit order
                                    TM.Register.ExitOrder(
                                        tradeData.RuleId
                                    );
                                    return ProcessResults.Continue;
                                }
                            }
                            else
                            {
                                // COVER
                                if (tradeData.ShortClose && (TM.last_ExitOrder == null || TM.last_ExitOrder.State == OrderStates.Done))
                                {
                                    _proxy.LogTrade("COVER", TM.tradeСfg.Order_Volume, TM.Day_PNL, tradeData.RuleId, Security.LastTrade.Price, TM.tradeСfg.Order_Shift, TM.Position_PNL, TM.Min_Position_PNL, TM.Max_Position_PNL);

                                    // Exit order
                                    TM.Register.ExitOrder(
                                        tradeData.RuleId
                                    );
                                    return ProcessResults.Continue;
                                }
                            }
                        }
                    }
                }
                else
                {
                    // Exceptions overflow 
                    if (TM.Exceptions_Count >= TM.tradeСfg.Max_Exceptions_Count)
                    {
                        addLogMessage("Кол-во исключений превысило максимально допустимый порог");
                        mw.stopTrading();
                        TM.resetExceptionsCount();
                        return ProcessResults.Continue;
                    }

                    // Reregistering failed orders
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


        private void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            mw.updateRobotStatus();
        }


        private void addLogMessage(String message, params object[] obj)
        {
            mw.addLogMessage(message, obj);
        }


        /// Refresh robot state

        private DateTime lastRefreshTime = DateTime.Now;


        private void setRobotWait()
        {
            mw.Dispatcher.Invoke(() => {
                isWork = false;
                addLogMessage("Робот в ожидании.\n");
                mw.tb_status.Text = "В ожидании";
                mw.tb_status.Background = mw.Select_Color(MyColors.Yellow);
            });
        }


        private void updateMonitorValues(TradeState tradeState)
        {
            mw.mw.GuiAsync(() =>
            {
                mw.mw.tb_refreshInterval.Text = string.Format("{0:0}c", DateTime.Now.Subtract(lastRefreshTime).TotalSeconds);
                lastRefreshTime = DateTime.Now;

                if (tradeState.AdditionalData.message != "") addLogMessage(tradeState.AdditionalData.message);

                // COMMON
                mw.mw.tb_startTime.Text = (tradeState.AdditionalData.Open_Time).ToString(@"yyyy/MM/dd HH:mm:ss");
                mw.mw.tb_stopTime.Text = (tradeState.AdditionalData.Close_Time).ToString(@"yyyy/MM/dd HH:mm:ss");
                mw.mw.tb_allTrades_startTime.Text = (tradeState.AdditionalData.Open_Trades_Time).ToString(@"yyyy/MM/dd HH:mm:ss");
                mw.mw.tb_allTrades_stopTime.Text = (tradeState.AdditionalData.Close_Trades_Time).ToString(@"yyyy/MM/dd HH:mm:ss");

                decimal progressValue = getLoadPercent();
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
            });
        }


        private void updatePnlDisplays()
        {
            mw.setTextboxTextAndBackgroundByValue(mw.tb_day_pnl, TM.Day_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.tb_session_pnl, TM.Session_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.tb_position_pnl, TM.Position_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.mw.tb_max_ppnl, TM.Max_Position_PNL);
            mw.setTextboxTextAndBackgroundByValue(mw.mw.tb_min_ppnl, TM.Min_Position_PNL);
        }


        private void processPnlLimits()
        {
            if (TM.tradeСfg.Max_Day_Profit != 0 && TM.Session_PNL >= TM.tradeСfg.Max_Day_Profit)
            {
                addLogMessage("Лимит прибыли достигнут! УРА! :)");
                mw.stopTrading();
            }
            else if (TM.tradeСfg.Max_Day_Loss != 0 && TM.Session_PNL <= -TM.tradeСfg.Max_Day_Loss)
            {
                addLogMessage("Лимит убытка достигнут! За тучей идёт солнце!");
                mw.stopTrading();
            }
        }


        /// Error handlers


        protected override void OnOrderFailed(OrderFail orderFail)
        {
            addLogMessage(string.Format("Ошибка регистрации заявки №{0}. Сообщение об ошибке: {1}", orderFail.Order.Id, orderFail.Error.Message));
        }


        protected override void OnStopOrderFailed(OrderFail orderFail)
        {
            addLogMessage(string.Format("Ошибка регистрации стоп-заявки №{0}. Сообщение об ошибке: {1}", orderFail.Order.Id, orderFail.Error.Message));
        }


        /// Server synchronization
        

        private decimal getLoadPercent(decimal eps = 0)
        {
            decimal progressValue = (decimal)(transmittedTradesCount + eps) / TM.MySecurity.Trader.Trades.Count();
            progressValue = Math.Ceiling(progressValue * 100);
            progressValue = progressValue > 100 ? 100 : progressValue;

            return progressValue;
        }


        private void initConnection()
        {
            serverExceptionCount = 0;
            _proxy = ChannelFactory<IWorkService>.CreateChannel(binding, address);
            var errorMessage = _proxy.InitConnection(USERNAME);
            if (errorMessage != null)
            {
                throw new Exception(errorMessage);
            }
            if (threadConnectionReaffirmation.ThreadState != ThreadState.Running)
                threadConnectionReaffirmation.Start();
            if (_tfPeriods != null)
                transmittedCandlesCount = new int[_tfPeriods.Count];
            transmittedTradesCount = 0;

            isServerConnectionEstablished = true;
            addLogMessage("Успешное подключение к серверу!");
        }


        private bool reaffirmConnection()
        {
            try
            {
                _proxy.ReaffirmConnection();
                isServerConnectionEstablished = true;
                return true;
            }
            catch (Exception ex)
            {
                isServerConnectionEstablished = false;
                addLogMessage("Попытка подключения к серверу..");

                try
                {
                    if (++serverExceptionCount % maxServerExceptionCount == 0)
                        initConnection();
                    return true;
                }
                catch (Exception exc)
                {
                    addLogMessage(exc.Message.ToString());
                }
                return false;
            }
        }


        private void terminateConnection()
        {
            try
            {
                doStopConnectionReaffirmationThread = true;
                _proxy.TerminateConnection();
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show("Не удалось закрыть соединение с сервером:\n\n" + ex.Message);
            }
        }


        /// Server preprocessing

        private const int maxTransmitTradesCount = 5000;
        private int transmittedTradesCount = 0;
        private const int maxTransmitCandlesCount = 50;
        private int[] transmittedCandlesCount;
        

        private Candle[][] getNewCandles()
        {
            List<TimeFrameCandle>[] result = new List<TimeFrameCandle>[transmittedCandlesCount.Length];
            for (int i = 0; i < transmittedCandlesCount.Length; i++)
            {
                var candles = _candleManager.GetTimeFrameCandles(Security, TimeSpan.FromSeconds(_tfPeriods[i]), 1000).ToArray();
                int nonTransmittedCandlesCount = candles.Length - transmittedCandlesCount[i];
                result[i] = candles.Range(transmittedCandlesCount[i], nonTransmittedCandlesCount > maxTransmitTradesCount ? maxTransmitTradesCount : nonTransmittedCandlesCount).ToList();
                transmittedCandlesCount[i] += result[i].Count - 1;
            }

            return result.Select(x => x.ToArray()).ToArray();
        }


        private Trade[] getNewTrades()
        {
            var AllTrades = TM.MySecurity.Trader.Trades.ToArray();
            int nonTransmittedTradesCount = AllTrades.Length - transmittedTradesCount;
            List<Trade> result = AllTrades.Range(transmittedTradesCount, nonTransmittedTradesCount > maxTransmitTradesCount ? maxTransmitTradesCount : nonTransmittedTradesCount).ToList();
            transmittedTradesCount += result.Count;

            return result.ToArray();
        }
    }
}