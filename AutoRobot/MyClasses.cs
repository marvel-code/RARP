using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

namespace AutoRobot
{
    /**
     *  Enum для рассчета индикатора. 
     **/
    public enum CalculationType
    {
        Close, Median, HLC
    }

    /**
     *  Класс предсталяет собой отображение значений некоторого индикатора.
     **/
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

    public enum Sides
    {
        Buy, Sell
    }

    public enum OrderType
    {
        Enter, Exit, Stop
    }

    public enum InOutConditions
    {
        LongIn, ShortIn, LongOut, ShortOut
    }

    public enum MyColors
    {
        Red, Yellow, Green, Blue, Cyan, White
    }

    public enum MaType
    {
        Simple,

        Exponential
    }
}
