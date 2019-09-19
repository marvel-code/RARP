using System;
using System.Collections.Generic;
using Ecng.Common;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using transportDataParrern;

namespace stocksharp.ServiceContracts
{
    public partial class ProcessingData
    {
        // >> Customizable strategy settings
        public List<int> tf_Periods = new List<int>
        {
           120,
           300
        };
        public List<ADX_Configuration> adx_cfgList = new List<ADX_Configuration>
        {
            new ADX_Configuration(
                    0, 3, MaType.Exponential
                ),
        };
        public List<BBW_Configuration> bbw_cfgList = new List<BBW_Configuration>
        {
            new BBW_Configuration(
                    0, 3, 2, CalculationType.Median, MaType.Simple
                ),
        };
        public List<KAMA_Configuration> kama_cfgList = new List<KAMA_Configuration>
        {
            new KAMA_Configuration(0, 3, CalculationType.Median, 2, 30)
        };
        public List<MA_Configuration> ma_cfgList = new List<MA_Configuration>
        {
            new MA_Configuration(0, 1, MaType.Simple, CalculationType.Median),
        };
        public List<ROC_Configuration> roc_cfgList = new List<ROC_Configuration>
        {
            new ROC_Configuration(1, 1, CalculationType.Median),
        };
        public List<Volume_Configuration> volume_cfgList = new List<Volume_Configuration>
        {
            new Volume_Configuration(0, 2, 1, 1),
            new Volume_Configuration(1, 2, 1, 1),
        };
    }

    public partial class WorkService
    {
        // Position_AvrTv_MAX, Position_AvrTv_MIN..............1...................................
        private decimal avrTv_4PositionMaxMin_1 =>
            tf[1].volume.GetAvrTv(1800, 0);

        // Position_AvrTv_MAX, Position_AvrTv_MIN..............2...................................
        private decimal avrTv_4PositionMaxMin_2 =>
            tf[1].volume.GetAvrTv(300, 0);

        // Position_AvrTv_MAX, Position_AvrTv_MIN..............3...................................
        private decimal avrTv_4PositionMaxMin_3 =>
            tf[1].volume.GetAvrTv(180, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN......1...........................................
        private decimal avrVv_4PositionMaxMin_1 =>
            tf[1].volume.GetAvrVv(1800, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN......2...........................................
        private decimal avrVv_4PositionMaxMin_2 =>
            tf[1].volume.GetAvrVv(300, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN......3...........................................
        private decimal avrVv_4PositionMaxMin_3 =>
            tf[1].volume.GetAvrVv(180, 0);

        private int avrTvPeriod_1 = 1800, // LOW
                    avrTvPeriod_2 = 300, // MID
                    avrTvPeriod_3 = 180; // HIGH

        private int avrVvPeriod_1 = 1800,
                    avrVvPeriod_2 = 300,
                    avrVvPeriod_3 = 180;


        //
        // Разрешение входа после выхода
        //

        private bool isEntryAllowedAfterExitRule =>
                    //-// Разрешение входа после выхода

                    true

                    //-//
                    && true;


        //
        // LONG.....................................................
        //

        private bool LongCommonRule =>

                    //-// LONG общее условие.....................................


                    tf[1].volume.GetAvrVv(300, 0) > new decimal(3.0)

                    &&
                    tf[1].volume.GetTactRealPrice(0) >= tf[1].volume.GetTactRealPrice(1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) >= tf[1].volume.GetTactRealPrice(4)
                    &&
                    tf[1].volume.GetTactRealPrice() >= tf[1].volume.GetTactRealPriceLocalMax()

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
           
//-// LONG добавочные условия...............................1

                    new List<bool>
                    {
                    tf[1].volume.GetAvrVv(1800, 0) > new decimal(1.0)                 //..1
                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 36)
                    &&
                    tf[1].volume.GetAvrTv(1800, 36) > tf[1].volume.GetAvrTv(1800, 72)

                    //true
                    },

         };


        //
        // SHORT..................................................
        //

        private bool ShortCommonRule =>

                    //-// SHORT общее условие..................................

                    tf[1].volume.GetAvrVv(300, 0) < -new decimal(3.0)

                    &&
                    tf[1].volume.GetTactRealPrice(0) <= tf[1].volume.GetTactRealPrice(1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) <= tf[1].volume.GetTactRealPrice(4)
                    &&
                    tf[1].volume.GetTactRealPrice() <= tf[1].volume.GetTactRealPriceLocalMin()

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {

//-// SHORT добавочные условия................................1

                    new List<bool>
                    {

                    tf[1].volume.GetAvrVv(1800, 0) < - new decimal(1.0)                 //..1
                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 36)
                    &&
                    tf[1].volume.GetAvrTv(1800, 36) > tf[1].volume.GetAvrTv(1800, 72)

                    },

        };


