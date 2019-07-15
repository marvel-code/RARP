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
        // Position_AvrTv_MAX, Position_AvrTv_MIN..............1...................................
        private decimal avrTv_4PositionMaxMin_1 =>
            tf[0].volume.GetAvrTv(1800, 0);

        // Position_AvrTv_MAX, Position_AvrTv_MIN..............2...................................
        private decimal avrTv_4PositionMaxMin_2 =>
            tf[0].volume.GetAvrTv(900, 0);

        // Position_AvrTv_MAX, Position_AvrTv_MIN..............3...................................
        private decimal avrTv_4PositionMaxMin_3 =>
            tf[0].volume.GetAvrTv(300, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN......1...........................................
        private decimal avrVv_4PositionMaxMin_1 =>
            tf[0].volume.GetAvrVv(1800, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN......2...........................................
        private decimal avrVv_4PositionMaxMin_2 =>
            tf[0].volume.GetAvrVv(900, 0);

        // Position_AvrVv_MAX, Position_AvrVv_MIN......3...........................................
        private decimal avrVv_4PositionMaxMin_3 =>
            tf[0].volume.GetAvrVv(300, 0);

        int avrTvPeriod_1 = 1800, // LOW
            avrTvPeriod_2 = 900, // MID
            avrTvPeriod_3 = 300; // HIGH

        int avrVvPeriod_1 = 1800,
            avrVvPeriod_2 = 900,
            avrVvPeriod_3 = 300;


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


                    tf[0].volume.GetAvrTv(300, 0) > new decimal(15.0)
                    &&
                    tf[0].volume.GetAvrTv(180, 0) > tf[0].volume.GetAvrTv(300, 0)

                    &&
                    tf[0].volume.GetAvrVv(1800, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(600, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(300, 0)

                    &&
                    tf[0].volume.GetTactRealPrice(0) >= tf[0].volume.GetTactRealPrice(1)
                    &&
                    tf[0].volume.GetTactRealPrice(0) >= tf[0].volume.GetTactRealPrice(4)
                    &&
                    tf[0].volume.GetTactRealPrice() >= tf[0].volume.GetTactRealPriceLocalMax()
                    &&
                    tf[0].volume.GetTactRealPrice(0) < tf[0].volume.GetTactRealPrice(2) +
                    new decimal(150)



                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
           
//-// LONG добавочные условия...............................1

                    new List<bool>
                    {
                    tf[0].GetPriceChannelHigh(1, 0) > tf[0].GetPriceChannelHigh(1, 1) +
                    new decimal(0)                                                  //..1

                    ,
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(20)                //..2

                    //true
                    },

//-// LONG добавочные условия...............................2

                    new List<bool>
                    {

                    tf[0].volume.GetAvrTv(1800, 0) >= tf[0].volume.GetAvrTv(1800, 1)//..1
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) >= tf[0].volume.GetAvrTv(1800, 4)

                    ,
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(20)                //..2

                    //true
                    },

//-// LONG добавочные условия...............................3

                    new List<bool>
                    {

                    tf[0].roc[0].val > new decimal(0.08)                            //..1
                    &&
                    tf[0].roc[0].val > tf[0].roc[0].val_p
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > new decimal(0.3)

                    ,
                    tf[0].roc[0].val > new decimal(0.06)                            //..2
                    &&
                    tf[0].roc[0].val > tf[0].roc[0].val_p
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > new decimal(3.0)

                    ,
                    tf[0].roc[0].val > new decimal(0.05)                            //..3
                    &&
                    tf[0].roc[0].val > tf[0].roc[0].val_p
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(10)

                    //true
                    },
        //-//
         };


        //
        // SHORT..................................................
        //

        private bool ShortCommonRule =>

                    //-// SHORT общее условие..................................

                    tf[0].volume.GetAvrTv(300, 0) > new decimal(15.0)
                    &&
                    tf[0].volume.GetAvrTv(180, 0) > tf[0].volume.GetAvrTv(300, 0)

                    &&
                    tf[0].volume.GetAvrVv(1800, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(600, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(300, 0)

                    &&
                    tf[0].volume.GetTactRealPrice(0) <= tf[0].volume.GetTactRealPrice(1)
                    &&
                    tf[0].volume.GetTactRealPrice(0) <= tf[0].volume.GetTactRealPrice(4)
                    &&
                    tf[0].volume.GetTactRealPrice() <= tf[0].volume.GetTactRealPriceLocalMin()
                    &&
                    tf[0].volume.GetTactRealPrice(0) > tf[0].volume.GetTactRealPrice(2) -
                    new decimal(150)

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {

//-// SHORT добавочные условия................................1

                    new List<bool>
                    {

                    tf[0].GetPriceChannelLow(1, 0) < tf[0].GetPriceChannelLow(1, 1) -
                    new decimal(0)                                                  //..1

                    ,
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(20)                //..2

                    //true
                    },

//-// SHORT добавочные условия...............................2

                    new List<bool>
                    {

                    tf[0].volume.GetAvrTv(1800, 0) >= tf[0].volume.GetAvrTv(1800, 1)//..1
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) >= tf[0].volume.GetAvrTv(1800, 4)

                    ,
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(20)                //..2

                    //true
                    },

//-// SHORT добавочные условия...............................3

                    new List<bool>
                    {

                    tf[0].roc[0].val < -new decimal(0.08)                           //..1
                    &&
                    tf[0].roc[0].val < tf[0].roc[0].val_p
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < - new decimal(0.3)

                    ,
                    tf[0].roc[0].val < -new decimal(0.06)                           //..2
                    &&
                    tf[0].roc[0].val < tf[0].roc[0].val_p
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < -new decimal(3.0)

                    ,
                    tf[0].roc[0].val < -new decimal(0.05)                           //..3   
                    &&
                    tf[0].roc[0].val < tf[0].roc[0].val_p
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(10)                

                    //true
                    },

            //-//
        };


        //
        // Запреты на вход на свече выхода.............................
        //

        private bool isEntryAllowed =>
                  //-// Запрет на вход 

                  !tf[0].IsExitCandle()
                  &&
                  Current_Day_PNL < new decimal(1000)
                  &&
                  Current_Time.TimeOfDay > new TimeSpan(10, 30, 0)
                  &&
                  Current_Time.TimeOfDay < new TimeSpan(18, 30, 0)
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
          /*       
                      tf[0].volume.GetAvrVv(300, 0) < 0
                      &&
                      tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(300, 0)
           */          true
            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>
        {
//-// SELL дополнительные условия..............................

                
                    tf[0].volume.GetAvrTv(1800, 1) > new decimal(10.0)              //..1
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) < new decimal(10.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(300, 0)


                    ,
                    tf[0].volume.GetAvrVv(1800, 0) < Position_AvrVv1_MAX *          //..2
                    new decimal(0.6)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(300, 0)


                    ,
                    tf[0].volume.GetAvrTv(1800, 0) > tf[0].volume.GetAvrTv(1800, 12)//..3
                    &&
                    tf[0].volume.GetAvrTv(1800, 12) > tf[0].volume.GetAvrTv(1800, 24)
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) > tf[0].volume.GetAvrTv(1800, 24) +
                    new decimal(0.45)
                    &&
                    tf[0].volume.GetAvrVv(1800, 0) < tf[0].volume.GetAvrVv(1800, 12)
                    &&
                    tf[0].volume.GetAvrVv(1800, 12) < tf[0].volume.GetAvrVv(1800, 24)
                    &&
                    tf[0].volume.GetAvrVv(1800, 0) < tf[0].volume.GetAvrVv(1800, 24) -
                    new decimal(0.35)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(300, 0)


                    ,
                    tf[0].GetPriceChannelLow(1, 0) < tf[0].GetPriceChannelLow(1, 1) -//..4
                    new decimal(0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(180, 4)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrVv(300, 4)


                    ,
                    tf[0].volume.GetAvrTv(300, 0) < new decimal(40)                //..5
                    &&
                    Position_PNL_MAX >= new decimal(500)
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(20)


                    ,
                    tf[0].volume.GetAvrTv(300, 0) > new decimal(40)                //..6
                    &&
                    Position_PNL_MAX >= new decimal(1000)
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(50)


                    ,
                    Position_PNL <= Position_PNL_MAX - new decimal(500)            //..7



/*                    ,
                    tf[0].volume.GetAvrVv(180, 0) < 0                                   //..4
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(300, 0)
                    &&
                    tf[0].volume.GetAvrTv(180, 0) > tf[0].volume.GetAvrTv(300, 0)
                    &&
                    tf[0].GetPriceChannelLow(1, 0) < tf[0].GetPriceChannelLow(1, 1) -
                    new decimal(0)                                                  

                    ,
                    tf[0].volume.GetAvrVv(1800, 0) < tf[0].volume.GetAvrVv(1800, 12)    //..1
                    &&
                    tf[0].volume.GetAvrVv(1800, 12) < tf[0].volume.GetAvrVv(1800, 24)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrVv(1800, 0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrVv(1800, 0)

                    ,
                    tf[0].volume.GetAvrVv(300, 0) < new decimal(10)                     //..2
                    &&
                    tf[0].volume.GetAvrVv(300, 1) > new decimal(10)

                    ,
                    tf[0].volume.GetAvrVv(300, 0) < new decimal(10)                     //..3
                    &&
                    tf[0].volume.GetAvrVv(300, 2) > new decimal(10)
/*
                    ,
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrVv(1800, 0)   //..1
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(20.0)




*/
        };


        //
        // COVER.........................................................
        //

        private bool CoverCommonRule =>
                  //-// COVER общее условие......................................
                  /*             
                                    tf[0].volume.GetAvrVv(300, 0) > 0
                                    &&
                                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(300, 0)
                          */   true
            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>
        {
//-// COVER дополнительные условия..............................


                    tf[0].volume.GetAvrTv(1800, 1) > new decimal(10.0)                //..1
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) < new decimal(10.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(300, 0)


                    ,
                    tf[0].volume.GetAvrVv(1800, 0) > Position_AvrVv1_MIN *            //..2
                    new decimal(0.6)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(300, 0)


                    ,
                    tf[0].volume.GetAvrTv(1800, 0) > tf[0].volume.GetAvrTv(1800, 12)  //..3
                    &&
                    tf[0].volume.GetAvrTv(1800, 12) > tf[0].volume.GetAvrTv(1800, 24)
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) > tf[0].volume.GetAvrTv(1800, 24) +
                    new decimal(0.45)
                    &&
                    tf[0].volume.GetAvrVv(1800, 0) > tf[0].volume.GetAvrVv(1800, 12)
                    &&
                    tf[0].volume.GetAvrVv(1800, 12) > tf[0].volume.GetAvrVv(1800, 24)
                    &&
                    tf[0].volume.GetAvrVv(1800, 0) > tf[0].volume.GetAvrVv(1800, 24) +
                    new decimal(0.35)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(300, 0)


                    ,
                    tf[0].GetPriceChannelHigh(1, 0) > tf[0].GetPriceChannelHigh(1, 1) + //..4
                    new decimal(0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(180, 4)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrVv(300, 4)


                    ,
                    tf[0].volume.GetAvrTv(300, 0) < new decimal(40)                     //..5
                    &&
                    Position_PNL_MAX >= new decimal(500)
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(20)


                    ,
                    tf[0].volume.GetAvrTv(300, 0) > new decimal(40)                     //..6
                    &&
                    Position_PNL_MAX >= new decimal(1000)
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(50)


                    ,
                    Position_PNL <= Position_PNL_MAX - new decimal(500)                 //..7


  /*                  ,
                    tf[0].volume.GetAvrVv(180, 0) > 0                                   //..4
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > 0
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(300, 0)
                    &&
                    tf[0].volume.GetAvrTv(180, 0) > tf[0].volume.GetAvrTv(300, 0)
                    &&
                    tf[0].GetPriceChannelHigh(1, 0) > tf[0].GetPriceChannelHigh(1, 1) +
                    new decimal(0)   
/*
                    ,
                    tf[0].volume.GetAvrVv(1800, 0) > tf[0].volume.GetAvrVv(1800, 12)    //..1
                    &&
                    tf[0].volume.GetAvrVv(1800, 12) > tf[0].volume.GetAvrVv(1800, 24)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrVv(1800, 0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrVv(1800, 0)

                    ,
                    tf[0].volume.GetAvrVv(300, 0) > - new decimal(10)                   //..2
                    &&
                    tf[0].volume.GetAvrVv(300, 1) < - new decimal(10)

                    ,
                    tf[0].volume.GetAvrVv(300, 0) > - new decimal(10)                   //..3
                    &&
                    tf[0].volume.GetAvrVv(300, 2) < - new decimal(10)

                    ,
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrVv(1800, 0)   //..1
                    &&
                    tf[0].volume.GetAvrTv(1800, 0) > new decimal(20.0)








*/
            //-//
        };
    }
}
