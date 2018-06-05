using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using System.Runtime.Serialization;

namespace stocksharp
{

    // >> Server



    // >> Indicators

    public class DecimalIndicatorValue
    {
        public Decimal Value { get; set; }

        public DateTime OpenTime { get; set; }

        public DecimalIndicatorValue(DateTime _OpenTime, Decimal _Value)
        {
            Value = _Value;
            OpenTime = _OpenTime;
        }
    }

    public enum CalculationType
    {
        Close,
        Median,
        HLC
    }

    public enum MaType
    {
        Simple,
        Exponential
    }
}