        //
        // Разрешения на вход .................................................
        //

        private bool isEntryAllowed =>

                  //-// На свече выхода, при достижении  , по времени суток ............. 

                  !tf[1].IsExitCandle()
                  &&
                  Current_Day_PNL < new decimal(1000)
                  &&
                  Current_Time.TimeOfDay > new TimeSpan(10, 10, 0)
                  &&
                  Current_Time.TimeOfDay < new TimeSpan(18, 30, 0)
                  &&
                (
                  Current_Time.TimeOfDay > new TimeSpan(11, 10, 0)
                  ||
                   (
                    tf[1].GetPriceChannelLow(2, 0) < tf[1].GetPriceChannelLow(2, 1) -
                    new decimal(0)

                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > new decimal(10)
                    &&
                    tf[1].volume.GetAvrTv(300, 0) > tf[1].volume.GetAvrTv(1800, 0)
                    &&
                       (
                       tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 12) +
                       new decimal(1.5)
                       ||
                       tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 18) +
                       new decimal(1.5)
                       ||
                       tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 24) +
                       new decimal(1.5)
                       )

                    &&
                    tf[1].volume.GetAvrVv(1800, 0) < -new decimal(1.0)
                    &&
                    tf[1].volume.GetAvrVv(300, 0) < tf[1].volume.GetAvrVv(1800, 0)
                    &&
                      (
                      tf[1].volume.GetAvrVv(1800, 0) < tf[1].volume.GetAvrVv(1800, 12) -
                      new decimal(0.45)
                      ||
                      tf[1].volume.GetAvrVv(1800, 0) < tf[1].volume.GetAvrVv(1800, 18) -
                      new decimal(0.45)
                      ||
                      tf[1].volume.GetAvrVv(1800, 0) < tf[1].volume.GetAvrVv(1800, 24) -
                      new decimal(0.45)
                      )
                    )

                    ||

                   (
                    tf[1].GetPriceChannelHigh(2, 0) > tf[1].GetPriceChannelHigh(2, 1) +
                    new decimal(0)

                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > new decimal(10)
                    &&
                    tf[1].volume.GetAvrTv(300, 0) > tf[1].volume.GetAvrTv(1800, 0)
                    &&
                       (
                       tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 12) +
                       new decimal(1.5)
                       ||
                       tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 18) +
                       new decimal(1.5)
                       ||
                       tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 24) +
                       new decimal(1.5)
                       )

                    &&
                    tf[1].volume.GetAvrVv(1800, 0) > new decimal(1.0)
                    &&
                    tf[1].volume.GetAvrVv(300, 0) > tf[1].volume.GetAvrVv(1800, 0)
                    &&
                      (
                      tf[1].volume.GetAvrVv(1800, 0) > tf[1].volume.GetAvrVv(1800, 12) +
                      new decimal(0.45)
                      ||
                      tf[1].volume.GetAvrVv(1800, 0) > tf[1].volume.GetAvrVv(1800, 18) +
                      new decimal(0.45)
                      ||
                      tf[1].volume.GetAvrVv(1800, 0) > tf[1].volume.GetAvrVv(1800, 24) +
                      new decimal(0.45)
                      )
                    )
            )
            // false
            //-//
            && true;

        private bool isLongAllowed =>
            //-// Запрет на LONG

            true

            //-//
            && true;

        private bool isShortAllowed =>
            //-// Запрет на SHORT

            true

            //-//
            && true;


        //
        // SELL..........................................................
        //

        private bool SellCommonRule =>

                    //-// SELL общее условие......................................

                    //                   tf[1].IsRedCandle(0)

                    //                   tf[1].volume.GetAvrVv(300, 0) < tf[1].volume.GetAvrVv(300, 12)
                    //                 &&
                    tf[1].volume.GetAvrVv(180, 0) < tf[1].volume.GetAvrVv(180, 1)
                    &&
                    tf[1].volume.GetAvrVv(300, 0) < tf[1].volume.GetAvrVv(300, 1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPrice(1)
            //          true
            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>
        {
//-// SELL дополнительные условия..............................

                    tf[1].volume.GetAvrTv(300, 0) > new decimal(15)                   //..1
                    &&
                    tf[1].volume.GetAvrTv(300, 0) > tf[1].volume.GetAvrTv(300, 18) +
                    new decimal(1)
                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 18)

                    &&
                    tf[1].volume.GetAvrVv(300, 0) < tf[1].volume.GetAvrVv(300, 18) -
                    new decimal(0.7)
                    &&
                    tf[1].volume.GetAvrVv(1800, 0) < tf[1].volume.GetAvrVv(1800, 18) -
                    new decimal(0.3)

                    ,
                    tf[1].volume.GetAvrVv(1800, 0) < new decimal(0.0)                 //..2
                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 36)
                    &&
                    tf[1].volume.GetAvrTv(1800, 36) > tf[1].volume.GetAvrTv(1800, 72)

                    ,
                    tf[1].volume.GetAvrVv(1800, 0) < new decimal(0.0)                 //..3
                    &&
                    tf[1].volume.GetAvrTv(300, 0) > new decimal(15)

        };


        //
        // COVER.........................................................
        //

        private bool CoverCommonRule =>

                    //-// COVER общее условие......................................

                    //                  tf[1].volume.GetAvrVv(300, 0) > tf[1].volume.GetAvrVv(300, 12)
                    //                &&
                    tf[1].volume.GetAvrVv(180, 0) > tf[1].volume.GetAvrVv(180, 1)
                    &&
                    tf[1].volume.GetAvrVv(300, 0) > tf[1].volume.GetAvrVv(300, 1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPrice(1)

            //   true
            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>
        {
//-// COVER дополнительные условия..............................



                    tf[1].volume.GetAvrTv(300, 0) > new decimal(15)                 //..1 
                    &&
                    tf[1].volume.GetAvrTv(300, 0) > tf[1].volume.GetAvrTv(300, 18) +
                    new decimal(1)
                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 18)

                    &&
                    tf[1].volume.GetAvrVv(300, 0) > tf[1].volume.GetAvrVv(300, 18) +
                    new decimal(0.7)
                    &&
                    tf[1].volume.GetAvrVv(1800, 0) > tf[1].volume.GetAvrVv(1800, 18) +
                    new decimal(0.3)


                    ,
                    tf[1].volume.GetAvrVv(1800, 0) > new decimal(0.0)                 //..2
                    &&
                    tf[1].volume.GetAvrTv(1800, 0) > tf[1].volume.GetAvrTv(1800, 36)
                    &&
                    tf[1].volume.GetAvrTv(1800, 36) > tf[1].volume.GetAvrTv(1800, 72)


                    ,
                    tf[1].volume.GetAvrVv(1800, 0) > new decimal(0.0)                 //..3
                    &&
                    tf[1].volume.GetAvrTv(300, 0) > new decimal(15)

        };
    }
}
