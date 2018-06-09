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
        string currentUser;

        public string InitConnection(string username)
        {
            currentUser = username;
            bool result = UserManager.Process_UserConnect(username);

            if (!result)
            {
                return "Required user doesn`t exist or connected at the moment.";
            }

            return null;
        }
        public void ReaffirmConnection()
        {
            UserManager.ReaffirmUserConnection(currentUser);
        }
        public void TerminateConnection()
        {
            UserManager.Process_UserDisconnect(currentUser);
        }

        // >> Server processing
        public ServerDataObject currentDataObj;
        public List<TimeFrame> tf;

        public TradeState GetTradeState(PartnerDataObject partnerDataObject, ServerDataObject dataObj, NeedAction needAction)
        {
            try
            {
                UserManager.Update_UserData(currentUser);
                GUIServer.MainWindow.Instance.SetPartnerData(currentUser, partnerDataObject);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления информации о пользователе");
                TerminateConnection();
            }

            currentDataObj = dataObj;
            TradeState result = null;

            // -- Synchronizing of current via received
            // - Time
            ProcessingData.TerminalTime = dataObj.TerminalTime;
            ProcessingData.LastEnterTime = dataObj.LastEnterTime;
            ProcessingData.LastExitTime = dataObj.LastExitTime;
            // - Data
            try
            {
                ProcessingData.Update_AllTrades(dataObj.NewTrades);
                ProcessingData.Update_TfCandles(currentDataObj.NewCandles);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления данных");
                TerminateConnection();
            }
            // - Indicators
            try
            {
                ProcessingData.Process_UpdateIndicators();
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления индикаторов");
                TerminateConnection();
            }

            // -- Processing
            try
            {
                tf = ProcessingData.timeFrameList;
                result = updateTradeState(needAction);
            }
            catch (Exception ex)
            {
                Log.addLog(GUIServer.LogType.Warn, "Ошибка обновления состояния торговли");
                TerminateConnection();
            }

            try
            {
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
                    AllTrades_N = ProcessingData.AllTrades.Count,

                    Open_Trades_Time = ProcessingData.AllTrades[0].Time,
                    Close_Trades_Time = ProcessingData.AllTrades.Last().Time,

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
            return ProcessingData.tf_Periods;
        }
    }
}