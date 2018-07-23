using System;
using System.Collections.Generic;
using System.Linq;
using StockSharp.Algo.Candles;

namespace stocksharp
{
    public class BBW : Indicator
    {
        // Рабочие значения
        public Decimal val { get { return Get_Value(); } }
        public Decimal val_p { get { return Get_Value(1); } }

        // Параметры
        public MaType Ma_Type { get; private set; }
        public CalculationType Calc_Type { get; private set; }
        public Decimal Deviation { get; private set; }
        public int Period { get; private set; }
        // Массивы значений
        private List<DecimalIndicatorValue> Values;
        private List<DecimalIndicatorValue> HighBand_Values;
        private List<DecimalIndicatorValue> LowBand_Values;

        private MA MA_Values;
        // Инициализация
        public BBW(int _Period, Decimal _Deviation, CalculationType _Calc_Type, MaType _Ma_Type)
        {
            Period = _Period;
            Deviation = _Deviation;
            Calc_Type = _Calc_Type;
            Ma_Type = _Ma_Type;

            Reset();
        }
        // Обновление значений
        public void Update(List<Candle> _Buffer)
        {
            Candle candle = _Buffer.Count == 0 ? null : _Buffer[_Buffer.Count - 1];

            if (candle == null)
                return;

            // Если не новое значение, обновляем
            if (Values.Count != 0 && Values[Values.Count - 1].OpenTime == candle.Time)
            {
                HighBand_Values.RemoveAt(HighBand_Values.Count - 1);
                LowBand_Values.RemoveAt(LowBand_Values.Count - 1);
                Values.RemoveAt(Values.Count - 1);
            }
            // Мусор
            else if (Values.Count > Period)
            {
                HighBand_Values.RemoveAt(0);
                LowBand_Values.RemoveAt(0);
                Values.RemoveAt(0);
            }
            // Обновляем MA
            MA_Values.Update(candle);
            // Считаем StDev
            Decimal SUM = 0;
            Decimal Price;
            Decimal MA_Value = MA_Values.Get_Value();
            if (MA_Values.Buffer.Count >= Period)
                for (int i = 0; i < Period; i++)
                {
                    Price = Get_Calced_Value(MA_Values.Buffer[MA_Values.Buffer.Count - 1 - i], Calc_Type);
                    SUM += (Price - MA_Value) * (Price - MA_Value);
                }
            Decimal StDev = (decimal)Math.Sqrt((double)(SUM / Period));
            // Обновляем значения BB
            Decimal k = Deviation;
            HighBand_Values.Add(new DecimalIndicatorValue(candle.Time, MA_Values.Get_Value() + k * StDev));
            LowBand_Values.Add(new DecimalIndicatorValue(candle.Time, MA_Values.Get_Value() - k * StDev));
            // Обновляем значение BBW
            Values.Add(new DecimalIndicatorValue(candle.Time, 2 * k * StDev));
        }
        public void Reset()
        {
            Values = new List<DecimalIndicatorValue>();
            HighBand_Values = new List<DecimalIndicatorValue>();
            LowBand_Values = new List<DecimalIndicatorValue>();

            MA_Values = new MA(Period, Ma_Type, Calc_Type);
        }
        // Ширина между HighBand и LowBand
        public Decimal Get_Value(int shift = 0)
        {
            if (Values.Count <= shift)
                return 0;
            else
                return Values[Values.Count - 1 - shift].Value;
        }
        // Время открытия свечи для значения
        public DateTime Get_OpenTime(Byte shift = 0)
        {
            if (Values.Count <= shift)
                return DateTime.Now.AddYears(-1);
            else
                return Values[Values.Count - 1 - shift].OpenTime;
        }
    }
    public class BBW_Configuration
    {
        public BBW_Configuration(int _TF_Number, int _Period, Decimal _Deviation, CalculationType _Calc_Type, MaType _Ma_Type)
        {
            TF_Number = _TF_Number;
            Period = _Period;
            Deviation = _Deviation;
            Ma_Type = _Ma_Type;
            Calc_Type = _Calc_Type;
        }
        public int TF_Number { get; private set; }
        public int Period { get; private set; }
        public Decimal Deviation { get; private set; }
        public MaType Ma_Type { get; private set; }
        public CalculationType Calc_Type { get; private set; }
    }
}
