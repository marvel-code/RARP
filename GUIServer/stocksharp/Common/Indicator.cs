using System;
using StockSharp.Algo.Candles;

namespace stocksharp
{
    public class Indicator
    {
        public Decimal Get_Calced_Value(Candle _Candle, CalculationType _Calc_Type)
        {
            if (_Calc_Type == CalculationType.Close) return _Candle.ClosePrice;
            else if (_Calc_Type == CalculationType.Median) return (_Candle.HighPrice + _Candle.LowPrice) / 2;
            else if (_Calc_Type == CalculationType.HLC) return (_Candle.HighPrice + _Candle.LowPrice + _Candle.ClosePrice) / 3;

            return 0;
        }
    }
}
