using System;
using System.Collections.Generic;
using System.Linq;

using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;

namespace stocksharp
{
    public class Vol
    {
        /**
         * Рабочие значения
         **/
        // Осциляторы
        public Decimal vo { get { return Get_VO(); } }
        public Decimal vo_p { get { return Get_VO(1); } }
        public Decimal bo { get { return Get_BO(); } }
        public Decimal bo_p { get { return Get_BO(1); } }
        public Decimal so { get { return Get_SO(); } }
        public Decimal so_p { get { return Get_SO(1); } }
        // Вектора
        public Decimal vector { get { return Get_VectorVolume(); } }
        public Decimal vector_p { get { return Get_VectorVolume(1); } }
        // Объемы текущих одиночных свеч
        public Decimal total { get { return Get_TotalVolume(); } }
        public Decimal total_p { get { return Get_TotalVolume(1); } }
        public Decimal total_pp { get { return Get_TotalVolume(2); } }
        public Decimal buy { get { return Get_DirectionVolume(OrderDirections.Buy); } }
        public Decimal buy_p { get { return Get_DirectionVolume(OrderDirections.Buy, 1); } }
        public Decimal buy_pp { get { return Get_DirectionVolume(OrderDirections.Buy, 2); } }
        public Decimal sell { get { return Get_DirectionVolume(OrderDirections.Sell); } }
        public Decimal sell_p { get { return Get_DirectionVolume(OrderDirections.Sell, 1); } }
        public Decimal sell_pp { get { return Get_DirectionVolume(OrderDirections.Sell, 2); } }
        // Средняя скорость за период
        public Decimal tv { get { return Get_TotalVolume_Velocity(); } }
        public Decimal tv_p { get { return Get_TotalVolume_Velocity(1); } }
        public Decimal bv { get { return Get_DirectionVolume_Velocity(OrderDirections.Buy); } }
        public Decimal bv_p { get { return Get_DirectionVolume_Velocity(OrderDirections.Buy, 1); } }
        public Decimal sv { get { return Get_DirectionVolume_Velocity(OrderDirections.Sell); } }
        public Decimal sv_p { get { return Get_DirectionVolume_Velocity(OrderDirections.Sell, 1); } }
        public Decimal vv { get { return Get_VectorVolume_Velocity(); } }
        public Decimal vv_p { get { return Get_VectorVolume_Velocity(1); } }
        // Средняя скорость за свечу
        public Decimal actv { get { return Get_Average_Candle_Total_Volume(); } }
        public Decimal actv_p { get { return Get_Average_Candle_Total_Volume(1); } }
        public Decimal acbv { get { return Get_Average_Candle_Buy_Volume(); } }
        public Decimal acbv_p { get { return Get_Average_Candle_Buy_Volume(1); } }
        public Decimal acsv { get { return Get_Average_Candle_Sell_Volume(); } }
        public Decimal acsv_p { get { return Get_Average_Candle_Sell_Volume(1); } }
        // Регистрация значений произвольных скоростей за период
        public Dictionary<int, Decimal[]> TVV_for_order_info;
        public Dictionary<int, Decimal[]> BVV_for_order_info;
        public Dictionary<int, Decimal[]> SVV_for_order_info;
        /**
         * Настройки
         **/
        private ServiceContracts.ProcessingData _processingData;
        // Параметры
        public int Long_Period { get; private set; }
        public int Short_Period { get; private set; }
        public int Velocity_Period_Seconds { get; private set; }
        // Массив свечек и сделок
        public List<Candle> Buffer;
        // Массивы значений
        private List<DecimalIndicatorValue> Values_BuyVolume;
        private List<DecimalIndicatorValue> Values_SellVolume;
        // Инициализация
        public Vol(ServiceContracts.ProcessingData processingData, int _Long_Period, int _Short_Period, int _Velocity_Period_Seconds)
        {
            _processingData = processingData;

            Long_Period = _Long_Period;
            Short_Period = _Short_Period;
            Velocity_Period_Seconds = _Velocity_Period_Seconds;

            Reset();
        }
        // Обновление значений
        public void Update(Candle _candle)
        {
            if (_candle == null)
                return;

            // Если текущая, заменяем
            if (Buffer.Count != 0 && Buffer[Buffer.Count - 1].Time == _candle.Time)
                Buffer.RemoveAt(Buffer.Count - 1);
            // Если избыток
            if (Buffer.Count > Long_Period + Short_Period)
                Buffer.RemoveAt(0);

            Buffer.Add(_candle);
        }
        // Стереть все значения
        public void Reset()
        {
            Values_BuyVolume = new List<DecimalIndicatorValue>();
            Values_SellVolume = new List<DecimalIndicatorValue>();
            Buffer = new List<Candle>();

            TVV_for_order_info = new Dictionary<int, Decimal[]>();
            BVV_for_order_info = new Dictionary<int, Decimal[]>();
            SVV_for_order_info = new Dictionary<int, Decimal[]>();
        }
        // Осциляторы
        public Decimal Get_VO(int shift = 0)
        {
            Decimal Long_Sum = Get_Sum_Period_TotalVolume(Long_Period, Short_Period + shift);
            Decimal Short_Sum = Get_Sum_Period_TotalVolume(Short_Period, shift);
            return Long_Sum == 0 ? 0 : (Short_Sum / Short_Period * Long_Period / Long_Sum - 1) * 100;
        }
        public Decimal Get_BO(int shift = 0)
        {
            Decimal Long_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Buy, Long_Period, Short_Period + shift);
            Decimal Short_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Buy, Short_Period, shift);
            return Long_Sum == 0 ? 0 : (Short_Sum / Short_Period * Long_Period / Long_Sum - 1) * 100;
        }
        public Decimal Get_SO(int shift = 0)
        {
            Decimal Long_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Sell, Long_Period, Short_Period + shift);
            Decimal Short_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Sell, Short_Period, shift);
            return Long_Sum == 0 ? 0 : (Short_Sum / Short_Period * Long_Period / Long_Sum - 1) * 100;
        }
        // Вектор
        public Decimal Get_VectorVolume(int shift = 0)
        {
            return Get_DirectionVolume(OrderDirections.Buy, shift) - Get_DirectionVolume(OrderDirections.Sell, shift);
        }
        // Объемы свечи
        public Decimal Get_TotalVolume(int shift = 0)
        {
            if (Buffer.Count <= shift)
                return 0;

            return Buffer[Buffer.Count - 1 - shift].TotalVolume;
        }
        public Decimal Get_DirectionVolume(OrderDirections _Order_Direction, int shift = 0)
        {
            var AllTrades = _processingData.AllTrades;

            // Если недостаточно данных
            if (Buffer.Count <= 1 || Buffer.Count <= shift || AllTrades.Count == 0)
                return 0;

            Decimal result = 0;
            Trade t;
            TimeSpan TF_Period = Buffer[1].Time - Buffer[0].Time;
            Candle candle = Buffer[Buffer.Count - 1 - shift];
            for (int i = 0; i < AllTrades.Count - shift; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более ранней свече
                if (t.Time < candle.Time)
                    break;
                // Если сделка принадлежит более поздней свече
                if (t.Time >= candle.Time.Add(TF_Period))
                    continue;
                // Учитываем направление
                if (t.OrderDirection == _Order_Direction)
                    result += t.Volume;
            }

            return result;
        }
        // Суммарные объемы свеч
        public Decimal Get_Sum_Period_TotalVolume(int _Candles_Count, int shift = 0)
        {
            if (Buffer.Count < shift + _Candles_Count)
                return 0;

            Decimal result = 0;
            for (int i = 0; i < _Candles_Count; i++)
                result += Buffer[Buffer.Count - 1 - i - shift].TotalVolume;

            return result;
        }
        public Decimal Get_Sum_Period_DirectionVolume(OrderDirections _Order_Direction, int _Candles_Count, int shift = 0)
        {
            Decimal result = 0;
            for (int i = 0; i < _Candles_Count; i++)
                result += Get_DirectionVolume(_Order_Direction, i + shift);

            return result;
        }
        // Средние объемы свеч
        public Decimal Get_Average_Candle_Total_Volume(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
                return 0;

            return Get_TotalVolume(shift) / duration;
        }
        public Decimal Get_Average_Candle_Buy_Volume(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
                return 0;

            return Get_DirectionVolume(OrderDirections.Buy, shift) / duration;
        }
        public Decimal Get_Average_Candle_Sell_Volume(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
                return 0;

            return Get_DirectionVolume(OrderDirections.Sell, shift) / duration;
        }
        // Скорости
        public Decimal Get_TotalVolume_Velocity(int shift = 0)
        {
            return Get_DirectionVolume_Velocity(OrderDirections.Buy, shift) + Get_DirectionVolume_Velocity(OrderDirections.Sell, shift);
        }
        public Decimal Get_VectorVolume_Velocity(int shift = 0)
        {
            return Get_DirectionVolume_Velocity(OrderDirections.Buy, shift) - Get_DirectionVolume_Velocity(OrderDirections.Sell, shift);
        }
        public Decimal Get_DirectionVolume_Velocity(OrderDirections _Order_Direction, int shift = 0)
        {
            var AllTrades = _processingData.AllTrades;

            if (AllTrades.Count == 0)
                return 0;

            Trade t;
            int _Velocity_Period_Seconds = Velocity_Period_Seconds;
            DateTimeOffset _Current_Time = _processingData.TerminalTime;
            Decimal result = 0;
            for (int i = 0; i < AllTrades.Count; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более раннему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * (shift + 1)) < _Current_Time)
                    break;
                // Если сделка принадлежит более позднему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * shift) >= _Current_Time.AddSeconds((double)_Velocity_Period_Seconds))
                    continue;
                if (t.OrderDirection == _Order_Direction)
                    result += t.Volume;
            }
            result /= _Velocity_Period_Seconds;

            return result;
        }

        public Decimal Get_TVV_Value(int _Velocity_Period_Seconds, int shift = 0) // Get total volume velocity value
        {
            Decimal result = Get_BVV_Value(_Velocity_Period_Seconds, shift) + Get_SVV_Value(_Velocity_Period_Seconds, shift);

            // For order info
            if (shift < 2)
            {
                if (!TVV_for_order_info.ContainsKey(_Velocity_Period_Seconds))
                {
                    TVV_for_order_info.Add(_Velocity_Period_Seconds, new Decimal[2]);
                }
                TVV_for_order_info[_Velocity_Period_Seconds][shift] = result;
            }

            return result;
        }
        public Decimal Get_BVV_Value(int _Velocity_Period_Seconds, int shift = 0) // Get buy volume velocity value
        {
            var AllTrades = _processingData.AllTrades;

            if (AllTrades.Count == 0)
                return 0;

            Trade t;
            DateTimeOffset _Current_Time = _processingData.TerminalTime;
            OrderDirections _Order_Direction = OrderDirections.Buy;
            Decimal result = 0;
            for (int i = 0; i < AllTrades.Count; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более раннему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * (shift + 1)) < _Current_Time)
                    break;
                // Если сделка принадлежит более позднему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * shift) >= _Current_Time.AddSeconds(_Velocity_Period_Seconds))
                    continue;
                if (t.OrderDirection == _Order_Direction)
                    result += t.Volume;
            }
            result /= _Velocity_Period_Seconds;

            // For order info
            if (shift < 2)
            {
                if (!BVV_for_order_info.ContainsKey(_Velocity_Period_Seconds))
                {
                    BVV_for_order_info.Add(_Velocity_Period_Seconds, new Decimal[2]);
                }
                BVV_for_order_info[_Velocity_Period_Seconds][shift] = result;
            }

            return result;
        }
        public Decimal Get_SVV_Value(int _Velocity_Period_Seconds, int shift = 0) // Get sell volume velocity value
        {
            var AllTrades = _processingData.AllTrades;

            if (AllTrades.Count == 0)
                return 0;

            Trade t;
            DateTimeOffset _Current_Time = _processingData.TerminalTime;
            OrderDirections _Order_Direction = OrderDirections.Sell;
            Decimal result = 0;
            for (int i = 0; i < AllTrades.Count; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более раннему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * (shift + 1)) < _Current_Time)
                    break;
                // Если сделка принадлежит более позднему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * shift) >= _Current_Time.AddSeconds(_Velocity_Period_Seconds))
                    continue;
                if (t.OrderDirection == _Order_Direction)
                    result += t.Volume;
            }
            result /= _Velocity_Period_Seconds;

            // For order info
            if (shift < 2)
            {
                if (!SVV_for_order_info.ContainsKey(_Velocity_Period_Seconds))
                {
                    SVV_for_order_info.Add(_Velocity_Period_Seconds, new Decimal[2]);
                }
                SVV_for_order_info[_Velocity_Period_Seconds][shift] = result;
            }

            return result;
        }
        // Время открытия свечи
        public DateTimeOffset Get_OpenTime(int shift = 0)
        {
            if (Buffer.Count <= shift)
                return DateTime.Now.AddYears(-1);
            else
                return Buffer[Buffer.Count - 1 - shift].Time;
        }
        public Candle Get_Candle(int shift = 0)
        {
            return Buffer[Buffer.Count - 1 - shift];
        }
        public Decimal Get_Candle_Duration(int shift = 0)
        {
            var candle = Get_Candle();
            if (candle == null)
                return 1;

            Decimal result = (decimal)(_processingData.TerminalTime - candle.Time).TotalSeconds;

            return result;
        }
    }
    public class Volume_Configuration
    {
        public Volume_Configuration(int _TF_Number, int _Long_Period, int _Short_Period, int _Velocity_Period_Seconds)
        {
            TF_Number = _TF_Number;
            Long_Period = _Long_Period;
            Short_Period = _Short_Period;
            Velocity_Period_Seconds = _Velocity_Period_Seconds;
        }
        public int TF_Number { get; private set; }
        public int Long_Period { get; private set; }
        public int Short_Period { get; private set; }
        public int Velocity_Period_Seconds { get; private set; }
    }
}