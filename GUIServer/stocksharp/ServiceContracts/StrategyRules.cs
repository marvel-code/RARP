using System;
using System.Collections.Generic;
using Ecng.Common;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using transportDataParrern;
using Ecng.Common;

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
            new KAMA_Configuration(
                    1, 3, CalculationType.Median, 2, 30
                )
        };
        public List<MA_Configuration> ma_cfgList = new List<MA_Configuration>
        {
            new MA_Configuration(1, 1, MaType.Simple, CalculationType.Median),
            new MA_Configuration(1, 3, MaType.Simple, CalculationType.Median),
            new MA_Configuration(1, 12, MaType.Simple, CalculationType.Median),
        };
        public List<ROC_Configuration> roc_cfgList = new List<ROC_Configuration>
        {
            new ROC_Configuration(1, 1, CalculationType.Median),
            new ROC_Configuration(1, 2, CalculationType.Median),
        };
        public List<Volume_Configuration> volume_cfgList = new List<Volume_Configuration>
        {
            new Volume_Configuration(0, 2, 1, 1),
            new Volume_Configuration(1, 2, 1, 1),
        };
    }

    public partial class WorkService
    {
        // Position_AvrTv_MAX, Position_AvrTv_MIN.................................................
        private decimal avrTv_4PositionMaxMin_1 =>

            tf[1].volume.GetAvrTv(600, 0);
        // Position_AvrTv_MAX, Position_AvrTv_MIN.................................................
        private decimal avrTv_4PositionMaxMin_2 =>

            tf[1].volume.GetAvrTv(300, 0);
        // Position_AvrTv_MAX, Position_AvrTv_MIN.................................................
        private decimal avrTv_4PositionMaxMin_3 =>

            tf[1].volume.GetAvrTv(180, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN.................................................
        private decimal avrVv_4PositionMaxMin_1 =>

            tf[1].volume.GetAvrVv(600, 0);
        // Position_AvrVv_MAX, Position_AvrVv_MIN.................................................
        private decimal avrVv_4PositionMaxMin_2 =>

            tf[1].volume.GetAvrVv(300, 0);
        // Position_AvrVv_MAX, Position_AvrVv_MIN.................................................
        private decimal avrVv_4PositionMaxMin_3 =>

            tf[1].volume.GetAvrVv(180, 0);

        int avrTvPeriod_1 = 600, // LOW
            avrTvPeriod_2 = 300, // MID
            avrTvPeriod_3 = 180; // HIGH
        private bool Is_Tv_Crocodile(int shift = 0) =>
                tf[0].volume.GetAvrTv(avrTvPeriod_1, shift) < tf[0].volume.GetAvrTv(avrTvPeriod_2, shift) &&
                tf[0].volume.GetAvrTv(avrTvPeriod_2, shift) < tf[0].volume.GetAvrTv(avrTvPeriod_3, shift);
        private bool Are_Tv_CrocodileExtremums_Inited =>
            Crocodile_AvrTv1_MAX != 0 &&
            Crocodile_AvrTv2_MAX != 0 &&
            Crocodile_AvrTv3_MAX != 0 &&
            Crocodile_AvrTv1_MIN != 0 &&
            Crocodile_AvrTv2_MIN != 0 &&
            Crocodile_AvrTv3_MIN != 0;

        int avrVvPeriod_1 = 600,
            avrVvPeriod_2 = 300,
            avrVvPeriod_3 = 180;
        private bool Is_Vv_Crocodile(int shift = 0) =>
                tf[0].volume.GetAvrVv(avrVvPeriod_1, shift) < tf[0].volume.GetAvrVv(avrVvPeriod_2, shift) &&
                tf[0].volume.GetAvrVv(avrVvPeriod_2, shift) < tf[0].volume.GetAvrVv(avrVvPeriod_3, shift)
                ||
                tf[0].volume.GetAvrVv(avrVvPeriod_1, shift) > tf[0].volume.GetAvrVv(avrVvPeriod_2, shift) &&
                tf[0].volume.GetAvrVv(avrVvPeriod_2, shift) > tf[0].volume.GetAvrVv(avrVvPeriod_3, shift);
        private bool Are_Vv_CrocodileExtremums_Inited =>
            Crocodile_AvrVv1_MAX != 0 &&
            Crocodile_AvrVv2_MAX != 0 &&
            Crocodile_AvrVv3_MAX != 0 &&
            Crocodile_AvrVv1_MIN != 0 &&
            Crocodile_AvrVv2_MIN != 0 &&
            Crocodile_AvrVv3_MIN != 0;


        //
        // Разрешение входа после выхода
        //

        private bool isEntryAllowedAfterExitRule =>
                    //-// Разрешение входа после выхода

                    true

                    //-//
                    && true;


//
// LONG.......................................
//

        private bool LongCommonRule =>

                    //-// LONG общее условие......................

                    tf[0].GetPriceChannelHigh(1, 0) > tf[0].GetPriceChannelHigh(1, 1) +
                    new decimal(0)

                    &&

                    tf[1].roc[1].val > 0

                    &&


                    tf[1].ma[0].val > tf[1].ma[1].val      //..p=1>p=3
                    &&
                    tf[1].ma[1].val > tf[1].ma[2].val_p    //..p=3>p=12
                    &&
                    tf[1].ma[2].val > tf[1].ma[2].val_p + new decimal(10.0)

                    //                true

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>

                {
//-// LONG добавочные условия..................

/// Add............................................................................1
 
                    new List<bool>
                    {

                    tf[1].roc[0].val > new decimal(0.07)          //..per=1          //..1
                    &&
                    tf[1].roc[0].val > tf[1].roc[0].val_p

                    ,
                    tf[1].roc[1].val > new decimal(0.09)          //..per=2          //..2

                    ,
                    tf[1].roc[0].val > new decimal(0.04)          //..per=1          //..3
                    &&
                    tf[1].roc[0].val > tf[1].roc[0].val_p
                    &&
                    tf[1].volume.total >tf[1].volume.total_p * new decimal(2.0)

                    ,
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) *          //..4
                    new decimal(1.2)
                    &&
                    tf[1].volume.total > tf[1].volume.total_p * new decimal(2.0)
                    &&
                    tf[1].IsGreenCandle(0)
                   
   //true
                    },

/// Add............................................................................2
 
                    new List<bool>
                    {

                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(1.0)  //..1
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(1.0)

                    ,
                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(0.7)  //..2
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(0.7)
                    &&
                    tf[1].volume.total > new decimal(5000)

                    ,
                    tf[1].roc[0].val > new decimal(0.10)                             //..3
	            &&
                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(0.4)
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(0.4)
                    &&
                    tf[1].volume.vector > new decimal(600)
// true
                    },

//-// LONG добавочные условия...............................3

                    new List<bool>
                    {

                    tf[1].roc[1].val > tf[1].roc[1].val_p                            //..1

                    ,
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) *          //..2
                    new decimal(1.2)
                    &&
                    tf[1].volume.total > tf[1].volume.total_p * new decimal(2.0)
                    &&
                    tf[1].IsGreenCandle(0)

                    ,
                    tf[1].roc[0].val > new decimal(0.10)                             //..3
                    &&
                    tf[1].roc[0].val > tf[1].roc[0].val_p

                     //true
                    },

//-// LONG добавочные условия...............................4

                    new List<bool>
                    {

                    tf[1].GetPriceChannelHigh(6, 0) > tf[1].GetPriceChannelHigh(6, 1) +
                    new decimal(30)                                                  //..1

                    ,
                    tf[1].roc[0].val > new decimal(0.10)                             //..2

                     //true
                    },

//-// LONG добавочные условия...............................5

                    new List<bool>
                    {

                    tf[1].volume.GetAvrTv(300, 0) > new decimal(15.0)                //..1
                    &&
                    tf[1].volume.GetAvrTv(180, 0) > tf[1].volume.GetAvrTv(300, 0)
                    &&
                    tf[1].volume.GetAvrVv(300, 0) > new decimal(2.0)

                    ,
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) *          //..2
                    new decimal(1.4)
                    &&
                    tf[1].volume.total > tf[1].volume.total_p * new decimal(2.0)
                    &&
                    tf[1].IsGreenCandle(0)
                    &&
                    tf[1].GetCandleDuration(0) != 0
                    &&
                    tf[1].GetCandleHLRange(0) / tf[1].GetCandleDuration(0) >
                    (tf[1].GetCandleHLRange(1) / tf[1].period) * new decimal(1.5)
                    &&
                    tf[1].volume.vector > new decimal(1500)

                     //true
                    },

           //-//
                };


