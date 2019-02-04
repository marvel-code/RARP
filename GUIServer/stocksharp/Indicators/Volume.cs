using System;
using System.Collections.Generic;
using System.Linq;
using Ecng.Collections;
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
        public decimal vo => Get_VO();
        public decimal vo_p => Get_VO(1);
        public decimal vo_pp => Get_VO(2);
        public decimal bo => Get_BO();
        public decimal bo_p => Get_BO(1);
        public decimal bo_pp => Get_BO(2);
        public decimal so => Get_SO();
        public decimal so_p => Get_SO(1);
        public decimal so_pp => Get_SO(2);
        // Вектора
        public decimal vector => Get_VectorVolume();
        public decimal vector_p => Get_VectorVolume(1);
        public decimal vector_pp => Get_VectorVolume(2);
        public decimal vector_h => Get_VectorHigh();
        public decimal vector_hp => Get_VectorHigh(1);
        public decimal vector_hpp => Get_VectorHigh(2);
        public decimal vector_l => Get_VectorLow();
        public decimal vector_lp => Get_VectorLow(1);
        public decimal vector_lpp => Get_VectorLow(2);
        // Объемы текущих одиночных свеч
        public decimal total => Get_TotalVolume();
        public decimal total_p => Get_TotalVolume(1);
        public decimal total_pp => Get_TotalVolume(2);
        public decimal buy => Get_DirectionVolume(OrderDirections.Buy);
        public decimal buy_p => Get_DirectionVolume(OrderDirections.Buy, 1);
        public decimal buy_pp => Get_DirectionVolume(OrderDirections.Buy, 2);
        public decimal sell => Get_DirectionVolume(OrderDirections.Sell);
        public decimal sell_p => Get_DirectionVolume(OrderDirections.Sell, 1);
        public decimal sell_pp => Get_DirectionVolume(OrderDirections.Sell, 2);
        // Средняя скорость за период
        public decimal tv => Get_TotalVolume_Velocity();
        public decimal tv_p => Get_TotalVolume_Velocity(1);
        public decimal bv => Get_DirectionVolume_Velocity(OrderDirections.Buy);
        public decimal bv_p => Get_DirectionVolume_Velocity(OrderDirections.Buy, 1);
        public decimal sv => Get_DirectionVolume_Velocity(OrderDirections.Sell);
        public decimal sv_p => Get_DirectionVolume_Velocity(OrderDirections.Sell, 1);
        public decimal vv => Get_VectorVolume_Velocity();
        public decimal vv_p => Get_VectorVolume_Velocity(1);
        // Средняя скорость за свечу
        public decimal act => Get_Average_Candle_Total_Volume();
        public decimal act_p => Get_Average_Candle_Total_Volume(1);
        public decimal act_pp => Get_Average_Candle_Total_Volume(2);
        public decimal acb => Get_Average_Candle_Buy_Volume();
        public decimal acb_p => Get_Average_Candle_Buy_Volume(1);
        public decimal acb_pp => Get_Average_Candle_Buy_Volume(2);
        public decimal acs => Get_Average_Candle_Sell_Volume();
        public decimal acs_p => Get_Average_Candle_Sell_Volume(1);
        public decimal acs_pp => Get_Average_Candle_Sell_Volume(2);
        public decimal acv => Get_Average_Candle_Vector();
        public decimal acv_p => Get_Average_Candle_Vector(1);
        public decimal acv_pp => Get_Average_Candle_Vector(2);

        // Регистрация значений произвольных скоростей за период
        public Dictionary<int, decimal[]> TVV_for_order_info;
        public Dictionary<int, decimal[]> BVV_for_order_info;
        public Dictionary<int, decimal[]> SVV_for_order_info;

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
        // Регистрация значений вектора для определения vector_h, vector_l
        private const int _maxBufferSize_vectors_hl = 5;
        private List<decimal> _buffer_vectors_h;
        private List<decimal> _buffer_vectors_l;
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
            {
                return;
            }

            bool isNewCandle = true;

            // Если текущая, заменяем
            if (Buffer.Count != 0 && Buffer[Buffer.Count - 1].Time == _candle.Time)
            {
                Buffer.RemoveAt(Buffer.Count - 1);
                isNewCandle = false;
            }
            // Если избыток
            if (Buffer.Count > Long_Period + Short_Period)
            {
                Buffer.RemoveAt(0);
            }

            Buffer.Add(_candle);


            // - Vector high & low
            if (isNewCandle)
            {
                decimal vh = 0, vl = 0;

                if (Buffer.Count >= 2)
                {
                    List<Trade> AllTrades = _processingData.AllTrades;
                    Trade t;
                    DateTime candleCloseTime = _candle.Time.Add(Buffer[1].Time - Buffer[0].Time);
                    decimal v = 0;

                    int i = _processingData.GetTradeIStart(_candle.Time);
                    if (i != -1)
                    {
                        for (; i < AllTrades.Count; i++)
                        {
                            // Берём сделку
                            t = AllTrades[i];
                            // Если сделка принадлежит более поздней свече
                            if (t.Time >= candleCloseTime)
                                break;

                            // Update vh, vl
                            if (t.OrderDirection == OrderDirections.Buy)
                            {
                                v += t.Volume;
                                if (v > vh)
                                {
                                    vh = v;
                                }
                            }
                            else
                            {
                                v -= t.Volume;
                                if (v < vl)
                                {
                                    vl = v;
                                }
                            }
                        }
                    }
                }

                _buffer_vectors_h.Add(vh);
                _buffer_vectors_l.Add(vl);

                if (_buffer_vectors_h.Count > _maxBufferSize_vectors_hl)
                {
                    _buffer_vectors_h.RemoveAt(0);
                    _buffer_vectors_l.RemoveAt(0);
                }
            }
            else
            {
                decimal current_vector = vector;
                // High
                if (_buffer_vectors_h[_buffer_vectors_h.Count - 1] < current_vector)
                {
                    _buffer_vectors_h[_buffer_vectors_h.Count - 1] = current_vector;
                }
                // Low
                if (_buffer_vectors_l[_buffer_vectors_l.Count - 1] > current_vector)
                {
                    _buffer_vectors_l[_buffer_vectors_l.Count - 1] = current_vector;
                }
            }
        }
        // Стереть все значения
        public void Reset()
        {
            Values_BuyVolume = new List<DecimalIndicatorValue>();
            Values_SellVolume = new List<DecimalIndicatorValue>();
            Buffer = new List<Candle>();

            TVV_for_order_info = new Dictionary<int, decimal[]>();
            BVV_for_order_info = new Dictionary<int, decimal[]>();
            SVV_for_order_info = new Dictionary<int, decimal[]>();

            _buffer_vectors_h = new List<decimal>();
            _buffer_vectors_l = new List<decimal>();

            _bvCache = new Dictionary<int, Dictionary<int, decimal>>();
            _svCache = new Dictionary<int, Dictionary<int, decimal>>();
        }
        // Осциляторы
        public decimal Get_VO(int shift = 0)
        {
            decimal Long_Sum = Get_Sum_Period_TotalVolume(Long_Period, Short_Period + shift);
            decimal Short_Sum = Get_Sum_Period_TotalVolume(Short_Period, shift);
            return Long_Sum == 0 ? 0 : (Short_Sum / Short_Period * Long_Period / Long_Sum - 1) * 100;
        }
        public decimal Get_BO(int shift = 0)
        {
            decimal Long_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Buy, Long_Period, Short_Period + shift);
            decimal Short_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Buy, Short_Period, shift);
            return Long_Sum == 0 ? 0 : (Short_Sum / Short_Period * Long_Period / Long_Sum - 1) * 100;
        }
        public decimal Get_SO(int shift = 0)
        {
            decimal Long_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Sell, Long_Period, Short_Period + shift);
            decimal Short_Sum = Get_Sum_Period_DirectionVolume(OrderDirections.Sell, Short_Period, shift);
            return Long_Sum == 0 ? 0 : (Short_Sum / Short_Period * Long_Period / Long_Sum - 1) * 100;
        }
        // Вектор
        public decimal Get_VectorVolume(int shift = 0)
        {
            List<Trade> AllTrades = _processingData.AllTrades;

            // Если недостаточно данных
            if (Buffer.Count <= 1 || Buffer.Count <= shift || AllTrades.Count == 0)
            {
                return 0;
            }

            decimal result = 0;
            Trade t;
            TimeSpan TF_Period = Buffer[1].Time - Buffer[0].Time;
            Candle candle = Buffer[Buffer.Count - 1 - shift];
            for (int i = 0; i < AllTrades.Count - shift; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более ранней свече
                if (t.Time < candle.Time)
                {
                    break;
                }
                // Если сделка принадлежит более поздней свече
                if (t.Time >= candle.Time.Add(TF_Period))
                {
                    continue;
                }
                // Учитываем направление
                if (t.OrderDirection == OrderDirections.Buy)
                {
                    result += t.Volume;
                }
                else
                {
                    result -= t.Volume;
                }
            }

            return result;
        }
        public decimal Get_VectorHigh(int shift = 0)
        {
            if (_buffer_vectors_h.Count <= shift)
            {
                return 0;
            }

            return _buffer_vectors_h[_buffer_vectors_h.Count - 1 - shift];
        }
        public decimal Get_VectorLow(int shift = 0)
        {
            if (_buffer_vectors_l.Count <= shift)
            {
                return 0;
            }

            return _buffer_vectors_l[_buffer_vectors_l.Count - 1 - shift];
        }
        // Объемы свечи
        public decimal Get_TotalVolume(int shift = 0)
        {
            if (Buffer.Count <= shift)
            {
                return 0;
            }

            return Buffer[Buffer.Count - 1 - shift].TotalVolume;
        }
        public decimal Get_DirectionVolume(OrderDirections _Order_Direction, int shift = 0)
        {
            List<Trade> AllTrades = _processingData.AllTrades;

            // Если недостаточно данных
            if (Buffer.Count <= 1 || Buffer.Count <= shift || AllTrades.Count == 0)
            {
                return 0;
            }

            decimal result = 0;
            Trade t;
            TimeSpan TF_Period = Buffer[1].Time - Buffer[0].Time;
            Candle candle = Buffer[Buffer.Count - 1 - shift];
            for (int i = 0; i < AllTrades.Count - shift; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более ранней свече
                if (t.Time < candle.Time)
                {
                    break;
                }
                // Если сделка принадлежит более поздней свече
                if (t.Time >= candle.Time.Add(TF_Period))
                {
                    continue;
                }
                // Учитываем направление
                if (t.OrderDirection == _Order_Direction)
                {
                    result += t.Volume;
                }
            }

            return result;
        }
        // Суммарные объемы свеч
        public decimal Get_Sum_Period_TotalVolume(int _Candles_Count, int shift = 0)
        {
            if (Buffer.Count < shift + _Candles_Count)
            {
                return 0;
            }

            decimal result = 0;
            for (int i = 0; i < _Candles_Count; i++)
            {
                result += Buffer[Buffer.Count - 1 - i - shift].TotalVolume;
            }

            return result;
        }
        public decimal Get_Sum_Period_DirectionVolume(OrderDirections _Order_Direction, int _Candles_Count, int shift = 0)
        {
            decimal result = 0;
            for (int i = 0; i < _Candles_Count; i++)
            {
                result += Get_DirectionVolume(_Order_Direction, i + shift);
            }

            return result;
        }
        // Средние объемы свеч
        public decimal Get_Average_Candle_Total_Volume(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
            {
                return 0;
            }

            return Get_TotalVolume(shift) / duration;
        }
        public decimal Get_Average_Candle_Vector(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
            {
                return 0;
            }

            return (Get_DirectionVolume(OrderDirections.Buy, shift) - Get_DirectionVolume(OrderDirections.Sell, shift)) / duration;
        }
        public decimal Get_Average_Candle_Buy_Volume(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
            {
                return 0;
            }

            return Get_DirectionVolume(OrderDirections.Buy, shift) / duration;
        }
        public decimal Get_Average_Candle_Sell_Volume(int shift = 0)
        {
            decimal duration = Get_Candle_Duration(shift);
            if (duration == 0)
            {
                return 0;
            }

            return Get_DirectionVolume(OrderDirections.Sell, shift) / duration;
        }
        // Скорости
        public decimal Get_TotalVolume_Velocity(int shift = 0)
        {
            return Get_DirectionVolume_Velocity(OrderDirections.Buy, shift) + Get_DirectionVolume_Velocity(OrderDirections.Sell, shift);
        }
        public decimal Get_VectorVolume_Velocity(int shift = 0)
        {
            return Get_DirectionVolume_Velocity(OrderDirections.Buy, shift) - Get_DirectionVolume_Velocity(OrderDirections.Sell, shift);
        }
        public decimal Get_DirectionVolume_Velocity(OrderDirections _Order_Direction, int shift = 0)
        {
            List<Trade> AllTrades = _processingData.AllTrades;

            if (AllTrades.Count == 0)
            {
                return 0;
            }

            Trade t;
            int _Velocity_Period_Seconds = Velocity_Period_Seconds;
            DateTimeOffset _Current_Time = _processingData.TerminalTime;
            decimal result = 0;
            for (int i = 0; i < AllTrades.Count; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Count - 1 - i];
                // Если сделка принадлежит более раннему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * (shift + 1)) < _Current_Time)
                {
                    break;
                }
                // Если сделка принадлежит более позднему периоду
                if (t.Time.AddSeconds(_Velocity_Period_Seconds * shift) >= _Current_Time.AddSeconds((double)_Velocity_Period_Seconds))
                {
                    continue;
                }

                if (t.OrderDirection == _Order_Direction)
                {
                    result += t.Volume;
                }
            }
            result /= _Velocity_Period_Seconds;

            return result;
        }
        public decimal GetTv(int _Velocity_Period_Seconds, int shift = 0) // Get total volume velocity value
        {
            decimal result = GetBv(_Velocity_Period_Seconds, shift) + GetSv(_Velocity_Period_Seconds, shift);

            // For order info
            if (shift < 2)
            {
                if (!TVV_for_order_info.ContainsKey(_Velocity_Period_Seconds))
                {
                    TVV_for_order_info.Add(_Velocity_Period_Seconds, new decimal[2]);
                }
                TVV_for_order_info[_Velocity_Period_Seconds][shift] = result;
            }

            return result;
        }
        public decimal GetVv(int _Velocity_Period_Seconds, int shift = 0) // Get veco volume velocity value
        {
            decimal result = GetBv(_Velocity_Period_Seconds, shift) - GetSv(_Velocity_Period_Seconds, shift);

            return result;
        }
        public decimal GetBv(int _Velocity_Period_Seconds, int shift = 0) // Get buy volume velocity value
        {
            decimal result = getCachedBvVal(_Velocity_Period_Seconds, shift, VCalcType.Shift);

            // For order info
            if (shift < 2)
            {
                if (!BVV_for_order_info.ContainsKey(_Velocity_Period_Seconds))
                {
                    BVV_for_order_info.Add(_Velocity_Period_Seconds, new decimal[2]);
                }
                BVV_for_order_info[_Velocity_Period_Seconds][shift] = result;
            }

            return result;
        }
        public decimal GetSv(int _Velocity_Period_Seconds, int shift = 0) // Get sell volume velocity value
        {
            decimal result = getCachedSvVal(_Velocity_Period_Seconds, shift, VCalcType.Shift);

            // For order info
            if (shift < 2)
            {
                if (!SVV_for_order_info.ContainsKey(_Velocity_Period_Seconds))
                {
                    SVV_for_order_info.Add(_Velocity_Period_Seconds, new decimal[2]);
                }
                SVV_for_order_info[_Velocity_Period_Seconds][shift] = result;
            }

            return result;
        }
        public decimal GetAvrVvMax(int _Velocity_Period_Seconds, int shift = 0)
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
                    {
                        break;
                    }
                }
                right = mid;
                mid = left;
                left = GetAvrVv(_Velocity_Period_Seconds, i);
            }

            return mid;
        }
        public decimal GetAvrVvMin(int _Velocity_Period_Seconds, int shift = 0)
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
                    {
                        break;
                    }
                }
                right = mid;
                mid = left;
                left = GetAvrVv(_Velocity_Period_Seconds, i);
            }

            return mid;
        }
        public decimal GetAvrTv(int _Velocity_Period_Seconds, int shift = 0)
        {
            return getAvrTvVal(_Velocity_Period_Seconds, shift);
        }
        public decimal GetAvrVv(int _Velocity_Period_Seconds, int shift = 0)
        {
            return getAvrVvVal(_Velocity_Period_Seconds, shift);
        }
        public decimal GetAvrBv(int _Velocity_Period_Seconds, int shift = 0)
        {
            return getAvrBvVal(_Velocity_Period_Seconds, shift);
        }
        public decimal GetAvrSv(int _Velocity_Period_Seconds, int shift = 0)
        {
            return getAvrSvVal(_Velocity_Period_Seconds, shift);
        }
        public decimal GetAvrPeriodVvMax(int _Period_Seconds, int _Offset, int shift = 0)
        {
            return GetAvrPeriodVvMax(_Period_Seconds, _Offset, shift);
        }
        public decimal GetAvrPeriodVvMin(int _Period_Seconds, int _Offset, int shift = 0)
        {
            return GetAvrPeriodVvMin(_Period_Seconds, _Offset, shift);
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
            Candle curCandle = Get_Candle();
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
            Candle curCandle = Get_Candle();
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
            OrderDirections side = OrderDirections.Buy;

            // Prepare
            if (_bvCache.ContainsKey(period) == false)
            {
                _bvCache.Add(period, new Dictionary<int, decimal>());
            }
            Candle curCandle = Get_Candle();
            if (curCandle == null)
            {
                return 0;
            }

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
            for (k = 0; k < tradesCount && AllTrades.ElementAtFromEnd(k).Time.TimeOfDay.TotalSeconds > calcDaySecond; k++)
            {
                ; // Omit late trades
            }

            for (k_last = k; k_last < tradesCount && AllTrades.ElementAtFromEnd(k_last).Time.TimeOfDay.TotalSeconds >= calcDaySecond - period; k_last++)
            {
                ; // Set last including trade index
            }

            k_last--;

            // Calculate
            decimal result = 0;
            Trade t;
            for (; k <= k_last; k++)
            {
                t = AllTrades.ElementAtFromEnd(k);
                if (t.OrderDirection == side)
                {
                    result += t.Volume;
                }
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
            OrderDirections side = OrderDirections.Sell;

            // Prepare
            if (_svCache.ContainsKey(period) == false)
            {
                _svCache.Add(period, new Dictionary<int, decimal>());
            }
            Candle curCandle = Get_Candle();
            if (curCandle == null)
            {
                return 0;
            }

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
            for (k = 0; k < tradesCount && AllTrades.ElementAtFromEnd(k).Time.TimeOfDay.TotalSeconds > calcDaySecond; k++)
            {
                ; // Omit late trades
            }

            for (k_last = k; k_last < tradesCount && AllTrades.ElementAtFromEnd(k_last).Time.TimeOfDay.TotalSeconds >= calcDaySecond - period; k_last++)
            {
                ; // Set last including trade index
            }

            k_last--;

            // Calculate
            decimal result = 0;
            Trade t;
            for (; k <= k_last; k++)
            {
                t = AllTrades.ElementAtFromEnd(k);
                if (t.OrderDirection == side)
                {
                    result += t.Volume;
                }
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
        private decimal getAvrTvVal(int period, int shift = 0)
        {

            return getAvrBvVal(period, shift) + getAvrSvVal(period, shift);
        }
        private decimal getAvrVvVal(int period, int shift = 0)
        {
            return getAvrBvVal(period, shift) - getAvrSvVal(period, shift);
        }
        private decimal getAvrBvVal(int period, int shift = 0)
        {
            Candle curCandle = Get_Candle();
            if (curCandle == null)
            {
                return 0;
            }

            int currentDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond = currentDaySecond - currentDaySecond % VV_TACT - shift * VV_TACT;

            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedBvVal(period, calcDaySecond - i, VCalcType.DaySecond);
            }
            result /= VV_TACT;

            // Output
            return result;
        }
        private decimal getAvrSvVal(int period, int shift = 0)
        {
            Candle curCandle = Get_Candle();
            if (curCandle == null)
            {
                return 0;
            }

            int currentDaySecond = (int)_processingData.TerminalTime.TimeOfDay.TotalSeconds;
            int calcDaySecond = currentDaySecond - currentDaySecond % VV_TACT - shift * VV_TACT;

            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedSvVal(period, calcDaySecond - i, VCalcType.DaySecond);
            }
            result /= VV_TACT;

            // Output
            return result;
        }
        private decimal getAvrPeriodVvMax(int periodSeconds, int offset, int shift = 0)
        {
            decimal result = getAvrVvVal(periodSeconds, offset);
            for (int i = offset + 1; i < periodSeconds; i++)
            {
                decimal tmp = getAvrVvVal(periodSeconds, i);
                if (result < tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        private decimal getAvrPeriodVvMin(int periodSeconds, int offset, int shift = 0)
        {
            decimal result = getAvrVvVal(periodSeconds, offset);
            for (int i = offset + 1; i < periodSeconds; i++)
            {
                decimal tmp = getAvrVvVal(periodSeconds, i);
                if (result > tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        // Время открытия свечи
        public DateTimeOffset Get_OpenTime(int shift = 0)
        {
            if (Buffer.Count <= shift)
            {
                return DateTime.Now.AddYears(-1);
            }
            else
            {
                return Buffer[Buffer.Count - 1 - shift].Time;
            }
        }
        public Candle Get_Candle(int shift = 0)
        {
            return Buffer[Buffer.Count - 1 - shift];
        }
        public decimal Get_Candle_Duration(int shift = 0)
        {
            Candle candle = Get_Candle(shift);
            if (candle == null)
            {
                return 0;
            }

            DateTime OpenTime = candle.Time;
            DateTimeOffset CloseTime = _processingData.TerminalTime;
            if (shift > 0)
            {
                CloseTime = Get_Candle(shift - 1).Time;
            }

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
