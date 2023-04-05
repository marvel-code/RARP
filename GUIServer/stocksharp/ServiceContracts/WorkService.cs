using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Threading;
using transportDataParrern;

namespace stocksharp.ServiceContracts
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.PerSession)]
    public partial class WorkService : IWorkService
    {
        // >> Connection
        private string _currentUser;

        public string InitConnection(string username)
        {
            _currentUser = username;
            _currentData = new ProcessingData();
            _currentData.Init();

            if (!_queue.ContainsKey(username))
            {
                _queue.Add(username, new Queue<Action>());
            }
            if (_qThreads.ContainsKey(username))
            {
                _qThreads.Remove(username);
            }
            _qThreads.Add(username, new Thread(() =>
            {
                while (true)
                {
                    Action act = null;
                    lock (_queue_locker)
                    {
                        if (_queue[username].Count != 0)
                        {
                            act = _queue[username].Dequeue();
                        }
                    }
                    if (act != null)
                    {
                        act();
                    }
                    Thread.Sleep(50);
                }
            }));
            //_qThreads[username].Start();

            bool result = UserManager.Process_UserConnect(username);

            if (!result)
            {
                return "Required user doesn`t exist or connected at the moment.";
            }

            return null;
        }
        public void ReaffirmConnection()
        {
            UserManager.ReaffirmUserConnection(_currentUser);
        }
        public void TerminateConnection()
        {
            UserManager.Process_UserDisconnect(_currentUser);
        }

        // >> Server data processing
        private ProcessingData _currentData;

        // ~ ignore
        private int c = 0;
        private static Dictionary<string, Thread> _qThreads = new Dictionary<string, Thread>();
        private volatile TradeState _tradeState = new TradeState();
        private static volatile object _queue_locker = new object();
        private static volatile Dictionary<string, Queue<Action>> _queue = new Dictionary<string, Queue<Action>>();
        private static volatile object _update_locker = new object();
        private void updateTradeState(PartnerDataObject partnerDataObject, ServerDataObject dataObj, NeedAction needAction)
        {
            lock (_update_locker)
            {
                try
                {
                    GUIServer.LogManager.RenderHtmlReport(_currentUser, partnerDataObject, _currentUser);
                    UserManager.Update_UserData(_currentUser);
                    GUIServer.MainWindow.Instance.SetPartnerData(_currentUser, partnerDataObject);
                }
                catch (Exception ex)
                {
                    Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления информации о пользователе" + ex);
                }

                TradeState result = new TradeState();

                // -- Synchronizing of current via received
                // - Time
                _currentData.TerminalTime = dataObj.TerminalTime.DateTime;
                _currentData.LastEnterTime = dataObj.LastEnterTime.DateTime;
                _currentData.LastExitTime = dataObj.LastExitTime.DateTime;
                // - Data
                try
                {
                    _currentData.Update_AllTrades(dataObj.NewTrades);
                    _currentData.Update_TradesIStarts();
                    _currentData.Update_TfCandles(dataObj.NewCandles);
                }
                catch (Exception ex)
                {
                    Log.addLog(GUIServer.LogType.Error, "Ошибка обновления данных");
                    TerminateConnection();
                }
                // - Indicators
                if (_currentData.GetTradeIStart(dataObj.NewCandles.Max(x => x.Max(y => y.Time))) != -1)
                {
                    try
                    {
                        _currentData.Process_UpdateIndicators();
                    }
                    catch (Exception ex)
                    {
                        Log.addLog(GUIServer.LogType.Error, "Ошибка обновления индикаторов");
                        TerminateConnection();
                    }

                    // -- Processing
                    try
                    {
                        result = getTradeState(_currentData.timeFrameList, needAction, partnerDataObject);
                    }
                    catch (Exception ex)
                    {
                        Log.addLog(GUIServer.LogType.Error, "Ошибка обновления состояния торговли" + ex.ToString());
                        result = new TradeState();
                    }
                }

                try
                {
                    List<TimeFrame> tf = _currentData.timeFrameList;
                    result.AdditionalData = new AdditionalDataStruct
                    {
                        message = "",
                        //message = string.Format("val={0} | val_p={1}", ProcessingData.timeFrameList[0].kama[0].val, ProcessingData.timeFrameList[0].kama[0].val_p),

                        adx_val = tf[0].adx[0].val,
                        adx_dip = tf[0].adx[0].dip,
                        adx_dim = tf[0].adx[0].dim,
                        adx_val_p = tf[0].adx[0].val_p,
                        adx_dip_p = tf[0].adx[0].dip_p,
                        adx_dim_p = tf[0].adx[0].dim_p,

                        total = tf[0].Volume[0].total,
                        total_p = tf[0].Volume[0].total_p,
                        buy = tf[0].Volume[0].buy,
                        buy_p = tf[0].Volume[0].buy_p,
                        sell = tf[0].Volume[0].sell,
                        sell_p = tf[0].Volume[0].sell_p,

                        Candles_N = tf.Sum(x => x.Buffer.Count),
                        AllTrades_N = _currentData.AllTrades.Count,

                        Open_Trades_Time = _currentData.AllTrades[0].Time,
                        Close_Trades_Time = _currentData.AllTrades.Last().Time,

                        Open_Time = tf[0].Buffer[0].Time,
                        Close_Time = tf[0].Buffer.Last().Time,
                    };
                }
                catch (Exception ex)
                {
                    Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления данных мониторинга" + ex);
                    TerminateConnection();
                }

                _tradeState = result;
            }
        }
        // ~
        public TradeState GetTradeState(PartnerDataObject partnerDataObject, ServerDataObject dataObj, NeedAction needAction)
        {
            if (dataObj.TerminalTime.Hour < 9 || DateTime.Now.Hour < 9)
            {
                TerminateConnection();
                return new TradeState();
            }

            try
            {
                GUIServer.LogManager.UpdateBillingData(partnerDataObject, _currentUser);
                GUIServer.LogManager.RenderHtmlReport(_currentUser, partnerDataObject, _currentUser);
                UserManager.Update_UserData(_currentUser);
                GUIServer.MainWindow.Instance.SetPartnerData(_currentUser, partnerDataObject);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления информации о пользователе" + ex);
            }

            TradeState result = new TradeState();

            // -- Synchronizing of current via received
            // - Time
            _currentData.TerminalTime = dataObj.TerminalTime.DateTime;
            _currentData.LastEnterTime = dataObj.LastEnterTime.DateTime;
            _currentData.LastExitTime = dataObj.LastExitTime.DateTime;
            // - Data
            try
            {
                _currentData.Update_AllTrades(dataObj.NewTrades);
                _currentData.Update_TradesIStarts();
                _currentData.Update_TfCandles(dataObj.NewCandles);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Error, "Ошибка обновления данных");
                TerminateConnection();
            }
            // - Indicators
            if (_currentData.GetTradeIStart(dataObj.NewCandles.Max(x => x.Max(y => y.Time))) != -1)
            {
                try
                {
                    _currentData.Process_UpdateIndicators();
                }
                catch (Exception ex)
                {
                    Log.addLog(GUIServer.LogType.Error, "Ошибка обновления индикаторов");
                    TerminateConnection();
                }

                // -- Processing
                try
                {
                    result = getTradeState(_currentData.timeFrameList, needAction, partnerDataObject);
                }
                catch (Exception ex)
                {
                    Log.addLog(GUIServer.LogType.Error, "Ошибка обновления состояния торговли" + ex.ToString());
                    result = new TradeState();
                }
            }

            try
            {
                List<TimeFrame> tf = _currentData.timeFrameList;
                result.AdditionalData = new AdditionalDataStruct
                {
                    message = "",
                    //message = string.Format("val={0} | val_p={1}", ProcessingData.timeFrameList[0].kama[0].val, ProcessingData.timeFrameList[0].kama[0].val_p),

                    adx_val = tf[0].adx[0].val,
                    adx_dip = tf[0].adx[0].dip,
                    adx_dim = tf[0].adx[0].dim,
                    adx_val_p = tf[0].adx[0].val_p,
                    adx_dip_p = tf[0].adx[0].dip_p,
                    adx_dim_p = tf[0].adx[0].dim_p,

                    total = tf[0].Volume[0].total,
                    total_p = tf[0].Volume[0].total_p,
                    buy = tf[0].Volume[0].buy,
                    buy_p = tf[0].Volume[0].buy_p,
                    sell = tf[0].Volume[0].sell,
                    sell_p = tf[0].Volume[0].sell_p,

                    Candles_N = tf.Sum(x => x.Buffer.Count),
                    AllTrades_N = _currentData.AllTrades.Count,

                    Open_Trades_Time = _currentData.AllTrades[0].Time,
                    Close_Trades_Time = _currentData.AllTrades.Last().Time,

                    Open_Time = tf[0].Buffer[0].Time,
                    Close_Time = tf[0].Buffer.Last().Time,
                };
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления данных мониторинга" + ex);
                TerminateConnection();
            }

            return result;
        }

        public List<int> GetTimeFramePeriods()
        {
            return _currentData.tf_Periods;
        }
        public void LogTrade(string action, int volume, decimal dayPnl, int ruleId, decimal securityPrice, decimal offset, decimal positionPnl = 0, decimal minPositionPnl = 0, decimal maxPositionPnl = 0)
        {
            // - Action log
            string message = string.Format("{0}({1}) по цене {2}", action, ruleId, securityPrice);
            if (action == "SELL" || action == "COVER")
            {
                message = string.Format("exit({1}) {0}", positionPnl, action);
            }
            Log.addLog(GUIServer.LogType.Info, string.Format("{0} :: {1}", _currentUser, message));

            // - Trade log
            string htmlLog = "";
            Dictionary<string, string> tradeInfo = new Dictionary<string, string>
            {
                {"Время", _currentData.TerminalTime.ToString("yyyy/MM/dd <b>HH:mm:ss")},
                {"Действие", action},
                {"Объем", volume.ToString()},
                {"PNL дня", dayPnl.ToString()},
                {"Правило", ruleId.ToString()},
                {"Цена инструмента", securityPrice.ToString()},
                {"Сдвиг", offset.ToString()},
                {"PNL позиции", action == "LONG" || action == "SHORT" ? "—" : positionPnl.ToString()},
                {"Min PNL позиции", action == "LONG" || action == "SHORT" ? "—" : minPositionPnl.ToString()},
                {"Max PNL позиции", action == "LONG" || action == "SHORT" ? "—" : maxPositionPnl.ToString()},
            };

            // Init trade log file if it doesnt exist
            string tradesLog_path = GUIServer.Globals.datereport_tradesLog_path(_currentUser);
            if (!File.Exists(tradesLog_path))
            {
                File.Create(tradesLog_path).Close();
                htmlLog += "<table width=100% cellpadding=5 border=1 style=\"border-collapse:collapse\">\r\n";
                htmlLog += "<tr>";
                foreach (KeyValuePair<string, string> i in tradeInfo)
                {
                    htmlLog += "<td>";
                    htmlLog += i.Key;
                }
                htmlLog += "\r\n";
            }

            // Log trade info
            htmlLog += "<tr>";
            foreach (KeyValuePair<string, string> i in tradeInfo)
            {
                htmlLog += "<td>";
                htmlLog += i.Value;
            }
            htmlLog += "\r\n";

            // Log indicators values
            htmlLog += "<tr>";
            htmlLog += "<td colspan=100>";
            htmlLog += "<details style=\"padding-left:10px\">";
            htmlLog += "<summary>Значения индикаторов</summary>";
            htmlLog += "<pre>";
            int tf_k = -1;
            foreach (TimeFrame tfi in _currentData.timeFrameList)
            {
                tf_k++;
                htmlLog += string.Format("<br><h1>{0}[{1}]</h1>", "TF", tf_k);

                int ind_k;

                /*// ADX
                ind_k = -1;
                foreach (var ind in tfi.adx)
                {
                    ind_k++;
                    htmlLog += string.Format("<h4>{0}[{1}]</h4>", "ADX", ind_k);

                    htmlLog += string.Format("val:\t\t{0:N0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t\t{0:N0}<br>", ind.val_p);
                    htmlLog += string.Format("dip:\t\t{0:N0}<br>", ind.dip);
                    htmlLog += string.Format("dip_p:\t\t\t{0:N0}<br>", ind.dip_p);
                    htmlLog += string.Format("dim:\t\t{0:N0}<br>", ind.dim);
                    htmlLog += string.Format("dim_p:\t\t\t{0:N0}<br>", ind.dim_p);
                }
                htmlLog += "<br>";

                // BBW
                ind_k = -1;
                foreach (var ind in tfi.bbw)
                {
                    ind_k++;
                    htmlLog += string.Format("<h4>{0}[{1}]</h4>", "BBW", ind_k);

                    htmlLog += string.Format("val:\t\t{0:N0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t\t{0:N0}<br>", ind.val_p);
                }
                htmlLog += "<br>";

                // KAMA
                ind_k = -1;
                foreach (var ind in tfi.kama)
                {
                    ind_k++;
                    htmlLog += string.Format("<h4>{0}[{1}]</h4>", "KAMA", ind_k);

                    htmlLog += string.Format("val:\t\t{0:N0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t\t{0:N0}<br>", ind.val_p);
                }
                htmlLog += "<br>";

                // MA
                ind_k = -1;
                foreach (var ind in tfi.kama)
                {
                    ind_k++;
                    htmlLog += string.Format("<h4>{0}[{1}]</h4>", "MA", ind_k);

                    htmlLog += string.Format("val:\t\t{0:N0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t\t{0:N0}<br>", ind.val_p);
                }
                htmlLog += "<br>";

                // ROC
                ind_k = -1;
                foreach (var ind in tfi.roc)
                {
                    ind_k++;
                    htmlLog += string.Format("<h4>{0}[{1}]</h4>", "ROC", ind_k);

                    htmlLog += string.Format("val:\t\t{0:N2}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t\t{0:N2}<br>", ind.val_p);
                }
                htmlLog += "<br>";*/

                // VOLUME
                ind_k = -1;
                foreach (Vol ind in tfi.Volume)
                {
                    ind_k++;
                    htmlLog += string.Format("<h4>{0}[{1}]</h4>", "VOLUME", ind_k);

                    htmlLog += string.Format("total:\t\t{0}<br>", ind.total);
                    htmlLog += string.Format("total_p:\t\t{0}<br>", ind.total_p);
                    htmlLog += string.Format("buy:\t\t{0}<br>", ind.buy);
                    htmlLog += string.Format("buy_p:\t\t\t{0}<br>", ind.buy_p);
                    htmlLog += string.Format("sell:\t\t{0}<br>", ind.sell);
                    htmlLog += string.Format("sell_p:\t\t\t{0}<br>", ind.sell_p);
                    htmlLog += string.Format("vector:\t\t{0}<br>", ind.vector);
                    htmlLog += string.Format("vector_p:\t\t{0}<br>", ind.vector_p);
                    htmlLog += string.Format("vector_h:\t{0}<br>", ind.vector_h);
                    htmlLog += string.Format("vector_hp:\t\t{0}<br>", ind.vector_hp);
                    htmlLog += string.Format("vector_l:\t{0}<br>", ind.vector_l);
                    htmlLog += string.Format("vector_lp:\t\t{0}<br>", ind.vector_lp);
                }
                htmlLog += "<br>";
            }
            // Others
            {
                htmlLog += $"TerminalTime = {TM.TerminalTime}";
                htmlLog += "<br>";
                var AllTrades = tf[0].volume.AllTrades;
                htmlLog += $"AllTradesLength = {AllTrades.Length}";
                htmlLog += "<br>";
                for (int i = 0; i < 5; ++i)
                {
                    var Trade = AllTrades[AllTrades.Length - 1 - i];
                    htmlLog += $"{Trade.Time} - {Trade.Id}";
                    htmlLog += "<br>";
                }
                htmlLog += "<br>";
                // tacts volumes
                List<TimeFrame> TF = _currentData.timeFrameList;
                htmlLog += "<table>";
                htmlLog += "<tr><td>" + "Индикатор" + "</td><td>" + "shift = 1" + "</td><td>" + "shift = 0" + "</td></tr>";
                foreach (int n in MySettings.PRICE_SETTINGS)
                {
                    htmlLog += "<tr><td>" + $"GetTactExpPrice({n})" + "</td><td>"
                        + TF[0].volume.GetTactExpPrice(n, 1) + "</td><td>"
                        + TF[0].volume.GetTactExpPrice(n, 0) + "</td></tr>";
                }
                htmlLog += "<tr><td>" + $"GetTactRealPrice" + "</td><td>"
                    + TF[0].volume.GetTactRealPrice(1) + "</td><td>"
                    + TF[0].volume.GetTactRealPrice(0) + "</td></tr>";
                foreach (KeyValuePair<int, int> kvp in MySettings.VELOCITIES_SETTINGS)
                {
                    htmlLog += "<tr><td>" + $"GetExpTv({kvp.Key}, {kvp.Value})" + "</td><td>"
                        + TF[0].volume.GetExpTv(kvp.Key, kvp.Value, 1) + "</td><td>"
                        + TF[0].volume.GetExpTv(kvp.Key, kvp.Value, 0) + "</td></tr>";
                }
                foreach (KeyValuePair<int, int> kvp in MySettings.VELOCITIES_SETTINGS)
                {
                    htmlLog += "<tr><td>" + $"GetExpVv({kvp.Key}, {kvp.Value})" + "</td><td>"
                        + TF[0].volume.GetExpVv(kvp.Key, kvp.Value, 1) + "</td><td>"
                        + TF[0].volume.GetExpVv(kvp.Key, kvp.Value, 0) + "</td></tr>";
                }
                htmlLog += "<tr><td>" + $"GetAvrTv(180)" + "</td><td>"
                    + TF[0].volume.GetAvrTv(180, 1) + "</td><td>"
                    + TF[0].volume.GetAvrTv(180, 0) + "</td></tr>";
                htmlLog += "<tr><td>" + $"GetAvrVv(180)" + "</td><td>"
                    + TF[0].volume.GetAvrVv(180, 1) + "</td><td>"
                    + TF[0].volume.GetAvrVv(180, 0) + "</td></tr>";
                htmlLog += "<tr><td>" + $"GetAvrTactsTvMax(180, 18)" + "</td><td>"
                    + TF[0].volume.GetAvrTactsTvMax(180, 18, 1) + "</td><td>"
                    + TF[0].volume.GetAvrTactsTvMax(180, 18, 0) + "</td></tr>";
                htmlLog += "<tr><td>" + $"GetAvrTvMin(180, 18)" + "</td><td>"
                    + TF[0].volume.GetAvrTactsTvMin(180, 18, 1) + "</td><td>"
                    + TF[0].volume.GetAvrTactsTvMin(180, 18, 0) + "</td></tr>";
                htmlLog += "<tr><td>" + $"GetAvrVvMax(180, 18)" + "</td><td>"
                    + TF[0].volume.GetAvrTactsVvMax(180, 18, 1) + "</td><td>"
                    + TF[0].volume.GetAvrTactsVvMax(180, 18, 0) + "</td></tr>";
                htmlLog += "<tr><td>" + $"GetAvrVvMin(180, 18)" + "</td><td>"
                    + TF[0].volume.GetAvrTactsVvMin(180, 18, 1) + "</td><td>"
                    + TF[0].volume.GetAvrTactsVvMin(180, 18, 0) + "</td></tr>";
                htmlLog += "</table>";
            }
            htmlLog += $"";
            htmlLog += "</pre>";
            htmlLog += "</details><br>\r\n";

            // Append trade log
            try
            {
                File.AppendAllText(tradesLog_path, htmlLog);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Error, "Не удалось логировать в файл: {0} content: {1}", ex.ToString(), htmlLog);
            }
        }
        public void LogMessage(string message)
        {
            Log.addLog(GUIServer.LogType.Info, string.Format("{0} :: {1}", _currentUser, message));
        }
    }
}
