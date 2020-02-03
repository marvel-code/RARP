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
        private enum Sides
        {
            Buy, Sell
        }


        /**
         * Рабочие значения
         **/
        // Осциляторы
        public decimal vo => Get_VO();
        public decimal vo_p => Get_VO(1);
        public decimal bo => Get_BO();
        public decimal bo_p => Get_BO(1);
        public decimal so => Get_SO();
        public decimal so_p => Get_SO(1);
        // Вектора
        public decimal vector => Get_VectorVolume();
        public decimal vector_p => Get_VectorVolume(1);
        public decimal vector_pp => Get_VectorVolume(2);
        public decimal vector_a => Math.Abs(vector);
        public decimal vector_ap => Math.Abs(vector_p);
        public decimal vector_app => Math.Abs(vector_pp);
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
        public decimal act => Get_TotalVolume() / Get_Candle_Duration();
        public decimal act_p => Get_TotalVolume(1) / Get_Candle_Duration(1);
        public decimal act_pp => Get_TotalVolume(2) / Get_Candle_Duration(2);
        public decimal acb => Get_DirectionVolume(OrderDirections.Buy) / Get_Candle_Duration();
        public decimal acb_p => Get_DirectionVolume(OrderDirections.Buy, 1) / Get_Candle_Duration(1);
        public decimal acb_pp => Get_DirectionVolume(OrderDirections.Buy, 2) / Get_Candle_Duration(2);
        public decimal acs => Get_DirectionVolume(OrderDirections.Sell) / Get_Candle_Duration();
        public decimal acs_p => Get_DirectionVolume(OrderDirections.Sell, 1) / Get_Candle_Duration(1);
        public decimal acs_pp => Get_DirectionVolume(OrderDirections.Sell, 2) / Get_Candle_Duration(2);
        public decimal acv => acb - acs;
        public decimal acv_p => acb_p - acs_p;
        public decimal acv_pp => acb_pp - acs_pp;
        // Регистрация high\low вектора
        private List<decimal> vectors_h;
        private List<decimal> vectors_l;
        // Регистрация exp tact velocities
        private DateTime _lastExpTactUpdateDateTime;
        private Dictionary<int, MA> _tactExpPrices;
        private Dictionary<KeyValuePair<int, int>, MA> _buyExpTactCache;
        private Dictionary<KeyValuePair<int, int>, MA> _sellExpTactCache;
        // Регистрация значений произвольных скоростей за период
        public Dictionary<int, decimal[]> TVV_for_order_info;
        public Dictionary<int, decimal[]> BVV_for_order_info;
        public Dictionary<int, decimal[]> SVV_for_order_info;
        /**
         * Настройки
         **/
        // Параметры
        public int Long_Period { get; private set; }
        public int Short_Period { get; private set; }
        public int Velocity_Period_Seconds { get; private set; }
        // Массив сделок
        public IEnumerable<Trade> IAllTrades;
        public Trade[] AllTrades;
        // Массив свечек
        public List<Candle> Buffer;
        // Проекция времени кратного секунде на последний индекс AllTrades этой секунды
        private int _lastProcessedProectionTradeIndex;
        private Dictionary<DateTime, int> _proectionDateTime2AllTrades;
        private Dictionary<DateTime, decimal> _buySumByMinutesCache;
        private Dictionary<DateTime, decimal> _sellSumByMinutesCache;
        private decimal getDirectionMinuteSumAtDateTime(Sides side, DateTime dt)
        {
            Dictionary<DateTime, decimal> sideCache = side == Sides.Buy ? _buySumByMinutesCache : _sellSumByMinutesCache;
            if (!sideCache.ContainsKey(dt))
            {
                return 0;
            }

            return sideCache[dt];
        }
        private int getNearestTradeIndexLaterDateTime(DateTime DT)
        {
            if (DT < AllTrades[0].Time)
            {
                return 0;
            }
            for (var dt = GetDateTimeWithoutMillis(DT).AddSeconds(1); dt <= Get_CurrentTime(); dt = dt.AddSeconds(1))
            {
                if (dt.Hour < 10)
                {
                    dt = dt.Add(-dt.TimeOfDay + TimeSpan.FromHours(10));
                }
                while (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    dt = dt.Add(-dt.TimeOfDay + TimeSpan.FromHours(10));
                    dt = dt.AddDays(1);
                }
                if (_proectionDateTime2AllTrades.ContainsKey(dt))
                {
                    int result = _proectionDateTime2AllTrades[dt];
                    while (result > 0 && AllTrades[result - 1].Time > DT)
                        --result;
                    return result;
                }
            }
            return AllTrades.Length - 1;
        }
        private int getNearestTradeIndexEarlierDateTime(DateTime DT)
        {
            if (DT > AllTrades[AllTrades.Length - 1].Time)
            {
                return AllTrades.Length - 1;
            }
            for (var dt = GetDateTimeWithoutMillis(DT).AddSeconds(-1); dt > AllTrades[0].Time; dt = dt.AddSeconds(-1))
            {
                if (dt.Hour < 10)
                {
                    dt = dt.Add(-dt.TimeOfDay);
                }
                while (dt.DayOfWeek == DayOfWeek.Saturday || dt.DayOfWeek == DayOfWeek.Sunday)
                {
                    dt = dt.Add(-dt.TimeOfDay - TimeSpan.FromSeconds(1));
                }
                if (_proectionDateTime2AllTrades.ContainsKey(dt))
                {
                    int result = _proectionDateTime2AllTrades[dt];
                    while (result + 1 < AllTrades.Length && AllTrades[result + 1].Time < DT)
                        ++result;
                    return result;
                }
            }
            return 0;
        }
        // Массивы значений
        private List<DecimalIndicatorValue> Values_BuyVolume;
        private List<DecimalIndicatorValue> Values_SellVolume;

        // Инициализация
        private ServiceContracts.ProcessingData TM;
        public Vol(ServiceContracts.ProcessingData processingData, int _Long_Period, int _Short_Period, int _Velocity_Period_Seconds)
        {
            TM = processingData;

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

            IAllTrades = TM.AllTrades;
            AllTrades = IAllTrades.ToArray();

            // Update AllTrades proection & sums by minutes
            int allTradesCount = IAllTrades.Count();
            if (_lastProcessedProectionTradeIndex == -1)
            {
                _lastProcessedProectionTradeIndex = allTradesCount - 1;
                var pq = IAllTrades.AsParallel().Select((t, i) => new { Index = i, Trade = t });
                object locker_proection = new object();
                object locker_buySumsCaches = new object();
                object locker_sellSumsCaches = new object();
                pq.ForAll(el =>
                {
                    // Proection
                    DateTime dt = GetDateTimeWithoutMillis(el.Trade.Time);
                    lock (locker_proection)
                    {
                        if (!_proectionDateTime2AllTrades.ContainsKey(dt))
                        {
                            _proectionDateTime2AllTrades.Add(dt, el.Index);
                        }
                        else
                            if (_proectionDateTime2AllTrades[dt] > el.Index)
                        {
                            _proectionDateTime2AllTrades[dt] = el.Index;
                        }
                    }
                    // Sums by minutes
                    DateTime sum_minutes_dt = GetDateTimeDivSeconds(dt, 60);
                    if (el.Trade.OrderDirection == OrderDirections.Buy)
                    {
                        lock (locker_buySumsCaches)
                        {
                            if (!_buySumByMinutesCache.ContainsKey(sum_minutes_dt))
                            {
                                _buySumByMinutesCache.Add(sum_minutes_dt, 0);
                            }

                            _buySumByMinutesCache[sum_minutes_dt] += el.Trade.Volume;
                        }
                    }
                    else
                    {
                        lock (locker_sellSumsCaches)
                        {
                            if (!_sellSumByMinutesCache.ContainsKey(sum_minutes_dt))
                            {
                                _sellSumByMinutesCache.Add(sum_minutes_dt, 0);
                            }

                            _sellSumByMinutesCache[sum_minutes_dt] += el.Trade.Volume;
                        }
                    }
                });
                Console.WriteLine("Proection & sums by minutes have been loaded.");
            }
            else
            {
                while (_lastProcessedProectionTradeIndex + 1 < allTradesCount)
                {
                    ++_lastProcessedProectionTradeIndex;
                    Trade trade = IAllTrades.ElementAt(_lastProcessedProectionTradeIndex);
                    // Proection
                    DateTime dt = GetDateTimeWithoutMillis(trade.Time);
                    if (!_proectionDateTime2AllTrades.ContainsKey(dt))
                    {
                        _proectionDateTime2AllTrades.Add(dt, _lastProcessedProectionTradeIndex);
                    }
                    // Sums by minutes
                    DateTime trade_minutes_dt = GetDateTimeDivSeconds(dt, 60);
                    Dictionary<DateTime, decimal> sideCache = trade.OrderDirection == OrderDirections.Buy ? _buySumByMinutesCache : _sellSumByMinutesCache;
                    if (!sideCache.ContainsKey(trade_minutes_dt))
                    {
                        sideCache.Add(trade_minutes_dt, 0);
                    }

                    sideCache[trade_minutes_dt] += trade.Volume;
                }
            }

            // Vector High\Low
            if (vectors_h.Count == 0 || Buffer[Buffer.Count - 1].Time != _candle.Time)
            {
                vectors_h.Add(0);
                vectors_l.Add(0);
            }
            decimal current_vector = Get_VectorVolume();
            if (vectors_h.Last() < current_vector)
            {
                vectors_h[vectors_h.Count - 1] = current_vector;
            }

            if (vectors_l.Last() > current_vector)
            {
                vectors_l[vectors_l.Count - 1] = current_vector;
            }

            // Tact exp velocities
            DateTime current_time = Get_CurrentTime();
            if (current_time - _lastExpTactUpdateDateTime > TimeSpan.FromHours(10))
            {
                Console.WriteLine("Avr caches init started..");
                foreach (KeyValuePair<int, int> kvp in MySettings.VELOCITIES_SETTINGS)
                {
                    DateTime timeStamp = DateTime.Now;
                    _proectionDateTime2AllTrades.AsParallel().ForAll(d => getAvrTvVal(kvp.Key, vCalcType: VCalcType.DateTime, dateTime: d.Key));
                    Console.WriteLine("Формирование кэша для {1} заняло {0}мин", Math.Round((DateTime.Now - timeStamp).Add(TimeSpan.FromSeconds(30)).TotalMinutes), kvp.Key);
                }
                Console.WriteLine("Avr caches init finished!");

                List<DateTime> tactsProectionsDateTimes = _proectionDateTime2AllTrades.Keys.Where(p => p.TimeOfDay.TotalSeconds % VV_TACT == 0).ToList();
                tactsProectionsDateTimes.Sort();
                List<DateTime> proections2add = new List<DateTime>();
                for (int i = 1; i < tactsProectionsDateTimes.Count; ++i)
                {
                    int delta = (int)(tactsProectionsDateTimes[i] - tactsProectionsDateTimes[i - 1]).TotalSeconds;
                    if (delta < 3600)
                    {
                        for (int j = 1; j < delta / VV_TACT; ++j)
                        {
                            proections2add.Add(tactsProectionsDateTimes[i - 1].AddSeconds(VV_TACT * j));
                        }
                    }
                }
                tactsProectionsDateTimes.AddRange(proections2add);
                tactsProectionsDateTimes.Sort();

                Console.WriteLine("Exp init started..");
                int k = 0;
                int k_max = tactsProectionsDateTimes.Count;
                int k_step = 1;
                foreach (DateTime dt in tactsProectionsDateTimes)
                {
                    ++k;
                    if (k * 10 > k_step * k_max) // k/k_max > k_step/10
                    {
                        Console.WriteLine("{0}0%", k_step);
                        ++k_step;
                    }
                    updateExpVelocities(dt);
                    updateExpPrices(dt);
                }
                _lastExpTactUpdateDateTime = tactsProectionsDateTimes.Last();
                Console.WriteLine("Exp init finished!");
            }
            else
            {
                while (_lastExpTactUpdateDateTime.AddSeconds(VV_TACT) <= current_time)
                {
                    _lastExpTactUpdateDateTime = _lastExpTactUpdateDateTime.AddSeconds(VV_TACT);
                    if (_lastExpTactUpdateDateTime.TimeOfDay.TotalHours < 10)
                    {
                        _lastExpTactUpdateDateTime = _lastExpTactUpdateDateTime.AddHours(10);
                        while (_lastExpTactUpdateDateTime.Date != TM.TerminalTime.Date)
                        {
                            _lastExpTactUpdateDateTime = _lastExpTactUpdateDateTime.AddDays(1);
                        }
                    }

                    DateTime dt = _lastExpTactUpdateDateTime;
                    updateExpVelocities(dt);
                    updateExpPrices(dt);
                }
            }

            // Buffer
            if (Buffer.Count != 0 && Buffer[Buffer.Count - 1].Time == _candle.Time)
            {
                Buffer.RemoveAt(Buffer.Count - 1);
            }

            if (Buffer.Count > Long_Period + Short_Period)
            {
                Buffer.RemoveAt(0);
            }

            Buffer.Add(_candle);
        }
        // Стереть все значения
        public void Reset()
        {
            IAllTrades = TM.AllTrades;

            _lastProcessedProectionTradeIndex = -1;
            _proectionDateTime2AllTrades = new Dictionary<DateTime, int>();
            _buySumByMinutesCache = new Dictionary<DateTime, decimal>();
            _sellSumByMinutesCache = new Dictionary<DateTime, decimal>();

            Values_BuyVolume = new List<DecimalIndicatorValue>();
            Values_SellVolume = new List<DecimalIndicatorValue>();
            Buffer = new List<Candle>();

            TVV_for_order_info = new Dictionary<int, decimal[]>();
            BVV_for_order_info = new Dictionary<int, decimal[]>();
            SVV_for_order_info = new Dictionary<int, decimal[]>();

            vectors_h = new List<decimal>();
            vectors_l = new List<decimal>();

            _lastExpTactUpdateDateTime = new DateTime();
            _tactExpPrices = new Dictionary<int, MA>();
            _buyExpTactCache = new Dictionary<KeyValuePair<int, int>, MA>();
            _sellExpTactCache = new Dictionary<KeyValuePair<int, int>, MA>();
            foreach (int kvp in MySettings.PRICE_SETTINGS)
            {
                _tactExpPrices.Add(kvp, new MA(kvp, MaType.Exponential, CalculationType.Close, isUnlimitedCache: true));
            }

            foreach (KeyValuePair<int, int> kvp in MySettings.VELOCITIES_SETTINGS)
            {
                _buyExpTactCache.Add(kvp, new MA(kvp.Value, MaType.Exponential, CalculationType.Close, isUnlimitedCache: true));
                _sellExpTactCache.Add(kvp, new MA(kvp.Value, MaType.Exponential, CalculationType.Close, isUnlimitedCache: true));
            }

            _bvCache = new Dictionary<int, Dictionary<DateTime, decimal>>();
            _svCache = new Dictionary<int, Dictionary<DateTime, decimal>>();
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
        public decimal Get_VectorHigh(int shift = 0)
        {
            if (vectors_h.Count <= shift)
            {
                return 0;
            }

            return vectors_h[vectors_h.Count - 1 - shift];
        }
        public decimal Get_VectorLow(int shift = 0)
        {
            if (vectors_l.Count <= shift)
            {
                return 0;
            }

            return vectors_l[vectors_l.Count - 1 - shift];
        }
        public decimal Get_VectorVolume(int shift = 0)
        {
            return Get_DirectionVolume(OrderDirections.Buy, shift) - Get_DirectionVolume(OrderDirections.Sell, shift);
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
            // Если недостаточно данных
            if (Buffer.Count <= 1 || Buffer.Count <= shift || AllTrades.Length == 0)
            {
                return 0;
            }

            decimal result = 0;
            Trade t;
            TimeSpan TF_Period = Buffer[1].Time - Buffer[0].Time;
            Candle candle = Buffer[Buffer.Count - 1 - shift];
            for (int i = 0; i < AllTrades.Length - shift; i++)
            {
                // Берём сделку
                t = AllTrades[AllTrades.Length - 1 - i];
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
            if (IAllTrades.Count() == 0)
            {
                return 0;
            }

            Trade t;
            int _Velocity_Period_Seconds = Velocity_Period_Seconds;
            DateTimeOffset _Current_Time = TM.TerminalTime;
            decimal result = 0;
            for (int i = 0; i < IAllTrades.Count(); i++)
            {
                // Берём сделку
                t = IAllTrades.ElementAtFromEnd(i);
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
            decimal result = getCachedBvVal(_Velocity_Period_Seconds, shift);

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
            decimal result = getCachedSvVal(_Velocity_Period_Seconds, shift);

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
            right = getAvrVvVal(_Velocity_Period_Seconds, 0);
            mid = getAvrVvVal(_Velocity_Period_Seconds, 1);
            left = getAvrVvVal(_Velocity_Period_Seconds, 2);
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
                left = getAvrVvVal(_Velocity_Period_Seconds, i);
            }

            return mid;
        }
        public decimal GetAvrVvMin(int _Velocity_Period_Seconds, int shift = 0)
        {
            decimal left, mid, right;
            right = getAvrVvVal(_Velocity_Period_Seconds, 0);
            mid = getAvrVvVal(_Velocity_Period_Seconds, 1);
            left = getAvrVvVal(_Velocity_Period_Seconds, 2);
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
                left = getAvrVvVal(_Velocity_Period_Seconds, i);
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
        public decimal GetAvrTactsTvMax(int periodSeconds, int tacts_count, int shift = 0)
        {
            decimal result = getAvrTvVal(periodSeconds, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = getAvrTvVal(periodSeconds, i);
                if (result < tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        public decimal GetAvrTactsTvMin(int periodSeconds, int tacts_count, int shift = 0)
        {
            decimal result = getAvrTvVal(periodSeconds, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = getAvrTvVal(periodSeconds, i);
                if (result > tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        public decimal GetAvrTactsVvMax(int periodSeconds, int tacts_count, int shift = 0)
        {
            decimal result = getAvrVvVal(periodSeconds, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = getAvrVvVal(periodSeconds, i);
                if (result < tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        public decimal GetAvrTactsVvMin(int periodSeconds, int tacts_count, int shift = 0)
        {
            decimal result = getAvrVvVal(periodSeconds, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = getAvrVvVal(periodSeconds, i);
                if (result > tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        public decimal GetAvrPeriodVvMax(int _Period_Seconds, int _Offset, int shift = 0)
        {
            return GetAvrPeriodVvMax(_Period_Seconds, _Offset, shift);
        }
        public decimal GetAvrPeriodVvMin(int _Period_Seconds, int _Offset, int shift = 0)
        {
            return GetAvrPeriodVvMin(_Period_Seconds, _Offset, shift);
        }
        public decimal GetExpTv(int periodSeconds, int n, int shift = 0)
        {
            return GetExpBv(periodSeconds, n, shift) + GetExpSv(periodSeconds, n, shift);
        }

        public decimal GetExpVv(int periodSeconds, int n, int shift = 0)
        {
            return GetExpBv(periodSeconds, n, shift) - GetExpSv(periodSeconds, n, shift);
        }

        public decimal GetExpBv(int periodSeconds, int n, int shift = 0)
        {
            return _buyExpTactCache[new KeyValuePair<int, int>(periodSeconds, n)].Get_Value(shift);
        }
        public decimal GetExpSv(int periodSeconds, int n, int shift = 0)
        {
            return _sellExpTactCache[new KeyValuePair<int, int>(periodSeconds, n)].Get_Value(shift);
        }
        public decimal GetExpTv(int periodSeconds, int n, DateTime dt)
        {
            return GetExpBv(periodSeconds, n, dt) + GetExpSv(periodSeconds, n, dt);
        }

        public decimal GetExpVv(int periodSeconds, int n, DateTime dt)
        {
            return GetExpBv(periodSeconds, n, dt) - GetExpSv(periodSeconds, n, dt);
        }

        public decimal GetExpBv(int periodSeconds, int n, DateTime dt)
        {
            return _buyExpTactCache[new KeyValuePair<int, int>(periodSeconds, n)].Get_Value(dt);
        }
        public decimal GetExpSv(int periodSeconds, int n, DateTime dt)
        {
            return _sellExpTactCache[new KeyValuePair<int, int>(periodSeconds, n)].Get_Value(dt);
        }
        public decimal GetExpTactsTvMax(int periodSeconds, int n, int tacts_count, int shift = 0)
        {
            decimal result = GetExpTv(periodSeconds, n, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = GetExpTv(periodSeconds, n, i);
                if (tmp > result)
                {
                    result = tmp;
                }
            }
            return result;
        }
        public decimal GetExpTactsTvMin(int periodSeconds, int n, int tacts_count, int shift = 0)
        {
            decimal result = GetExpTv(periodSeconds, n, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = GetExpTv(periodSeconds, n, i);
                if (tmp < result)
                {
                    result = tmp;
                }
            }
            return result;
        }
        public decimal GetExpTactsVvMax(int periodSeconds, int n, int tacts_count, int shift = 0)
        {
            decimal result = GetExpVv(periodSeconds, n, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = GetExpVv(periodSeconds, n, i);
                if (tmp > result)
                {
                    result = tmp;
                }
            }
            return result;
        }
        public decimal GetExpTactsVvMin(int periodSeconds, int n, int tacts_count, int shift = 0)
        {
            decimal result = GetExpVv(periodSeconds, n, shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = GetExpVv(periodSeconds, n, i);
                if (tmp < result)
                {
                    result = tmp;
                }
            }
            return result;
        }
        // Технический код рассчёта скоростей (не для использования во вне)
        private const int VV_TACT = MySettings.VV_TACT;
        private enum VCalcType { Shift, DateTime }
        private static TimeSpan maxCacheTimeDelta = TimeSpan.FromDays(2);
        private Dictionary<int, Dictionary<DateTime, decimal>> _bvCache; // Dict<period, Dict<datetime, value>
        private Dictionary<int, Dictionary<DateTime, decimal>> _svCache;
        private decimal getCachedTvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getCachedBvVal(period, shift, vCalcType, dateTime) + getCachedSvVal(period, shift, vCalcType, dateTime);
        }
        private decimal getCachedVvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getCachedBvVal(period, shift, vCalcType, dateTime) - getCachedSvVal(period, shift, vCalcType, dateTime);
        }
        private decimal getCachedBvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            Dictionary<int, Dictionary<DateTime, decimal>> sideCache = _bvCache;

            DateTime currentTime = TM.TerminalTime;
            DateTime calcDaySecond;
            switch (vCalcType)
            {
                case VCalcType.DateTime:
                    if (dateTime == null)
                    {
                        throw new Exception("datetime=null with VCalcType.DateTime");
                    }

                    calcDaySecond = dateTime.Value;
                    break;
                default:
                case VCalcType.Shift:
                    calcDaySecond = currentTime.AddSeconds(-shift);
                    break;
            }

            // Get past value
            decimal result = 0;
            if (sideCache.ContainsKey(period) && sideCache[period].ContainsKey(calcDaySecond))
            {
                result = sideCache[period][calcDaySecond];
            }
            else
            {
                result = getRealBvVal(period, shift, vCalcType, dateTime);
            }

            return result;
        }
        private decimal getCachedSvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            Dictionary<int, Dictionary<DateTime, decimal>> sideCache = _svCache;

            DateTime currentTime = TM.TerminalTime;
            DateTime calcDaySecond;
            switch (vCalcType)
            {
                case VCalcType.DateTime:
                    if (dateTime == null)
                    {
                        throw new Exception("datetime=null with VCalcType.DateTime");
                    }

                    calcDaySecond = dateTime.Value;
                    break;
                default:
                case VCalcType.Shift:
                    calcDaySecond = currentTime.AddSeconds(-shift);
                    break;
            }

            // Get past value
            decimal result = 0;
            if (sideCache.ContainsKey(period) && sideCache[period].ContainsKey(calcDaySecond))
            {
                result = sideCache[period][calcDaySecond];
            }
            else
            {
                result = getRealSvVal(period, shift, vCalcType, dateTime);
            }

            return result;
        }
        private decimal getRealTvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getRealBvVal(period, shift, vCalcType, dateTime) + getRealSvVal(period, shift, vCalcType, dateTime);
        }
        private decimal getRealVvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getRealBvVal(period, shift, vCalcType, dateTime) - getRealSvVal(period, shift, vCalcType, dateTime);
        }

        private readonly object locker_sideCache = new object();
        private decimal getRealSideVelocity(Sides side, int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            // Prepare
            Dictionary<int, Dictionary<DateTime, decimal>> sideCache = side == Sides.Buy ? _bvCache : _svCache;
            OrderDirections orderDireciton = side == Sides.Buy ? OrderDirections.Buy : OrderDirections.Sell;
            lock (locker_sideCache)
            {
                if (!sideCache.ContainsKey(period))
                {
                    sideCache.Add(period, new Dictionary<DateTime, decimal>());
                }
            }

            DateTime currentTime = TM.TerminalTime;
            DateTime calcTime;
            switch (vCalcType)
            {
                case VCalcType.DateTime:
                    if (dateTime == null)
                    {
                        throw new Exception("dateTime=null with VCalcType.DateTime");
                    }

                    calcTime = dateTime.Value;
                    break;
                default:
                case VCalcType.Shift:
                    calcTime = currentTime.AddSeconds(-shift);
                    break;
            }
            calcTime = GetDateTimeWithoutMillis(calcTime);
            DateTime beginCalcTime = calcTime;
            DateTime endCalcTime = beginCalcTime.AddSeconds(-period);
            DateTime lastCalcTime = GetDateTimeDivSeconds(endCalcTime, 60);

            // Calculate
            decimal result = 0;
            for (calcTime = GetDateTimeDivSeconds(calcTime, 60); calcTime >= lastCalcTime; calcTime = calcTime.AddMinutes(-1))
            {
                result += getDirectionMinuteSumAtDateTime(side, calcTime);
            }

            DateTime delBeforeDT = lastCalcTime;
            for (int k = getNearestTradeIndexEarlierDateTime(endCalcTime); k >= 0 && AllTrades[k].Time >= delBeforeDT; --k)
            {
                if (AllTrades[k].OrderDirection == orderDireciton)
                {
                    result -= AllTrades[k].Volume;
                }
            }

            DateTime delAfterDT = GetDateTimeDivSeconds(beginCalcTime, 60).AddMinutes(1);
            for (int k = getNearestTradeIndexLaterDateTime(beginCalcTime); k < AllTrades.Length && AllTrades[k].Time < delAfterDT; ++k)
            {
                if (AllTrades[k].OrderDirection == orderDireciton)
                {
                    result -= AllTrades[k].Volume;
                }
            }

            result /= period;

            // Cache
            lock (locker_sideCache)
            {
                if (sideCache[period].ContainsKey(calcTime))
                {
                    sideCache[period][calcTime] = result;
                }
                else
                {
                    sideCache[period].Add(calcTime, result);
                    if (sideCache[period].Count > maxCacheTimeDelta.TotalSeconds)
                    {
                        sideCache[period].RemoveWhere(x => x.Key < currentTime.Add(-maxCacheTimeDelta));
                    }
                }
            }

            return result;
        }
        private decimal getRealBvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getRealSideVelocity(Sides.Buy, period, shift, vCalcType, dateTime);
        }
        private decimal getRealSvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getRealSideVelocity(Sides.Sell, period, shift, vCalcType, dateTime);
        }
        private decimal getAvrTvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getAvrBvVal(period, shift, vCalcType, dateTime) + getAvrSvVal(period, shift, vCalcType, dateTime);
        }
        private decimal getAvrVvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            return getAvrBvVal(period, shift, vCalcType, dateTime) - getAvrSvVal(period, shift, vCalcType, dateTime);
        }
        private decimal getAvrBvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            DateTime calcDayTime;
            switch (vCalcType)
            {
                case VCalcType.DateTime:
                    if (dateTime == null)
                    {
                        throw new Exception("dateTime=null with VCalcType.DateTime");
                    }

                    calcDayTime = GetTactDateTime(dateTime.Value);
                    break;
                default:
                case VCalcType.Shift:
                    DateTime currentTime = TM.TerminalTime;
                    calcDayTime = currentTime.AddSeconds(-currentTime.TimeOfDay.TotalSeconds % VV_TACT - shift * VV_TACT);
                    break;
            }

            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedBvVal(period, vCalcType: VCalcType.DateTime, dateTime: calcDayTime.AddSeconds(-i));
            }

            result /= VV_TACT;

            // Output
            return result;
        }
        private decimal getAvrSvVal(int period, int shift = 0, VCalcType vCalcType = VCalcType.Shift, DateTime? dateTime = null)
        {
            DateTime calcDayTime;
            switch (vCalcType)
            {
                case VCalcType.DateTime:
                    if (dateTime == null)
                    {
                        throw new Exception("dateTime=null with VCalcType.DateTime");
                    }

                    calcDayTime = GetTactDateTime(dateTime.Value);
                    break;
                default:
                case VCalcType.Shift:
                    DateTime currentTime = TM.TerminalTime;
                    calcDayTime = currentTime.AddSeconds(-currentTime.TimeOfDay.TotalSeconds % VV_TACT - shift * VV_TACT);
                    break;
            }

            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedSvVal(period, vCalcType: VCalcType.DateTime, dateTime: calcDayTime.AddSeconds(-i));
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

        private void updateExpVelocities(DateTime dt)
        {
            foreach (KeyValuePair<KeyValuePair<int, int>, MA> kvp in _buyExpTactCache)
            {
                kvp.Value.Update(new TimeFrameCandle { Time = dt, ClosePrice = getAvrBvVal(kvp.Key.Key, vCalcType: VCalcType.DateTime, dateTime: dt) });
            }

            foreach (KeyValuePair<KeyValuePair<int, int>, MA> kvp in _sellExpTactCache)
            {
                kvp.Value.Update(new TimeFrameCandle { Time = dt, ClosePrice = getAvrSvVal(kvp.Key.Key, vCalcType: VCalcType.DateTime, dateTime: dt) });
            }
        }
        private void updateExpPrices(DateTime dt)
        {
            foreach (KeyValuePair<int, MA> kvp in _tactExpPrices)
            {
                kvp.Value.Update(new TimeFrameCandle { Time = dt, ClosePrice = GetTactRealPrice(dt: dt) });
            }
        }
        // Графики
        public decimal getTactRealPrice4Chart(DateTime dt)
        {
            return IAllTrades.Last(t => t.Time <= dt).Price;
        }
        public decimal getAvrTvVal4Chart(int periodSeconds, DateTime dt)
        {
            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedTvVal(periodSeconds, vCalcType: VCalcType.DateTime, dateTime: dt);
            }

            result /= VV_TACT;

            // Output
            return result;
        }
        public decimal getAvrVvVal4Chart(int periodSeconds, DateTime dt)
        {
            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedSvVal(periodSeconds, vCalcType: VCalcType.DateTime, dateTime: dt);
            }

            result /= VV_TACT;

            // Output
            return result;
        }
        public decimal getAvrBvVal4Chart(int periodSeconds, DateTime dt)
        {
            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedBvVal(periodSeconds, vCalcType: VCalcType.DateTime, dateTime: dt);
            }

            result /= VV_TACT;

            // Output
            return result;
        }
        public decimal getAvrSvVal4Chart(int periodSeconds, DateTime dt)
        {
            // Calculating
            decimal result = 0;
            for (int i = 0; i < VV_TACT; i++)
            {
                result += getCachedSvVal(periodSeconds, vCalcType: VCalcType.DateTime, dateTime: dt);
            }

            result /= VV_TACT;

            // Output
            return result;
        }
        // Общие данные
        public DateTime Get_CurrentTime()
        {
            return TM.TerminalTime;
        }
        public DateTime Get_OpenTime(int shift = 0)
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
            return Buffer.Count > shift ? Buffer[Buffer.Count - 1 - shift] : null;
        }
        public decimal Get_Candle_Duration(int shift = 0)
        {
            Candle candle = Get_Candle(shift);
            if (candle == null)
            {
                return 0;
            }

            DateTime OpenTime = candle.Time;
            DateTime CloseTime = TM.TerminalTime;
            if (shift > 0)
            {
                CloseTime = Get_Candle(shift - 1).Time;
            }

            return (decimal)(CloseTime - OpenTime).TotalSeconds;
        }

        private Dictionary<DateTime, decimal> _tactRealPrice_buffer = new Dictionary<DateTime, decimal>();
        public decimal GetTactRealPrice(int shift = 0, DateTime? dt = null)
        {
            decimal result;

            // Init time bounds
            DateTime calcTime = dt != null ? dt.Value : TM.TerminalTime;
            calcTime = calcTime.AddSeconds(-calcTime.TimeOfDay.TotalSeconds % VV_TACT - shift * VV_TACT);
            calcTime = GetDateTimeWithoutMillis(calcTime);

            if (_tactRealPrice_buffer.ContainsKey(calcTime))
            {
                result = _tactRealPrice_buffer[calcTime];
            }
            else
            {
                result = AllTrades[getNearestTradeIndexEarlierDateTime(calcTime)].Price;
                _tactRealPrice_buffer.Add(calcTime, result);
            }

            return result;
        }

        private Dictionary<int, decimal> _tactAveragePriceByTrades = new Dictionary<int, decimal>();
        public decimal GetTactAveragePriceByTrades(int shift = 0)
        {
            decimal result = 0;
            Candle curCandle = Get_Candle();
            Candle prevCandle = Get_Candle(1);
            if (curCandle != null && prevCandle != null)
            {
                // Reset cache
                if (curCandle.Time.Day != prevCandle.Time.Day)
                {
                    _tactAveragePriceByTrades = new Dictionary<int, decimal>();
                }

                // Init time bounds
                int currentDaySecond = (int)TM.TerminalTime.TimeOfDay.TotalSeconds;
                int openDaySecond = currentDaySecond - currentDaySecond % VV_TACT - (shift + 1) * VV_TACT;
                if (_tactAveragePriceByTrades.ContainsKey(openDaySecond))
                {
                    // Get cached value
                    result = _tactAveragePriceByTrades[openDaySecond];
                }
                else
                {
                    int closeDaySecond = openDaySecond + VV_TACT;

                    // Calculate new value
                    int AllTradesCount = IAllTrades.Count();
                    Trade t;
                    int k = 0;
                    decimal sum = 0;
                    decimal volume = 0;
                    while (k < AllTradesCount && (int)(t = IAllTrades.ElementAtFromEnd(k)).Time.TimeOfDay.TotalSeconds >= openDaySecond)
                    {
                        k++;
                        int tradeDaySecond = (int)t.Time.TimeOfDay.TotalSeconds;
                        if (tradeDaySecond >= closeDaySecond)
                        {
                            continue;
                        }

                        sum += t.Price * t.Volume;
                        volume += t.Volume;
                    }

                    // Cache
                    if (volume != 0)
                    {
                        result = sum / volume;
                    }

                    _tactAveragePriceByTrades.Add(openDaySecond, result);
                }
            }
            return result;
        }

        private Dictionary<int, decimal> _tactAveragePriceBySeconds = new Dictionary<int, decimal>();
        public decimal GetTactAveragePriceBySeconds(int shift = 0)
        {
            decimal result = 0;
            Candle curCandle = Get_Candle();
            Candle prevCandle = Get_Candle(1);
            if (curCandle != null && prevCandle != null)
            {
                // Reset cache
                if (curCandle.Time.Day != prevCandle.Time.Day)
                {
                    _tactAveragePriceBySeconds = new Dictionary<int, decimal>();
                }

                // Init time bounds
                int currentDaySecond = (int)TM.TerminalTime.TimeOfDay.TotalSeconds;
                int openDaySecond = currentDaySecond - currentDaySecond % VV_TACT - (shift + 1) * VV_TACT;
                if (_tactAveragePriceBySeconds.ContainsKey(openDaySecond))
                {
                    // Get cached value
                    result = _tactAveragePriceBySeconds[openDaySecond];
                }
                else
                {
                    int closeDaySecond = openDaySecond + VV_TACT;

                    // Calculate new value
                    int AllTradesCount = IAllTrades.Count();
                    Trade t;
                    int k = 0;
                    decimal sum = 0;
                    int count = 0;
                    int prevTradeDaySecond = 0;
                    while (k < AllTradesCount && (int)(t = IAllTrades.ElementAtFromEnd(k)).Time.TimeOfDay.TotalSeconds >= openDaySecond)
                    {
                        k++;
                        int tradeDaySecond = (int)t.Time.TimeOfDay.TotalSeconds;
                        if (tradeDaySecond >= closeDaySecond || tradeDaySecond == prevTradeDaySecond)
                        {
                            continue;
                        }

                        prevTradeDaySecond = tradeDaySecond;

                        sum += t.Price;
                        count++;
                    }

                    // Cache
                    if (count != 0)
                    {
                        result = sum / count;
                    }

                    _tactAveragePriceBySeconds.Add(openDaySecond, result);
                }
            }
            return result;
        }
        public decimal GetTactRealPriceLocalMax(int number = 0) // number=0 is last
        {
            int shift = 0;
            while (number >= 0)
            {
                ++shift;
                if (GetTactRealPrice(shift) > GetTactRealPrice(shift - 1) && GetTactRealPrice(shift) > GetTactRealPrice(shift + 1))
                {
                    --number;
                }
            }

            return GetTactRealPrice(shift);
        }
        public decimal GetTactRealPriceLocalMin(int number = 0) // number=0 is last
        {
            int shift = 0;
            while (number >= 0)
            {
                ++shift;
                if (GetTactRealPrice(shift) < GetTactRealPrice(shift - 1) && GetTactRealPrice(shift) < GetTactRealPrice(shift + 1))
                {
                    --number;
                }
            }

            return GetTactRealPrice(shift);
        }
        public decimal GetTactRealPriceTactsMax(int tacts_count, int shift = 0)
        {
            decimal result = GetTactRealPrice(shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = GetTactRealPrice(i);
                if (result < tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }
        public decimal GetTactRealPriceTactsMin(int tacts_count, int shift = 0)
        {
            decimal result = GetTactRealPrice(shift);
            for (int i = shift + 1; i < shift + tacts_count; i++)
            {
                decimal tmp = GetTactRealPrice(i);
                if (result > tmp)
                {
                    result = tmp;
                }
            }

            return result;
        }

        public decimal GetTactExpPrice(int n, int shift = 0)
        {
            if (!MySettings.PRICE_SETTINGS.Contains(n))
            {
                throw new Exception(string.Format("Настройка экспоненты с периодом {0} отсутствует в MySettings.PRICE_SETTINGS!", n));
            }

            return _tactExpPrices.ContainsKey(n) ? _tactExpPrices[n].Get_Value(shift) : GetTactRealPrice(shift);
        }
        public decimal GetTactExpPrice(int n, DateTime dt)
        {
            if (!MySettings.PRICE_SETTINGS.Contains(n))
            {
                throw new Exception(string.Format("Настройка экспоненты с периодом {0} отсутствует в MySettings.PRICE_SETTINGS!", n));
            }

            return _tactExpPrices.ContainsKey(n) ? _tactExpPrices[n].Get_Value(dt) : GetTactRealPrice(dt: dt);
        }

        // Snippets
        private DateTime GetDateTimeDivSeconds(DateTime dt, int seconds)
        {
            return dt.AddSeconds(-dt.TimeOfDay.TotalSeconds % seconds);
        }
        private DateTime GetDateTimeWithoutMillis(DateTime dt)
        {
            return GetDateTimeDivSeconds(dt, 1);
        }
        private DateTime GetTactDateTime(DateTime dt)
        {
            return GetDateTimeDivSeconds(dt, VV_TACT);
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
