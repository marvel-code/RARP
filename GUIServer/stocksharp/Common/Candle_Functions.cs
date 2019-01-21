using System;

using StockSharp.Algo.Candles;
using StockSharp.BusinessEntities;

using StockSharp.Messages;


namespace stocksharp
{
    partial class TimeFrame
    {
        /**
         * Работа со свечами
         **/
        public Decimal GetCandleDuration(int shift = 0)
        {
            if (shift > 0)
            {
                return period;
            }

            if (shift == 0)
            {
                return (decimal)_processingData.TerminalTime.TimeOfDay.TotalSeconds % period;
            }

            return -1;
        }

        public Decimal GetCandleHigh(int shift = 0)
        {
            var candle = GetCandle(shift);
            if (candle == null) return 0;
            return candle.HighPrice;
        }
        public Decimal GetCandleLow(int shift = 0)
        {
            var candle = GetCandle(shift);
            if (candle == null) return 0;
            return candle.LowPrice;
        }

        public Boolean IsGreenCandle(int shift = 0, int cnt = 1)
        {
            if (Buffer.Count <= shift + cnt - 1) return false;
            for (int i = shift; i < shift + cnt; i++)
                if (GetCandle(i).ClosePrice <= GetCandle(i).OpenPrice) // Если не зелёная
                    return false;
            return true;
        }
        public Boolean IsRedCandle(int shift = 0, int cnt = 1)
        {
            if (Buffer.Count <= shift + cnt - 1) return false;
            for (int i = shift; i < shift + cnt; i++)
                if (GetCandle(i).ClosePrice >= GetCandle(i).OpenPrice) // Если не красная
                    return false;
            return true;
        }

        public Decimal GetCandleBodyRange(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            if (IsRedCandle(shift))
                return (GetCandle(shift).OpenPrice - GetCandle(shift).ClosePrice);
            else
                return (GetCandle(shift).ClosePrice - GetCandle(shift).OpenPrice);
        }
        public Decimal GetCandleBodyCentre(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;

            return (GetCandle(shift).OpenPrice + GetCandle(shift).ClosePrice) / 2;
        }

        public Decimal GetCandleHLRange(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            return GetCandle(shift).HighPrice - GetCandle(shift).LowPrice;
        }
        public Decimal GetCandleHLCentre(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            return (GetCandle(shift).LowPrice + GetCandle(shift).HighPrice) / 2;
        }

        public Decimal GetUpperShadow(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            if (IsGreenCandle(shift))
                return GetCandle(shift).HighPrice - GetCandle(shift).ClosePrice;
            else
                return GetCandle(shift).HighPrice - GetCandle(shift).OpenPrice;
        }
        public Decimal GetLowerShadow(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            if (IsGreenCandle(shift))
                return GetCandle(shift).OpenPrice - GetCandle(shift).LowPrice;
            else
                return GetCandle(shift).ClosePrice - GetCandle(shift).LowPrice;
        }


        public Decimal last_FractalPrice(Sides trend_direction, int side_candles_cnt = 2)
        {
            if (Buffer.Count <= side_candles_cnt * 2 + 1)
                return 0;

            int j;
            if (trend_direction == Sides.Buy)
            { // Long
                for (int i = side_candles_cnt + 1; i < Buffer.Count; i++) // Ищем фрактальную свечу
                {
                    j = 1;
                    while (j <= side_candles_cnt) // Проверяем бока
                        if (GetCandle(i).HighPrice >= GetCandle(i - j).HighPrice && GetCandle(i).HighPrice >= GetCandle(i + j).HighPrice)
                            j++;
                        else
                            break;
                    if (j == side_candles_cnt + 1)
                        return GetCandle(i).HighPrice;
                }
            }
            else
            { // Short
                for (int i = side_candles_cnt + 1; i < Buffer.Count; i++) // Ищем фрактальную свечу
                {
                    j = 1;
                    while (j <= side_candles_cnt) // Проверяем бока
                        if (GetCandle(i).LowPrice <= GetCandle(i - j).LowPrice && GetCandle(i).LowPrice <= GetCandle(i + j).LowPrice)
                            j++;
                        else
                            break;
                    if (j == side_candles_cnt + 1)
                        return GetCandle(i).LowPrice;
                }
            }

            return trend_direction == Sides.Buy ? GetCandle().HighPrice : GetCandle().LowPrice;
        }

        public Boolean ComparePrevCandlesTails(Sides _Side, int _Candles_Count, Decimal _Price_Shift)
        {
            if (Buffer.Count < _Candles_Count + 1)
                return false;

            var Current_Price = GetCandle().ClosePrice;
            if (_Side == Sides.Buy)
                // Buy
                for (int i = 1; i < _Candles_Count; i++)
                {
                    if (Current_Price < GetCandle(i).HighPrice + _Price_Shift)
                        return false;
                }
            else
                // Sell
                for (int i = 1; i < _Candles_Count; i++)
                {
                    if (Current_Price > GetCandle(i).LowPrice - _Price_Shift)
                        return false;
                }

            return true;
        }
        public Boolean IsEnterCandle(int shift = 0)
        {
            Candle _candle = GetCandle(shift);
            if (_candle == null)
                return false;

            var time = _processingData.LastEnterTime;

            return
                time >= _candle.Time
                &&
                time < _candle.Time.Add(Period);
        }
        public Boolean IsExitCandle(int shift = 0)
        {
            Candle _candle = GetCandle(shift);
            if (_candle == null)
                return false;

            var time = _processingData.LastExitTime;

            return
                time >= _candle.Time
                &&
                time < _candle.Time.Add(Period);
        }

        // PRICE CHANNEL

        public decimal GetPriceChannelWidth(int count, int shift = 0)
        {
            return GetPriceChannelHigh(count, shift) - GetPriceChannelLow(count, shift);
        }
        public decimal GetPriceChannelHigh(int count, int shift = 0)
        {
            decimal result = 0;
            Candle candle;
            for (int i = shift; i < count + shift; i++)
            {
                candle = GetCandle(i);
                if (candle == null)
                    break;

                if (candle.HighPrice > result)
                    result = candle.HighPrice;
            }

            return result;
        }
        public decimal GetPriceChannelMid(int count, int shift = 0)
        {
            return (GetPriceChannelHigh(count, shift) + GetPriceChannelLow(count, shift)) / 2;
        }
        public decimal GetPriceChannelLow(int count, int shift = 0)
        {
            decimal result = 999999;
            Candle candle;
            for (int i = shift; i < count + shift; i++)
            {
                candle = GetCandle(i);
                if (candle == null)
                    break;

                if (candle.LowPrice < result)
                    result = candle.LowPrice;
            }

            return result;
        }


        public Boolean IsDoji(int shift = 0, Decimal filter = 0)
        {
            if (Buffer.Count <= shift) return false;
            return Math.Abs(GetCandle(shift).OpenPrice - GetCandle(shift).ClosePrice) <= filter;
        }
    }
}