//
// SHORT......................................
//

        private bool ShortCommonRule =>

                    //-// SHORT общее условие....................

                    tf[0].GetPriceChannelLow(1, 0) < tf[0].GetPriceChannelLow(1, 1) -
                    new decimal(0)

                    &&

                    tf[1].GetPriceChannelWidth(24, 0) < new decimal(1400)

                    &&

                    tf[1].roc[1].val < 0

                    &&

                    tf[1].ma[0].val < tf[1].ma[1].val      //..p=1<p=3
                    &&
                    tf[1].ma[1].val < tf[1].ma[2].val_p    //..p=3<p=12
                    &&
                    tf[1].ma[2].val < tf[1].ma[2].val_p - new decimal(10.0)

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {
//-// SHORT добавочные условия................

/// Add............................................................................1
 
                    new List<bool>
                    {

                    tf[1].roc[0].val < - new decimal(0.07)          //..per=1          //..1
                    &&
                    tf[1].roc[0].val < tf[1].roc[0].val_p

                    ,
                    tf[1].roc[1].val < - new decimal(0.09)          //..per=2          //..2

                    ,
                    tf[1].roc[0].val < - new decimal(0.04)          //..per=1          //..3
                    &&
                    tf[1].roc[0].val < tf[1].roc[0].val_p
                    &&
                    tf[1].volume.total >tf[1].volume.total_p * new decimal(2.0)

                    ,
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) *            //..4
                    new decimal(1.2)
                    &&
                    tf[1].volume.total > tf[1].volume.total_p * new decimal(2.0)
                    &&
                    tf[1].IsRedCandle(0)
// true
                    },

