using System;
using System.Collections.Generic;

using StockSharp.BusinessEntities;
using StockSharp.Algo.Candles;

using GUIServer;

namespace stocksharp.ServiceContracts
{
    public class ProcessingData
    {
        // >> Customizable strategy settings
        public List<int> tf_Periods = new List<int>
        {
            3600,
            1800
        };
        public List<ADX_Configuration> adx_cfgList = new List<ADX_Configuration>
        {
            new ADX_Configuration(0, 2, MaType.
                //Simple
                Exponential
                )
        };
        public List<BBW_Configuration> bbw_cfgList = new List<BBW_Configuration>
        {

        };
        public List<KAMA_Configuration> kama_cfgList = new List<KAMA_Configuration>
        {
            new KAMA_Configuration(0, 2, CalculationType.Median, 2, 30)
        };
        public List<MA_Configuration> ma_cfgList = new List<MA_Configuration>
        {

        };
        public List<ROC_Configuration> roc_cfgList = new List<ROC_Configuration>
        {
            new ROC_Configuration(0, 1, CalculationType.Median),
            new ROC_Configuration(0, 2, CalculationType.Median),
        };
        public List<Volume_Configuration> volume_cfgList = new List<Volume_Configuration>
        {
            new Volume_Configuration(0, 1, 1, 10)
        };

        public List<TimeFrame> timeFrameList;

        public List<Trade> AllTrades;
        public DateTimeOffset TerminalTime;
        public DateTimeOffset LastEnterTime;
        public DateTimeOffset LastExitTime;
        
        public void Init()
        {
            Log.addLog(LogType.Info, " >> INIT");

            AllTrades = new List<Trade>();
            TerminalTime = DateTimeOffset.Now;
            LastEnterTime = DateTimeOffset.Now;
            LastExitTime = DateTimeOffset.Now;

            timeFrameList = new List<TimeFrame>();
            foreach (var tf_per in tf_Periods)
                timeFrameList.Add(new TimeFrame(this, tf_per));
            // Initialize indicators (ind - indicator)
            foreach (var ind in adx_cfgList)
                timeFrameList[ind.TF_Number].adx.Add(new ADX(ind.Period, ind.Ma_Type));
            foreach (var ind in bbw_cfgList)
                timeFrameList[ind.TF_Number].bbw.Add(new BBW(ind.Period, ind.Deviation, ind.Calc_Type, ind.Ma_Type));
            foreach (var ind in kama_cfgList)
                timeFrameList[ind.TF_Number].kama.Add(new KAMA(ind.Period, ind.Calc_Type, ind.FN, ind.SN));
            foreach (var ind in ma_cfgList)
                timeFrameList[ind.TF_Number].ma.Add(new MA(ind.Period, ind.Ma_Type, ind.Calc_Type));
            foreach (var ind in roc_cfgList)
                timeFrameList[ind.TF_Number].roc.Add(new ROC(ind.Period, ind.Calc_Type));
            foreach (var ind in volume_cfgList)
                timeFrameList[ind.TF_Number].volume.Add(new Vol(this, ind.Long_Period, ind.Short_Period, ind.Velocity_Period_Seconds));
        }

        public void Update_AllTrades(Trade[] newTrades)
        {
            List<Trade> tradesToAdd = new List<Trade>();
            var lastRecordedTrade = AllTrades.Count == 0 ? null : AllTrades[AllTrades.Count - 1];

            Trade trade;
            for (int i = 0; i < newTrades.Length; i++)
            {
                trade = newTrades[newTrades.Length - 1 - i];

                if (lastRecordedTrade == trade)
                    break;

                tradesToAdd.Add(trade);
            }

            tradesToAdd.Reverse();
            AllTrades.AddRange(tradesToAdd);
        }

        public void Update_TfCandles(Candle[][] candles)
        {
            int i = -1;
            foreach (var _tf in timeFrameList)
            {
                i++;
                var buffer = _tf.Buffer;
                foreach (var candle in candles[i])
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
        public void Process_UpdateIndicators()
        {
            foreach (var _tf in timeFrameList)
                _tf.Update_Indicators();
        }
    }
}
