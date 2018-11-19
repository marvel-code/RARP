using System;
using System.Collections.Generic;
using System.Linq;

using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;

using Ecng.Collections;

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
        public Decimal vo_pp { get { return Get_VO(2); } }
        public Decimal bo { get { return Get_BO(); } }
        public Decimal bo_p { get { return Get_BO(1); } }
        public Decimal bo_pp { get { return Get_BO(2); } }
        public Decimal so { get { return Get_SO(); } }
        public Decimal so_p { get { return Get_SO(1); } }
        public Decimal so_pp { get { return Get_SO(2); } }
        // Вектора
        public Decimal vector { get { return Get_VectorVolume(); } }
        public Decimal vector_p { get { return Get_VectorVolume(1); } }
        public Decimal vector_pp { get { return Get_VectorVolume(2); } }
        public Decimal vector_h { get { return Get_VectorHigh(); } }
        public Decimal vector_hp { get { return Get_VectorHigh(1); } }
        public Decimal vector_hpp { get { return Get_VectorHigh(2); } }
        public Decimal vector_l { get { return Get_VectorLow(); } }
        public Decimal vector_lp { get { return Get_VectorLow(1); } }
        public Decimal vector_lpp { get { return Get_VectorLow(2); } }
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
        public Decimal act { get { return Get_Average_Candle_Total_Volume(); } }
        public Decimal act_p { get { return Get_Average_Candle_Total_Volume(1); } }
        public Decimal act_pp { get { return Get_Average_Candle_Total_Volume(2); } }
        public Decimal acb { get { return Get_Average_Candle_Buy_Volume(); } }
        public Decimal acb_p { get { return Get_Average_Candle_Buy_Volume(1); } }
        public Decimal acb_pp { get { return Get_Average_Candle_Buy_Volume(2); } }
        public Decimal acs { get { return Get_Average_Candle_Sell_Volume(); } }
        public Decimal acs_p { get { return Get_Average_Candle_Sell_Volume(1); } }
        public Decimal acs_pp { get { return Get_Average_Candle_Sell_Volume(2); } }
        public Decimal acv { get { return Get_Average_Candle_Vector(); } }
        public Decimal acv_p { get { return Get_Average_Candle_Vector(1); } }
        public Decimal acv_pp { get { return Get_Average_Candle_Vector(2); } }
        // Регистрация значений произвольных скоростей за период
        public Dictionary<int, Decimal[]> TVV_for_order_info;
        public Dictionary<int, Decimal[]> BVV_for_order_info;
        public Dictionary<int, Decimal[]> SVV_for_order_info;
        // Регистрация значений вектора для определения vector_h, vector_l
        private List<Decimal> vectors_h;
        private List<Decimal> vectors_l;
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

            
            // VECTOR HIGH & LOW
            if (vectors_h.Count != 0 && Buffer[Buffer.Count - 1].Time == _candle.Time)
            {
                if (vectors_h.Count > 5)
                {
                    vectors_h.RemoveAt(0);
                    vectors_l.RemoveAt(0);
                }

                var current_vector = vector;
                if (vectors_h.Last() < current_vector)
                    vectors_h[vectors_h.Count - 1] = current_vector;
                if (vectors_l.Last() > current_vector)
                    vectors_l[vectors_l.Count - 1] = current_vector;
            }
            else
            {
                vectors_h.Add(0);
                vectors_l.Add(0);
            }
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

            vectors_h = new List<decimal>();
            vectors_l = new List<decimal>();

            _bvCache = new Dictionary<int, Dictionary<int, decimal>>();
            _svCache = new Dictionary<int, Dictionary<int, decimal>>();
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
        public Decimal Get_VectorHigh(int shift = 0)
        {
            if (vectors_h.Count <= shift)
                return 0;

            return vectors_h[vectors_h.Count - 1 - shift];
        }
        public Decimal Get_VectorLow(int shift = 0)
        {
            if (vectors_l.Count <= shift)
                return 0;

            return vectors_l[vectors_l.Count - 1 - shift];
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
        public Decimal Get_Average_Candle_Vector(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
                return 0;

            return (Get_DirectionVolume(OrderDirections.Buy, shift) - Get_DirectionVolume(OrderDirections.Sell, shift)) / duration;
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
        public Decimal GetTv(int _Velocity_Period_Seconds, int shift = 0) // Get total volume velocity value
        {
            Decimal result = GetBv(_Velocity_Period_Seconds, shift) + GetSv(_Velocity_Period_Seconds, shift);

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
        public Decimal GetVv(int _Velocity_Period_Seconds, int shift = 0) // Get veco volume velocity value
        {
            Decimal result = GetBv(_Velocity_Period_Seconds, shift) - GetSv(_Velocity_Period_Seconds, shift);
            
            return result;
        }
        public Decimal GetBv(int _Velocity_Period_Seconds, int shift = 0) // Get buy volume velocity value
        {
            Decimal result = getCachedBvVal(_Velocity_Period_Seconds, shift, VCalcType.Shift);

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
        public Decimal GetSv(int _Velocity_Period_Seconds, int shift = 0) // Get sell volume velocity value
        {
            Decimal result = getCachedSvVal(_Velocity_Period_Seconds, shift, VCalcType.Shift);

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
        public Decimal GetAvrVvMax(int _Velocity_Period_Seconds, int shift = 0)
        {
            decimal left, mid, right;
            right = GetAvrVv(_Velocity_Period_Seconds, 0);
            mid = GetAvrVv(_Velocity_Period_Seconds, 1);
            left = GetAvrVv(_Velocity_Period_Seconds, 2);
            for (int i = 3; (left == mid && mid == right && right == 0) == false; i++)
            {
                if (mid > left && mid > right)
                {
                    shift--;
                    if (shift == -1)
                        break;
                }
                right = mid;
                mid = left;
                left = GetAvrVv(_Velocity_Period_Seconds, i);
            }

            return mid;
        }
        public Decimal GetAvrVvMin(int _Velocity_Period_Seconds, int shift = 0)
        {
            decimal left, mid, right;
            right = GetAvrVv(_Velocity_Period_Seconds, 0);
            mid = GetAvrVv(_Velocity_Period_Seconds, 1);
            left = GetAvrVv(_Velocity_Period_Seconds, 2);
            for (int i = 3; (left == mid && mid == right && right == 0) == false; i++)
            {
                if (mid < left && mid < right)
                {
                    shift--;
                    if (shift == -1)
                        break;
                }
                right = mid;
                mid = left;
                left = GetAvrVv(_Velocity_Period_Seconds, i);
            }

            return mid;
        }
        public Decimal GetAvrTv(int _Velocity_Period_Seconds, int shift = 0)
        {

            return GetAvrBv(_Velocity_Period_Seconds, shift) + GetAvrSv(_Velocity_Period_Seconds, shift);
        }
        public Decimal GetAvrVv(int _Velocity_Period_Seconds, int shift = 0)
        {
            return GetAvrBv(_Velocity_Period_Seconds, shift) - GetAvrSv(_Velocity_Period_Seconds, shift);
        }
        public Decimal GetAvrBv(int _Velocity_Period_Seconds, int shift = 0)
        {
            var curCandle = Get_Candle();
            if (curCandle == null)
                return 0;
            int currentDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond = currentDaySecond - currentDaySecond % VV_TACT - shift * VV_TACT;

            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedBvVal(_Velocity_Period_Seconds, calcDaySecond - i, VCalcType.DaySecond);
            }
            result /= VV_TACT;

            // Output
            return result;
        }
        public Decimal GetAvrSv(int _Velocity_Period_Seconds, int shift = 0)
        {
            var curCandle = Get_Candle();
            if (curCandle == null)
                return 0;
            int currentDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond = currentDaySecond - currentDaySecond % VV_TACT - shift * VV_TACT;

            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedSvVal(_Velocity_Period_Seconds, calcDaySecond - i, VCalcType.DaySecond);
            }
            result /= VV_TACT;

            // Output
            return result;
        }
        // Технический код рассчёта скоростей (не для использования во вне)
        private const int VV_TACT = MySettings.VOL_VV_STD_TACT;
        private enum VCalcType { Shift, DaySecond }
        private const int maxCacheTimeDelta = 5000;
        private Dictionary<int, Dictionary<int, decimal>> _bvCache; // Dict<period, Dict<daySecond, value>
        private Dictionary<int, Dictionary<int, decimal>> _svCache;
        private decimal getCachedTvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            return getCachedBvVal(period, arg, vCalcType) + getCachedSvVal(period, arg, vCalcType);
        }
        private decimal getCachedVvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            return getCachedBvVal(period, arg, vCalcType) - getCachedSvVal(period, arg, vCalcType);
        }
        private decimal getCachedBvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            var curCandle = Get_Candle();
            if (curCandle == null)
            {
                return 0;
            }
            decimal result = 0;
            int curDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond;
            int delta;
            switch (vCalcType)
            {
                case VCalcType.Shift:
                    calcDaySecond = curDaySecond - arg;
                    delta = arg;
                    break;
                case VCalcType.DaySecond:
                    calcDaySecond = arg;
                    delta = curDaySecond - arg;
                    break;
                default:
                    calcDaySecond = curDaySecond - arg;
                    delta = arg;
                    break;
            }

            // Get past value
            if (delta > 0)
            {
                if (_bvCache.ContainsKey(period) && _bvCache[period].ContainsKey(calcDaySecond))
                {
                    result = _bvCache[period][calcDaySecond];
                }
                else
                {
                    result = getRealBvVal(period, arg, vCalcType);
                }
            }

            // Get real value
            if (delta == 0)
            {
                result = getRealBvVal(period, arg, vCalcType);
            }

            return result;
        }
        private decimal getCachedSvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            var curCandle = Get_Candle();
            if (curCandle == null)
            {
                return 0;
            }
            decimal result = 0;
            int curDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond;
            int delta;
            switch (vCalcType)
            {
                case VCalcType.Shift:
                    calcDaySecond = curDaySecond - arg;
                    delta = arg;
                    break;
                case VCalcType.DaySecond:
                    calcDaySecond = arg;
                    delta = curDaySecond - arg;
                    break;
                default:
                    calcDaySecond = curDaySecond - arg;
                    delta = arg;
                    break;
            }

            // Get past value
            if (delta > 0)
            {
                if (_svCache.ContainsKey(period) && _svCache[period].ContainsKey(calcDaySecond))
                {
                    result = _svCache[period][calcDaySecond];
                }
                else
                {
                    result = getRealSvVal(period, arg, vCalcType);
                }
            }

            // Get real value
            if (delta == 0)
            {
                result = getRealSvVal(period, arg, vCalcType);
            }

            return result;
        }
        private decimal getRealTvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            return getRealBvVal(period, arg, vCalcType) + getRealSvVal(period, arg, vCalcType);
        }
        private decimal getRealVvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            return getRealBvVal(period, arg, vCalcType) - getRealSvVal(period, arg, vCalcType);
        }
        private decimal getRealBvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            // Settings
            var side = OrderDirections.Buy;

            // Prepare
            if (_bvCache.ContainsKey(period) == false)
            {
                _bvCache.Add(period, new Dictionary<int, decimal>());
            }
            var curCandle = Get_Candle();
            if (curCandle == null)
                return 0;
            IEnumerable<Trade> AllTrades;
            try
            {
                AllTrades = _processingData.AllTrades;
            }
            catch
            {
                return 0;
            }
            int currentDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond;
            switch (vCalcType)
            {
                case VCalcType.Shift:
                    calcDaySecond = currentDaySecond - arg;
                    break;
                case VCalcType.DaySecond:
                    calcDaySecond = arg;
                    break;
                default:
                    calcDaySecond = currentDaySecond - arg;
                    break;
            }

            int tradesCount = AllTrades.Count();
            int k, k_last;
            for (k = 0; k < tradesCount && AllTrades.ElementAtFromEnd(k).Time.TimeOfDay.TotalSeconds > calcDaySecond; k++) ; // Omit late trades
            for (k_last = k; k_last < tradesCount && AllTrades.ElementAtFromEnd(k_last).Time.TimeOfDay.TotalSeconds >= calcDaySecond - period; k_last++) ; // Set last including trade index
            k_last--;

            // Calculate
            decimal result = 0;
            Trade t;
            for (; k <= k_last; k++)
            {
                t = AllTrades.ElementAtFromEnd(k);
                if (t.OrderDirection == side) result += t.Volume;
            }
            result /= period;

            // Cache
            if (_bvCache[period].ContainsKey(calcDaySecond))
            {
                _bvCache[period][calcDaySecond] = result;
            }
            else
            {
                _bvCache[period].Add(calcDaySecond, result);

                if (_bvCache[period].Count > maxCacheTimeDelta)
                {
                    _bvCache[period].RemoveWhere(x => x.Key < currentDaySecond - maxCacheTimeDelta);
                }
            }

            return result;
        }
        private decimal getRealSvVal(int period, int arg = 0, VCalcType vCalcType = VCalcType.Shift)
        {
            // Settings
            var side = OrderDirections.Sell;

            // Prepare
            if (_svCache.ContainsKey(period) == false)
            {
                _svCache.Add(period, new Dictionary<int, decimal>());
            }
            var curCandle = Get_Candle();
            if (curCandle == null)
                return 0;
            IEnumerable<Trade> AllTrades;
            try
            {
                AllTrades = _processingData.AllTrades;
            }
            catch
            {
                return 0;
            }
            int currentDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond;
            switch (vCalcType)
            {
                case VCalcType.Shift:
                    calcDaySecond = currentDaySecond - arg;
                    break;
                case VCalcType.DaySecond:
                    calcDaySecond = arg;
                    break;
                default:
                    calcDaySecond = currentDaySecond - arg;
                    break;
            }
            int tradesCount = AllTrades.Count();
            int k, k_last;
            for (k = 0; k < tradesCount && AllTrades.ElementAtFromEnd(k).Time.TimeOfDay.TotalSeconds > calcDaySecond; k++) ; // Omit late trades
            for (k_last = k; k_last < tradesCount && AllTrades.ElementAtFromEnd(k_last).Time.TimeOfDay.TotalSeconds >= calcDaySecond - period; k_last++) ; // Set last including trade index
            k_last--;

            // Calculate
            decimal result = 0;
            Trade t;
            for (; k <= k_last; k++)
            {
                t = AllTrades.ElementAtFromEnd(k);
                if (t.OrderDirection == side) result += t.Volume;
            }
            result /= period;

            // Cache
            if (_svCache[period].ContainsKey(calcDaySecond))
            {
                _svCache[period][calcDaySecond] = result;
            }
            else
            {
                _svCache[period].Add(calcDaySecond, result);

                if (_svCache[period].Count > maxCacheTimeDelta)
                {
                    _svCache[period].RemoveWhere(x => x.Key < currentDaySecond - maxCacheTimeDelta);
                }
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
            var candle = Get_Candle(shift);
            if (candle == null)
                return 0;

            var OpenTime = candle.Time;
            var CloseTime = _processingData.TerminalTime;
            if (shift > 0)
                CloseTime = Get_Candle(shift - 1).Time;
                
            return (decimal)(CloseTime - OpenTime).TotalSeconds;
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
