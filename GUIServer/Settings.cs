﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stocksharp
{
    public static class MySettings
    {
        public const int VV_TACT = 5;
        public static KeyValuePair<int, int>[] VELOCITIES_SETTINGS = new KeyValuePair<int, int>[]
{
            // (period, n)
            // 0
            new KeyValuePair<int, int>(1800,    50),
            // 1
            new KeyValuePair<int, int>(900,     50),
            // 2
            new KeyValuePair<int, int>(300,     21),
            // 3
            new KeyValuePair<int, int>(180,     8),
};
        public static int[] PRICE_SETTINGS = new int[]
        {
            // (n)
            // 0
           500,
            // 1
           250,
            // 2
            125,
            // 3
            50,
        };
    }
}
