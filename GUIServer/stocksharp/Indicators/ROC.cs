using System;
using System.Collections.Generic;

using StockSharp.Algo.Candles;

namespace stocksharp
{
    public class ROC : Indicator
    {
        // Рабочие значения
        public Decimal val { get { return Get_Value(); } }
        public Decimal val_p { get { return Get_Value(1); } }
        public Decimal abs { get { return Math.Abs(Get_Value()); } }
        public Decimal abs_p { get { return Math.Abs(Get_Value(1)); } }

        // Параметры
        public CalculationType Calc_Type { get; private set; }
        public int Period { get; private set; }
        // Массивы значений
        private List<DecimalIndicatorValue> Values;
        // Инициализация
        public ROC(int _Period, CalculationType _Calc_Type)
        {
            Period = _Period;
            Calc_Type = _Calc_Type;

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
            DecimalIndicatorValue New_Value = new DecimalIndicatorValue(candle.Time, 0);
            if (candle_period != null)
                New_Value.Value = (1 - Get_Calced_Value(candle_period, Calc_Type) / Get_Calced_Value(candle, Calc_Type)) * 100;
            // Обновляем значения
            Values.Add(New_Value);
        }
        // Стереть все значения
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
    public class ROC_Configuration
    {
        public ROC_Configuration(int _TF_Number, int _Period, CalculationType _Calc_Type)
        {
            TF_Number = _TF_Number;
            Period = _Period;
            Calc_Type = _Calc_Type;
        }
        public int TF_Number { get; private set; }
        public int Period { get; private set; }
        public CalculationType Calc_Type { get; private set; }
    }
}
