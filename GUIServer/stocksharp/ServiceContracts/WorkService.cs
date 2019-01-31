using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;

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

            bool result = UserManager.Process_UserConnect(username);

            if (!result)
                return "Required user doesn`t exist or connected at the moment.";

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

        public TradeState GetTradeState(PartnerDataObject partnerDataObject, ServerDataObject dataObj, NeedAction needAction)
        {
            try
            {
                UserManager.Update_UserData(_currentUser);
                GUIServer.MainWindow.Instance.SetPartnerData(_currentUser, partnerDataObject);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления информации о пользователе");
                TerminateConnection();
            }
            
            TradeState result = null;

            // -- Synchronizing of current via received
            // - Time
            _currentData.TerminalTime = dataObj.TerminalTime;
            _currentData.LastEnterTime = dataObj.LastEnterTime;
            _currentData.LastExitTime = dataObj.LastExitTime;
            // - Data
            try
            {
                _currentData.Update_AllTrades(dataObj.NewTrades);
                _currentData.Update_TfCandles(dataObj.NewCandles);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления данных");
                TerminateConnection();
            }
            // - Indicators
            try
            {
                _currentData.Process_UpdateIndicators();
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления индикаторов");
                TerminateConnection();
            }

            // -- Processing
            try
            {
                result = updateTradeState(_currentData.timeFrameList, needAction, partnerDataObject);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления состояния торговли" + ex.ToString());
                TerminateConnection();
            }

            try
            {
                var tf = _currentData.timeFrameList;
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
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления данных мониторинга");
                TerminateConnection();
            }

            return result;
        }
        public List<int> GetTimeFramePeriods()
        {
            return _currentData.tf_Periods;
        }
        public void LogTrade(string action, int ruleId, decimal marketPrice, decimal pnl = 0)
        {
            // - Action log
            string message = string.Format("{0}({1}) по цене {2}", action, ruleId, marketPrice);
            if (action == "SELL" || action == "COVER")
            {
                message += string.Format(" {0} PNL: {1}", pnl == 0 ? ' ' : pnl > 0 ? '\u8593' : '\u8595', pnl);
            }
            Log.addLog(GUIServer.LogType.Info, string.Format("{0} :: {1}", _currentUser, message));

            // - Trade log
            string htmlLog = "";
            Dictionary<string, string> tradeInfo = new Dictionary<string, string>
            {
                {"Время", DateTime.Now.ToString("dd.MM.yyyy hh:mm:ss")},
                {"Действие", action},
                {"Правило", ruleId.ToString()},
                {"Рын. цена", marketPrice.ToString()},
            };
            
            // Init trade log file if it doesnt exist
            if (!System.IO.File.Exists(GUIServer.Globals.tradesLog_fullFileName))
            {
                htmlLog += "<table>\r\n";
                htmlLog += "<tr>";
                foreach (var i in tradeInfo)
                {
                    htmlLog += "<td>";
                    htmlLog += i.Key;
                }
                htmlLog += "\r\n";
            }

            // Log trade info
            htmlLog += "<tr>";
            foreach (var i in tradeInfo)
            {
                htmlLog += "<td>";
                htmlLog += i.Value;
            }
            htmlLog += "\r\n";

            // Log indicators values
            htmlLog += "<tr>";
            htmlLog += "<td>";
            htmlLog += "<details>";
            htmlLog += "<summary>Значения индикаторов</summary>";
            foreach (var tfi in _currentData.timeFrameList)
            {
                int k;

                // ADX
                k = -1;
                foreach (var ind in tfi.adx)
                {
                    k++;
                    htmlLog += string.Format("<br><h2>{0}[{1}]</h2><br>", "ADX", k);

                    htmlLog += string.Format("val:\t\t{0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t{0}<br>", ind.val_p);
                    htmlLog += string.Format("dip:\t\t{0}<br>", ind.dip);
                    htmlLog += string.Format("dip_p:\t\t{0}<br>", ind.dip_p);
                    htmlLog += string.Format("dim:\t\t{0}<br>", ind.dim);
                    htmlLog += string.Format("dim_p:\t\t{0}<br>", ind.dim_p);
                }

                // BBW
                k = -1;
                foreach (var ind in tfi.bbw)
                {
                    k++;
                    htmlLog += string.Format("<br><h2>{0}[{1}]</h2><br>", "BBW", k);

                    htmlLog += string.Format("val:\t\t{0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t{0}<br>", ind.val_p);
                }

                // KAMA
                k = -1;
                foreach (var ind in tfi.kama)
                {
                    k++;
                    htmlLog += string.Format("<br><h2>{0}[{1}]</h2><br>", "KAMA", k);

                    htmlLog += string.Format("val:\t\t{0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t{0}<br>", ind.val_p);
                }

                // MA
                k = -1;
                foreach (var ind in tfi.kama)
                {
                    k++;
                    htmlLog += string.Format("<br><h2>{0}[{1}]</h2><br>", "MA", k);

                    htmlLog += string.Format("val:\t\t{0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t{0}<br>", ind.val_p);
                }

                // ROC
                k = -1;
                foreach (var ind in tfi.roc)
                {
                    k++;
                    htmlLog += string.Format("<br><h2>{0}[{1}]</h2><br>", "ROC", k);

                    htmlLog += string.Format("val:\t\t{0}<br>", ind.val);
                    htmlLog += string.Format("val_p:\t\t{0}<br>", ind.val_p);
                }

                // VOLUME
                k = -1;
                foreach (var ind in tfi.Volume)
                {
                    k++;
                    htmlLog += string.Format("<br><h2>{0}[{1}]</h2><br>", "VOLUME", k);

                    htmlLog += string.Format("total:\t\t{0}<br>", ind.total);
                    htmlLog += string.Format("total_p:\t\t{0}<br>", ind.total_p);
                    htmlLog += string.Format("buy:\t\t{0}<br>", ind.buy);
                    htmlLog += string.Format("buy_p:\t\t{0}<br>", ind.buy_p);
                    htmlLog += string.Format("sell:\t\t{0}<br>", ind.sell);
                    htmlLog += string.Format("sell_p:\t\t{0}<br>", ind.sell_p);
                    htmlLog += string.Format("vector:\t\t{0}<br>", ind.vector);
                    htmlLog += string.Format("vector_p:\t\t{0}<br>", ind.vector_p);
                    htmlLog += string.Format("vector_h:\t\t{0}<br>", ind.vector_h);
                    htmlLog += string.Format("vector_hp:\t\t{0}<br>", ind.vector_hp);
                    htmlLog += string.Format("vector_l:\t\t{0}<br>", ind.vector_l);
                    htmlLog += string.Format("vector_lp:\t\t{0}<br>", ind.vector_lp);
                }
            }
            htmlLog += "</details>\r\n";

            // Append trade log
            try
            {
                System.IO.File.AppendAllText(GUIServer.Globals.log_fullFileName, message + "\r\n");
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Error, "Не удалось логировать в файл: {0}", ex.ToString());
            }
        }
        public void LogMessage(string message)
        {
            Log.addLog(GUIServer.LogType.Info, string.Format("{0} :: {1}", _currentUser, message));
        }
    }
}
