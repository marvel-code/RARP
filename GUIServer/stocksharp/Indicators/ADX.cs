using System;
using System.Collections.Generic;

using StockSharp.Algo.Candles;

namespace stocksharp
{
    public class ADX
    {
        // Рабочие значения
        public Decimal val { get { return Math.Round(DX_Values.Get_Value()); } }
        public Decimal val_p { get { return Math.Round(DX_Values.Get_Value(1)); } }
        public Decimal dip { get { return Math.Round(DIPlus_Values.Get_Value()); } }
        public Decimal dip_p { get { return Math.Round(DIPlus_Values.Get_Value(1)); } }
        public Decimal dim { get { return Math.Round(DIMinus_Values.Get_Value()); } }
        public Decimal dim_p { get { return Math.Round(DIMinus_Values.Get_Value(1)); } }

        // Параметры
        public MaType Ma_Type { get; private set; }
        public int Period { get; private set; }
        // Массивы значений
        private MA DX_Values;
        private MA DIPlus_Values;
        private MA DIMinus_Values;
        // Инициализация
        public ADX(int _Period, MaType _Ma_Type)
        {
            Period = _Period;
            Ma_Type = _Ma_Type;

            Reset();
        }
        // Обновление значений
        public void Update(List<Candle> _Buffer)
        {
            Candle candle = _Buffer.Count == 0 ? null : _Buffer[_Buffer.Count - 1];
            Candle candle_p = _Buffer.Count < 2 ? null : _Buffer[_Buffer.Count - 2];

            if (candle == null)
                return;
            
            if (candle_p != null)
            {
                // INIT
                Decimal dmp;  // +DM
                Decimal dmm;  // -DM
                Decimal Sdip; // +SDI
                Decimal Sdim; // -SDI
                // +DM, -DM
                // +DI
                if (candle.HighPrice > candle_p.HighPrice)
                    dmp = candle.HighPrice - candle_p.HighPrice;
                else
                    dmp = 0;
                // -DI
                if (candle.LowPrice < candle_p.LowPrice)
                    dmm = candle_p.LowPrice - candle.LowPrice;
                else
                    dmm = 0;
                // +DI V -DI
                if (dmp == dmm)
                    dmm = dmp = 0;
                if (dmp > dmm)
                    dmm = 0;
                else
                    dmp = 0;
                // TR
                Decimal TR = Math.Max(Math.Max(candle.HighPrice - candle.LowPrice, Math.Abs(candle.HighPrice - candle_p.ClosePrice)), Math.Abs(candle.LowPrice - candle_p.ClosePrice));
                // +SDI -SDI
                if (TR != 0)
                {
                    Sdip = (dmp / TR) * 100;
                    Sdim = (dmm / TR) * 100;
                }
                else
                {
                    Sdip = 0;
                    Sdim = 0;
                }

                // Обновляем массивы значений +DI,-DI
                DIPlus_Values.Update(new TimeFrameCandle { Time = candle.Time, ClosePrice = Sdip });
                DIMinus_Values.Update(new TimeFrameCandle { Time = candle.Time, ClosePrice = Sdim });
                // Считаем DX
                Decimal DIPlus = Math.Round(DIPlus_Values.Get_Value());
                Decimal DIMinus = Math.Round(DIMinus_Values.Get_Value());
                Decimal DX = DIPlus + DIMinus == 0 ? 0 : Math.Abs(DIPlus - DIMinus) / (DIPlus + DIMinus) * 100;
                // Обновляем значения DX
                DX_Values.Update(new TimeFrameCandle { Time = candle.Time, ClosePrice = DX });
            }
        }
        public void Reset()
        {
            DX_Values = new MA(Period, Ma_Type, CalculationType.Close);
            DIPlus_Values = new MA(Period, Ma_Type, CalculationType.Close);
            DIMinus_Values = new MA(Period, Ma_Type, CalculationType.Close);
        }
        // Время открытия свечи для значения
        public DateTime Get_OpenTime(Byte shift = 0)
        {
            return DX_Values.Get_OpenTime(shift);
        }
    }
    public class ADX_Configuration
    {
        public ADX_Configuration(int _TF_Number, int _Period, MaType _Ma_Type)
        {
            TF_Number = _TF_Number;
            Period = _Period;
            Ma_Type = _Ma_Type;
        }
        public int TF_Number { get; private set; }
        public int Period { get; private set; }
        public MaType Ma_Type { get; private set; }
    }
}
