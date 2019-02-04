using System;
using System.Linq;
using System.Collections.Generic;
using GUIServer;
using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;

namespace stocksharp.ServiceContracts
{
    public partial class ProcessingData
    {
        public List<TimeFrame> timeFrameList;

        public List<Trade> AllTrades;
        private DateTime _lastTradeIStartDatetime;
        public Dictionary<DateTime, int> TradesIStarts;
        public DateTimeOffset TerminalTime;
        public DateTimeOffset LastEnterTime;
        public DateTimeOffset LastExitTime;

        public int GetTradeIStart(DateTime time)
        {
            int result = -1;

            // Get nearest containing time
            while (!TradesIStarts.ContainsKey(time))
            {
                time = time.AddMinutes(1);
                if (time > TerminalTime)
                    break;
            }
            
            // If time isn't out of range
            if (TradesIStarts.ContainsKey(time))
                result = TradesIStarts[time];

            return result;
        }

        public void Init()
        {
            Log.addLog(LogType.Info, " >> INIT");

            AllTrades = new List<Trade>();
            _lastTradeIStartDatetime = DateTime.Now.Date.AddDays(-2);
            TradesIStarts = new Dictionary<DateTime, int>();
            TerminalTime = DateTimeOffset.Now;
            LastEnterTime = DateTimeOffset.Now;
            LastExitTime = DateTimeOffset.Now;

            timeFrameList = new List<TimeFrame>();
            foreach (int tf_per in tf_Periods)
            {
                timeFrameList.Add(new TimeFrame(this, tf_per));
            }
            // Initialize indicators (ind - indicator)
            foreach (ADX_Configuration ind in adx_cfgList)
            {
                timeFrameList[ind.TF_Number].adx.Add(new ADX(ind.Period, ind.Ma_Type));
            }

            foreach (BBW_Configuration ind in bbw_cfgList)
            {
                timeFrameList[ind.TF_Number].bbw.Add(new BBW(ind.Period, ind.Deviation, ind.Calc_Type, ind.Ma_Type));
            }

            foreach (KAMA_Configuration ind in kama_cfgList)
            {
                timeFrameList[ind.TF_Number].kama.Add(new KAMA(ind.Period, ind.Calc_Type, ind.FN, ind.SN));
            }

            foreach (MA_Configuration ind in ma_cfgList)
            {
                timeFrameList[ind.TF_Number].ma.Add(new MA(ind.Period, ind.Ma_Type, ind.Calc_Type));
            }

            foreach (ROC_Configuration ind in roc_cfgList)
            {
                timeFrameList[ind.TF_Number].roc.Add(new ROC(ind.Period, ind.Calc_Type));
            }

            foreach (Volume_Configuration ind in volume_cfgList)
            {
                timeFrameList[ind.TF_Number].Volume.Add(new Vol(this, ind.Long_Period, ind.Short_Period, ind.Velocity_Period_Seconds));
            }
        }

        public void Update_AllTrades(Trade[] newTrades)
        {
            List<Trade> tradesToAdd = new List<Trade>();
            Trade lastRecordedTrade = AllTrades.Count == 0 ? null : AllTrades[AllTrades.Count - 1];

            Trade trade;
            for (int i = 0; i < newTrades.Length; i++)
            {
                trade = newTrades[newTrades.Length - 1 - i];

                if (lastRecordedTrade == trade)
                {
                    break;
                }

                tradesToAdd.Add(trade);
            }

            tradesToAdd.Reverse();
            AllTrades.AddRange(tradesToAdd);
        }
        public void Update_TfCandles(Candle[][] candles)
        {
            int i = -1;
            foreach (TimeFrame _tf in timeFrameList)
            {
                i++;
                List<Candle> buffer = _tf.Buffer;
                foreach (Candle candle in candles[i])
                {
                    if (buffer.Count == 0 || candle.Time >= buffer[_tf.Buffer.Count - 1].Time)
                    {
                        _tf.Process_Candle(candle);
                    }
                    else
                    {
                        //addLogMessage(string.Format("Странное изменение свечи таймрейма с периодом {0}с", _tf.Period));
                    }

                }
            }
        }
        public void Update_TradesIStarts()
        {
            while (_lastTradeIStartDatetime.AddMinutes(1) < AllTrades[AllTrades.Count - 1].Time)
            {
                for (int i = TradesIStarts.Count == 0 ? 0 : TradesIStarts[_lastTradeIStartDatetime]; i < AllTrades.Count; i++)
                {
                    if (AllTrades[i].Time > _lastTradeIStartDatetime.AddMinutes(1))
                    {

                        _lastTradeIStartDatetime = _lastTradeIStartDatetime.AddMinutes(Math.Floor((AllTrades[i].Time - _lastTradeIStartDatetime).TotalMinutes));
                        TradesIStarts[_lastTradeIStartDatetime] = i;
                    }
                }
            }
        }

        public void Process_UpdateIndicators()
        {
            foreach (TimeFrame _tf in timeFrameList)
            {
                _tf.Update_Indicators();
            }
        }
    }
}