/// Add............................................................................2
 
                    new List<bool>
                    {

                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(1.0)    //..1
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(1.0)

                    ,
                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(0.7)    //..2
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(0.7)
                    &&
                    tf[1].volume.total > new decimal(5000)

                    ,
                    tf[1].roc[0].val < - new decimal(0.10)                            //..3
		            &&
                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(0.4)
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(0.4)
                    &&
                    tf[1].volume.vector < - new decimal(600)
// true
                    },

//-// SHORT добавочные условия................................3

                    new List<bool>
                    {

                    tf[1].roc[1].val < tf[1].roc[1].val_p                            //..1

                    ,
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) *          //..2
                    new decimal(1.2)
                    &&
                    tf[1].volume.total > tf[1].volume.total_p * new decimal(2.0)
                    &&
                    tf[1].IsRedCandle(0)

                    ,
                    tf[1].roc[0].val < - new decimal(0.10)                           //..3
                    &&
                    tf[1].roc[0].val < tf[1].roc[0].val_p

                    },

//-// SHORT добавочные условия................................4

                    new List<bool>
                    {

                    tf[1].GetPriceChannelLow(6, 0) < tf[1].GetPriceChannelLow(6, 1) -
                    new decimal(30)                                                  //..1

                    ,
                    tf[1].roc[0].val < - new decimal(0.10)                           //..2

                    },

//-// SHORT добавочные условия................................5

                    new List<bool>
                    {

                    tf[1].volume.GetAvrTv(300, 0) > new decimal(15.0)               //..1
                    &&
                    tf[1].volume.GetAvrTv(180, 0) > tf[1].volume.GetAvrTv(300, 0)
                    &&
                    tf[1].volume.GetAvrVv(300, 0) < - new decimal(3.0)

                    ,
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) *          //..2
                    new decimal(1.4)
                    &&
                    tf[1].volume.total > tf[1].volume.total_p * new decimal(2.0)
                    &&
                    tf[1].IsRedCandle(0)
                    &&
                    tf[1].GetCandleDuration(0) != 0
                    &&
                    tf[1].GetCandleHLRange(0) / tf[1].GetCandleDuration(0) >
                    (tf[1].GetCandleHLRange(1) / tf[1].period) * new decimal(1.5)
                    &&
                    tf[1].volume.vector < - new decimal(1500)

                    },
            //-//
        };


//
// Разрешение на вход...........................................
//

            private bool isEntryAllowed =>

//-// Разрешение на вход на свече выхода

            !tf[1].IsExitCandle()
            &&
            _currentData.TerminalTime.TimeOfDay > new TimeSpan(10, 30, 00)
            &&
            _currentData.TerminalTime.TimeOfDay < new TimeSpan(18, 30, 00)

            //-//
            && true;

        private bool isLongAllowed =>
            //-// Разрешение на LONG

            true

            //-//
            && true;

        private bool isShortAllowed =>
            //-// Разрешение на SHORT

            true

            //-//
            && true;


//
// SELL....................................
//

        private bool SellCommonRule =>
//-// SELL общее условие...................

                   // tf[1].IsRedCandle(0)
            true
            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>
        {
//-// SELL дополнительные условия.........
           

                    tf[1].volume.GetAvrVv(300, 0) < tf[1].volume.GetAvrVv(600, 0)       //..1

                    ,
                    Is_Position
                    &&
                    tf[1].volume.GetAvrVv(300, 0) < Position_AvrVv1_MAX *                //..2
                    new decimal(0.85)

            //-//
        };


//
// COVER....................................
//

        private bool CoverCommonRule =>
//-// COVER общее условие..................

           //         tf[1].IsGreenCandle(0)
            true
            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>
        {
//-// COVER дополнительные условия..........


                    tf[1].volume.GetAvrVv(300, 0) > - tf[1].volume.GetAvrVv(600, 0)       //..1

                    ,
                    Is_Position
                    &&
                    tf[1].volume.GetAvrVv(300, 0) > Position_AvrVv1_MIN *                //..2
                    new decimal(0.85)

            //-//
        };
    }
}
