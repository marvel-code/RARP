using System;
using System.Collections.Generic;
using System.Linq;

using StockSharp.Algo.Candles;

namespace stocksharp
{
    public class MA : Indicator
    {
        // Массив значений
        public List<DecimalIndicatorValue> Values;
        public Decimal val { get { return Get_Value(); } }
        public Decimal val_p { get { return Get_Value(1); } }
        // Буффер свечек
        public List<Candle> Buffer;
        // Параметры
        public int Values_Count { get { return Values.Count; } }
        public int Period { get; private set; }
        public MaType Ma_Type { get; private set; }
        public CalculationType Calc_Type { get; private set; }
        // Инициализация
        public MA(int _Period, MaType _Ma_Type, CalculationType _Calc_type)
        {
            Period = _Period;
            Ma_Type = _Ma_Type;
            Calc_Type = _Calc_type;

            Reset();
        }
        // Обновление значения
        public void Update(Candle _candle)
        {
            if (_candle == null)
                throw new ArgumentNullException("candle");
            // Если времена открытия совпадают
            if (Values.Count != 0 && _candle.Time == Values[Values.Count - 1].OpenTime)
            {   // Удаляем значение для последующей замены
                Values.RemoveAt(Values.Count - 1);
                Buffer.RemoveAt(Buffer.Count - 1);
            }
            // Удаляем избыточные значения
            if (Values.Count > Period)
            {
                Values.RemoveAt(0);
                Buffer.RemoveAt(0);
            }
            // Добавляем свечку в буффер
            Buffer.Add(_candle);

            // Новое значение
            decimal new_Value = 0;
            // Текущее значение
            new_Value = Get_Calced_Value(_candle, Calc_Type);
            // Значение по периоду
            switch (Ma_Type)
            {
                case MaType.Exponential:
                    if (Values.Count >= Period)
                        new_Value = (Values[Values.Count - 1].Value * (Period - 1) + 2 * new_Value) / (Period + 1);
                    break;
                case MaType.Simple:
                    if (Buffer.Count >= Period)
                    {
                        // Перебираем свечи
                        for (int i = 2; i <= Period; i++)
                            new_Value += Get_Calced_Value(Buffer[Buffer.Count - i], Calc_Type);
                        new_Value /= Period;
                    }
                    break;
            }
            // Добавляем значение в массив
            Values.Add(new DecimalIndicatorValue(_candle.Time, new_Value));
        }
        public void Reset()
        {
            Values = new List<DecimalIndicatorValue>();
            Buffer = new List<Candle>();
        }
        // Значения индикатора
        public decimal Get_Value(int shift = 0)
        {
            if (Values.Count > shift)
                return Values[Values.Count - 1 - shift].Value;
            else
                return 0;
        }
        // Время открытия свечи индикатора
        public DateTime Get_OpenTime(int shift = 0)
        {
            if (Values.Count > shift)
                return Values[Values.Count - 1 - shift].OpenTime;
            else
                return DateTime.Now.AddYears(-1);
        }
    }
    public class MA_Configuration
    {
        public MA_Configuration(int _TF_Number, int _Period, MaType _Ma_Type, CalculationType _Calc_type)
        {
            TF_Number = _TF_Number;
            Period = _Period;
            Ma_Type = _Ma_Type;
            Calc_Type = _Calc_type;
        }
        public int TF_Number { get; private set; }
        public int Period { get; private set; }
        public MaType Ma_Type { get; private set; }
        public CalculationType Calc_Type { get; private set; }
    }
}
