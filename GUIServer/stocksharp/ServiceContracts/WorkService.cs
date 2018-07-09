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

                    total = tf[0].volume[0].total,
                    total_p = tf[0].volume[0].total_p,
                    buy = tf[0].volume[0].buy,
                    buy_p = tf[0].volume[0].buy_p,
                    sell = tf[0].volume[0].sell,
                    sell_p = tf[0].volume[0].sell_p,

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

            if (needAction == NeedAction.LongOrShortOpen && result.LongOpen)
            {
                Log.addLog(GUIServer.LogType.Info, "Открытие LONG");
            }
            else if (needAction == NeedAction.LongOrShortOpen && result.ShortOpen)
            {
                Log.addLog(GUIServer.LogType.Info, "Открытие SHORT");
            }
            else if (needAction == NeedAction.LongClose && result.LongClose)
            {
                Log.addLog(GUIServer.LogType.Info, "Закрытие LONG");
            }
            else if (needAction == NeedAction.ShortClose && result.ShortClose)
            {
                Log.addLog(GUIServer.LogType.Info, "Закрытие SHORT");
            }

            return result;
        }
        public List<int> GetTimeFramePeriods()
        {
            return _currentData.tf_Periods;
        }
    }
}