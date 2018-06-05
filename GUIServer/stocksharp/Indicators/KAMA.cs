using System;
using System.Collections.Generic;

using StockSharp.Algo.Candles;

namespace stocksharp
{
    public class KAMA : Indicator
    {
        // Рабочие значения
        public Decimal val { get { return Get_Value(); } }
        public Decimal val_p { get { return Get_Value(1); } }
        public Decimal val_pp { get { return Get_Value(2); } }

        // Параметры
        public CalculationType Calc_Type { get; private set; }
        public int Period { get; private set; }
        public Decimal FN { get; private set; }
        public Decimal SN { get; private set; }
        // Массивы значений
        private List<DecimalIndicatorValue> Values;
        // Инициализация
        public KAMA(int _Period, CalculationType _Calc_Type, Decimal _FN, Decimal _SN)
        {
            Period = _Period;
            Calc_Type = _Calc_Type;
            FN = _FN;
            SN = _SN;

            Reset();
        }
        // Обновление значений
        public void Update(List<Candle> _Buffer)
        {
            Candle candle = _Buffer.Count == 0 ? null : _Buffer[_Buffer.Count - 1];
            Candle candle_period = _Buffer.Count <= Period ? null : _Buffer[_Buffer.Count - 1 - Period];

            if (candle == null)
                return;

            // Если текущая, заменяем
            if (Values.Count != 0 && Values[Values.Count - 1].OpenTime == candle.Time)
                Values.RemoveAt(Values.Count - 1);
            // Если избыток
            if (Values.Count > Period)
                Values.RemoveAt(0);

            // Считаем значение
            DecimalIndicatorValue New_Value = new DecimalIndicatorValue(candle.Time, Get_Calced_Value(candle, Calc_Type));
            if (Values.Count >= Period)
            {
                Decimal fSC = 2 / (FN + 1);
                Decimal sSC = 2 / (SN + 1);
                Decimal Signal = Math.Abs(Get_Calced_Value(candle, Calc_Type) - Get_Calced_Value(candle_period, Calc_Type));
                Decimal Noise = 0;
                for (int i = _Buffer.Count - Period; i < _Buffer.Count; i++)
                    Noise += Math.Abs(Get_Calced_Value(_Buffer[i], Calc_Type) - Get_Calced_Value(_Buffer[i - 1], Calc_Type));
                Decimal ER = Noise == 0 ? 0 : Signal / Noise;
                Decimal SSC = ER * (fSC - sSC) + sSC;
                Decimal Value_p = Get_Value();
                Decimal Current_Price = Get_Calced_Value(candle, Calc_Type);

                New_Value.Value = Value_p + SSC * SSC * (Current_Price - Value_p);
            }
            // Обновляем значения
            Values.Add(New_Value);
        }
        public void Reset()
        {
            Values = new List<DecimalIndicatorValue>();
        }
        // Взять значение
        public Decimal Get_Value(int shift = 0)
        {
            if (Values.Count <= shift)
                return 0;
            else
                return Values[Values.Count - 1 - shift].Value;
        }
        // Время открытия свечи для значения
        public DateTime Get_OpenTime(int shift = 0)
        {
            if (Values.Count <= shift)
                return DateTime.Now.AddYears(-1);
            else
                return Values[Values.Count - 1 - shift].OpenTime;
        }
    }
    public class KAMA_Configuration
    {
        public KAMA_Configuration(int _TF_Number, int _Period, CalculationType _Calc_Type, Decimal _FN, Decimal _SN)
        {
            TF_Number = _TF_Number;
            Period = _Period;
            Calc_Type = _Calc_Type;
            FN = _FN;
            SN = _SN;
        }
        public int TF_Number { get; private set; }
        public int Period { get; private set; }
        public CalculationType Calc_Type { get; private set; }
        public Decimal FN { get; private set; }
        public Decimal SN { get; private set; }
    }
}
