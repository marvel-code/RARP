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
        public Boolean is_Green_Candle(int shift = 0, int cnt = 1)
        {
            if (Buffer.Count <= shift + cnt - 1) return false;
            for (int i = shift; i < shift + cnt; i++)
                if (getCandle(i).ClosePrice <= getCandle(i).OpenPrice) // Если не зелёная
                    return false;
            return true;
        }
        public Boolean is_Red_Candle(int shift = 0, int cnt = 1)
        {
            if (Buffer.Count <= shift + cnt - 1) return false;
            for (int i = shift; i < shift + cnt; i++)
                if (getCandle(i).ClosePrice >= getCandle(i).OpenPrice) // Если не красная
                    return false;
            return true;
        }
        public Boolean is_Doji(int shift = 0, Decimal filter = 0)
        {
            if (Buffer.Count <= shift) return false;
            return Math.Abs(getCandle(shift).OpenPrice - getCandle(shift).ClosePrice) <= filter;
        }

        public Decimal getCandleHLRange(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            return Math.Abs(getCandle(shift).OpenPrice - getCandle(shift).ClosePrice);
        }
        public Decimal Get_Candle_BodyCentre(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            if (is_Green_Candle(shift))
                return (getCandle(shift).OpenPrice + getCandle(shift).ClosePrice) / 2;
            else
                return (getCandle(shift).ClosePrice + getCandle(shift).OpenPrice) / 2;
        }

        public Decimal Get_Candle_HLRange(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            return getCandle(shift).HighPrice - getCandle(shift).LowPrice;
        }
        public Decimal Get_Candle_HLCentre(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            return (getCandle(shift).LowPrice + getCandle(shift).HighPrice) / 2;
        }

        public Decimal Get_Upper_TailRange(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            if (is_Green_Candle(shift))
                return getCandle(shift).HighPrice - getCandle(shift).ClosePrice;
            else
                return getCandle(shift).HighPrice - getCandle(shift).OpenPrice;
        }
        public Decimal Get_Lower_TailRange(int shift = 0)
        {
            if (Buffer.Count <= shift) return 0;
            if (is_Green_Candle(shift))
                return getCandle(shift).OpenPrice - getCandle(shift).LowPrice;
            else
                return getCandle(shift).ClosePrice - getCandle(shift).LowPrice;
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
                        if (getCandle(i).HighPrice >= getCandle(i - j).HighPrice && getCandle(i).HighPrice >= getCandle(i + j).HighPrice)
                            j++;
                        else
                            break;
                    if (j == side_candles_cnt + 1)
                        return getCandle(i).HighPrice;
                }
            }
            else
            { // Short
                for (int i = side_candles_cnt + 1; i < Buffer.Count; i++) // Ищем фрактальную свечу
                {
                    j = 1;
                    while (j <= side_candles_cnt) // Проверяем бока
                        if (getCandle(i).LowPrice <= getCandle(i - j).LowPrice && getCandle(i).LowPrice <= getCandle(i + j).LowPrice)
                            j++;
                        else
                            break;
                    if (j == side_candles_cnt + 1)
                        return getCandle(i).LowPrice;
                }
            }

            return trend_direction == Sides.Buy ? getCandle().HighPrice : getCandle().LowPrice;
        }

        public Boolean ComparePrevCandlesTails(Sides _Side, int _Candles_Count, Decimal _Price_Shift)
        {
            if (Buffer.Count < _Candles_Count + 1)
                return false;

            var Current_Price = getCandle().ClosePrice;
            if (_Side == Sides.Buy)
                // Buy
                for (int i = 1; i < _Candles_Count; i++)
                {
                    if (Current_Price < getCandle(i).HighPrice + _Price_Shift)
                        return false;
                }
            else
                // Sell
                for (int i = 1; i < _Candles_Count; i++)
                {
                    if (Current_Price > getCandle(i).LowPrice - _Price_Shift)
                        return false;
                }

            return true;
        }
        public Boolean IsEnterCandle(int shift = 0)
        {
            Candle _candle = getCandle(shift);
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
            Candle _candle = getCandle(shift);
            if (_candle == null)
                return false;

            var time = _processingData.LastExitTime;

            return
                time >= _candle.Time
                &&
                time < _candle.Time.Add(Period);
        }
    }
}
