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

                    /*
                    PositionPrices.ExpReal[0].Max.Value - в позиции   

                    PositionPrices.Real.Max.Value       - в позиции

                    tf[1].volume.GetTactExpPriceTactsMax(5, 24, 0) - Максимум экспоненты с коэфициентом 5 цены за 24 такта с 0 такта

                    tf[1].volume.GetExpTv(300, 24, tf[1].volume.GetExpTactsVvMaxTactsDuration(300, 24, 24))  - 
                    значение экспон Tv с момента локального минимума Vv (300-период, 24-коэфф експ, 24-период, в котором есть локал мин)
                    */

                    tf[1].GetCandleHigh(0) > tf[1].GetCandleHigh(1)
                    &&
                    tf[1].GetPriceChannelWidth(2, 0) > new decimal(200)
                    &&
                    tf[1].GetPriceChannelWidth(6, 0) < new decimal(1200)

                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPrice(2)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPrice(1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPriceTactsMax(60, 1) +
                    new decimal(0.0)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPrice(2) +
                    new decimal(150)
                    &&
                    tf[1].volume.GetTactExpPrice(500, 0) > tf[1].volume.GetTactExpPrice(500, 6)

                    &&
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(10.0)

                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > new decimal(0.0)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) > new decimal(0.0)

                    //  true

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
           
//-// LONG добавочные условия........................................................................1

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(1800, 50, 2) > tf[1].volume.GetExpTv(1800, 50, 4)    //..1
                    &&
                    tf[1].volume.GetExpTv(1800, 50, 0) > tf[1].volume.GetExpTv(1800, 50, 2)


                    ,
                    tf[1].volume.GetExpTv(1800, 50, 0) > new decimal(10.0)                     //..2


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..3

                    },

//-// LONG добавочные условия........................................................................2

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(900, 50, 2) > tf[1].volume.GetExpTv(900, 50, 4)      //..1
                    &&
                    tf[1].volume.GetExpTv(900, 50, 0) > tf[1].volume.GetExpTv(900, 50, 2)


                    ,
                    tf[1].volume.GetExpTv(900, 50, 0) > new decimal(10.0)                      //..2


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..3

                    },

//-// LONG добавочные условия........................................................................3

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(900, 50, 0) > tf[1].volume.GetExpTv(1800, 50, 0)     //..1


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..2

                    },

//-// LONG добавочные условия........................................................................4

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(1800, 50, 0)     //..1


                    ,
                    tf[1].volume.GetExpTv(1800, 50, 0) > new decimal(10.0)                     //..2


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..3

                    },

//-// LONG добавочные условия........................................................................5

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(300, 21, 1) > tf[1].volume.GetExpTv(300, 21, 2)      //..1
                    &&
                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(300, 21, 1)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 1) > tf[1].volume.GetExpTv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(180, 8, 1)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(300, 21, 0)


                    ,
                    tf[1].volume.GetAvrVv(180, 0) > new decimal(5.0)                           //..2

                    },

//-// LONG добавочные условия........................................................................6

                    new List<bool>
                    {

                    tf[1].volume.GetExpVv(900, 50, 1) > tf[1].volume.GetExpVv(900, 50, 2)      //..1
                    &&
                    tf[1].volume.GetExpVv(900, 50, 0) > tf[1].volume.GetExpVv(900, 50, 1)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 1) > tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > tf[1].volume.GetExpVv(300, 21, 1)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 1) > tf[1].volume.GetExpVv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) > tf[1].volume.GetExpVv(180, 8, 1)


                    ,
                    tf[1].volume.GetAvrVv(180, 0) > new decimal(5.0)                           //..2

                    },

//-// LONG добавочные условия........................................................................7

                    new List<bool>
                    {

                    tf[1].volume.GetExpVv(900, 50, 0) > tf[1].volume.GetExpVv(1800, 50, 0)     //..1


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..2             

                    },

//-// LONG добавочные условия........................................................................8

                    new List<bool>
                    {

                    tf[1].volume.GetExpVv(1800, 50, 0) > new decimal(0.0)                      //..1
                    &&
                    tf[1].volume.GetExpVv(900, 50, 0) > new decimal(0.0)


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..2


                    ,
                    tf[1].volume.GetAvrVv(180, 0) > new decimal(5.0)                           //..3

                    },

//-// LONG добавочные условия........................................................................9

                    new List<bool>
                    {

                    tf[1].volume.GetAvrTv(180, 0) > tf[1].volume.GetAvrTactsTvMax(180, 60, 1)  //..1


                    ,
                    tf[1].volume.GetAvrVv(180, 0) > tf[1].volume.GetAvrTactsVvMax(180, 60, 1)  //..2


                    ,
                    tf[1].volume.vector_h > new decimal(1000)                                  //..3
                    &&
                    tf[1].volume.vector_hp >  new decimal(1000)
                    &&
                    tf[1].volume.GetAvrVv(180, 0) > new decimal (5.0)

                    },

        //-//
                };


        //
        // SHORT..................................................
        //

        private bool ShortCommonRule =>

                    //-// SHORT общее условие..................................

                    tf[1].GetCandleLow(0) < tf[1].GetCandleLow(1)
                    &&
                    tf[1].GetPriceChannelWidth(2, 0) > new decimal(200)
                    &&
                    tf[1].GetPriceChannelWidth(6, 0) < new decimal(1200)

                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPrice(2)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPrice(1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPriceTactsMin(60, 1) -
                    new decimal(0.0)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPrice(2) -
                    new decimal(150)
                    &&
                    tf[1].volume.GetTactExpPrice(500, 0) < tf[1].volume.GetTactExpPrice(500, 6)

                    &&
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(10.0)

                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < new decimal(0.0)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < new decimal(0.0)

            //           true

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {

//-// SHORT добавочные условия.......................................................................1

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(1800, 50, 2) > tf[1].volume.GetExpTv(1800, 50, 4)    //..1
                    &&
                    tf[1].volume.GetExpTv(1800, 50, 0) > tf[1].volume.GetExpTv(1800, 50, 2)


                    ,
                    tf[1].volume.GetExpTv(1800, 50, 0) > new decimal(10.0)                         //..2


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..3

                    },

//-// SHORT добавочные условия.......................................................................2

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(900, 50, 2) > tf[1].volume.GetExpTv(900, 50, 4)      //..1
                    &&
                    tf[1].volume.GetExpTv(900, 50, 0) > tf[1].volume.GetExpTv(900, 50, 2)


                    ,
                    tf[1].volume.GetExpTv(900, 50, 0) > new decimal(10.0)                      //..2


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..3

                    },

//-// SHORT добавочные условия.......................................................................3

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(900, 50, 0) > tf[1].volume.GetExpTv(1800, 50, 0)     //..1


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..2

                    },

//-// SHORT добавочные условия.......................................................................4

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(1800, 50, 0)     //..1


                    ,
                    tf[1].volume.GetExpTv(1800, 50, 0) > new decimal(10.0)                     //..2


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..3

                    },

//-// SHORT добавочные условия.......................................................................5

                    new List<bool>
                    {

                    tf[1].volume.GetExpTv(300, 21, 1) > tf[1].volume.GetExpTv(300, 21, 2)      //..1
                    &&
                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(300, 21, 1)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 1) > tf[1].volume.GetExpTv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(180, 8, 1)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(300, 21, 0)


                    ,
                    tf[1].volume.GetAvrVv(180, 0) < - new decimal(5.0)                         //..2

                    },

//-// SHORT добавочные условия.......................................................................6

                    new List<bool>
                    {

                    tf[1].volume.GetExpVv(900, 50, 1) < tf[1].volume.GetExpVv(900, 50, 2)      //..1
                    &&
                    tf[1].volume.GetExpVv(900, 50, 0) < tf[1].volume.GetExpVv(900, 50, 1)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 1) < tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < tf[1].volume.GetExpVv(300, 21, 1)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 1) < tf[1].volume.GetExpVv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < tf[1].volume.GetExpVv(180, 8, 1)


                    ,
                    tf[1].volume.GetAvrVv(180, 0) < - new decimal(5.0)                         //..2

                    },
//-// SHORT добавочные условия.......................................................................7

                    new List<bool>
                    {

                    tf[1].volume.GetExpVv(900, 50, 0) < tf[1].volume.GetExpVv(1800, 50, 0)     //..1


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..2

                    },

//-// SHORT добавочные условия.......................................................................8

                    new List<bool>
                    {

                    tf[1].volume.GetExpVv(1800, 50, 0) < new decimal(0.0)                      //..1
                    &&
                    tf[1].volume.GetExpVv(900, 50, 0) < new decimal(0.0)


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(20.0)                          //..2


                    ,
                    tf[1].volume.GetAvrVv(180, 0) < -new decimal(5.0)                          //..3

                    },

//-// SHORT добавочные условия.......................................................................9

                    new List<bool>
                    {

                    tf[1].volume.GetAvrTv(180, 0) > tf[1].volume.GetAvrTactsTvMax(180, 60, 1)  //..1


                    ,
                    tf[1].volume.GetAvrVv(180, 0) < tf[1].volume.GetAvrTactsVvMin(180, 60, 1)  //..2


                    ,
                    tf[1].volume.vector_l < - new decimal(1000)                                //..3
                    &&
                    tf[1].volume.vector_lp < -  new decimal(1000)
                    &&
                    tf[1].volume.GetAvrVv(180, 0) < - new decimal (5.0)

                    },
            //-//
        };


        //
        // Разрешения на вход .................................................
        //

        private bool isEntryAllowed =>

            true

            //-//
            && true;

        private bool isLongAllowed =>

                  //-// Разрешение на LONG.............................................

                  !tf[1].IsExitCandle(0)
                  //    &&
                  //  Current_Day_PNL < new decimal(700)    //  Current_Day_PNL < new decimal(500)
                  &&
                  Current_Time.TimeOfDay > new TimeSpan(10, 05, 0)
                  &&
                  Current_Time.TimeOfDay < new TimeSpan(18, 37, 0)
            // true

            //-//
            && true;

        private bool isShortAllowed =>

                  //-// Разрешение на SHORT...........................................

                  !tf[1].IsExitCandle(0)
                  //       &&
                  //     Current_Day_PNL < new decimal(700)  //  Current_Day_PNL < new decimal(500)
                  &&
                  Current_Time.TimeOfDay > new TimeSpan(10, 05, 0)
                  &&
                  Current_Time.TimeOfDay < new TimeSpan(18, 37, 0)

            // true

            //-//
            && true;


        //
        // SELL..........................................................
        //

        private bool SellCommonRule =>

                    //-// SELL общее условие.........................................

                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPrice(2)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPrice(1)
            //           &&
            //         tf[1].volume.GetAvrVv(180, 0) < 0
            //  true
            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>

        {

//-// SELL дополнительные условия..............................


                    Position_PNL_MAX >= new decimal(500)                                              //..1


                    ,
                    Position_PNL_MAX >= new decimal(350)                                              //..2
                    &&
                    Position_PNL_MAX < new decimal(500)
                    &&
                    Position_PNL < Position_PNL_MAX * new decimal(1.5) - new decimal(250)


                    ,
                    Position_PNL_MAX >= new decimal(200)                                              //..3
                    &&
                    Position_PNL_MAX < new decimal(300)
                    &&
                    Position_PNL < Position_PNL_MAX - new decimal(250)


                    ,
                    Position_PNL_MAX < new decimal(200)                                               //..4
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(300)
                    &&
                    tf[1].volume.GetExpVv(900, 50, 0) < tf[1].volume.GetExpVv(900, 50, 2)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < tf[1].volume.GetExpVv(180, 8, 2)


                    ,
                    tf[1].volume.GetAvrVv(180) < - new decimal(5.5)                                   //..5
                    &&
                    tf[1].volume.GetAvrVv(180, 0) < tf[1].volume.GetAvrTactsVvMin(180, 12, 1)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 1) > tf[1].volume.GetExpTv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(180, 8, 1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPriceTactsMin(36, 1) +
                    new decimal(0.0)


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(7.0)                                  //..6  
                    &&
                    tf[1].volume.total > tf[1].volume.total_p
                    &&
                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(1.0)
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(1.0)
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPriceTactsMin(24, 1)
                    &&
                    tf[1].IsRedCandle(0)


                    ,
                    tf[1].volume.total + tf[1].volume.total_p > tf[1].volume.total_pp                 //..7  
                    &&
                    tf[1].volume.vector + tf[1].volume.vector_lp < - tf[1].volume.vector_hpp
                    &&
                    tf[1].volume.vector + tf[1].volume.vector_lp < tf[1].volume.vector_lpp
                    &&
                    tf[1].GetCandleLow(0) < tf[1].GetCandleLow(2) - new decimal(20.0)
                    &&
                    tf[1].IsRedCandle(0)
                    &&
                    tf[1].IsRedCandle(1)


                    ,
                    tf[1].volume.GetExpTv(300, 21, 12) > tf[1].volume.GetExpTv(300, 21, 24)           //..8  
                    &&
                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(300, 21, 12)
                    &&
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(7.0)
                    &&
                    tf[1].volume.GetAvrVv(180, 0) < tf[1].volume.GetAvrTactsVvMin(180, 12, 1)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < 0
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPriceTactsMin(24, 1)
                    &&
                    tf[1].GetCandleLow(0) < tf[1].GetCandleLow(1) - new decimal(20.0)


                    ,
                    tf[1].volume.total > tf[1].volume.total_p                                         //..9  
                    &&
                    tf[1].GetCandleHLRange(0) > new decimal(200)
                    &&
                    tf[1].GetUpperShadow(0) > tf[1].GetCandleHLRange(0) * new decimal(0.65)
                    &&
                    tf[1].IsRedCandle(0)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 1) > tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > tf[1].volume.GetExpVv(300, 21, 1)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 1) < tf[1].volume.GetExpVv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < tf[1].volume.GetExpVv(180, 8, 1)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < -new decimal(0.3)


                    ,
                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(300, 21, 2)            //..10
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(300, 21, 0)
                    &&
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(7.0)

                    &&
                    tf[1].volume.GetAvrVv(180, 0) < tf[1].volume.GetAvrTactsVvMin(180, 12, 1)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < tf[1].volume.GetExpVv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < tf[1].volume.GetExpVv(300, 21, 0)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < 0
                    &&
                    tf[1].volume.GetTactRealPrice(0) < tf[1].volume.GetTactRealPriceTactsMin(24, 1)
                    &&
                    tf[1].GetCandleLow(0) < tf[1].GetCandleLow(1) - new decimal(20.0)


                    ,
                    tf[1].GetCandleHLRange(1) > new decimal(200)                                     //..11
                    &&
                    tf[1].IsGreenCandle(1)
                    &&
                    tf[1].GetCandleBodyRange(0) > tf[1].GetCandleHLRange(1) * new decimal(0.6)
                    &&
                    tf[1].IsRedCandle(0) 
                    


//                    ,
  //                  Current_Day_PNL > new decimal(800)                                             //..11

        };


        //
        // COVER.........................................................
        //

        private bool CoverCommonRule =>

                    //-// COVER общее условие......................................

                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPrice(2)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPrice(1)
            //              &&
            //            tf[1].volume.GetAvrVv(180, 0) > 0

            // true

            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>

        {

//-// COVER дополнительные условия..............................


                    Position_PNL_MAX >= new decimal(500)                                              //..1


                    ,
                    Position_PNL_MAX >= new decimal(350)                                              //..2
                    &&
                    Position_PNL_MAX < new decimal(500)
                    &&
                    Position_PNL < Position_PNL_MAX * new decimal(1.5) - new decimal(250)


                    ,
                    Position_PNL_MAX >= new decimal(200)                                              //..3
                    &&
                    Position_PNL_MAX < new decimal(300)
                    &&
                    Position_PNL < Position_PNL_MAX - new decimal(250)


                    ,
                    Position_PNL_MAX < new decimal(200)                                               //..4
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(300)
                    &&
                    tf[1].volume.GetExpVv(900, 50, 0) < tf[1].volume.GetExpVv(900, 50, 2)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) < tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) < tf[1].volume.GetExpVv(180, 8, 2)


                    ,
                    tf[1].volume.GetAvrVv(180) > new decimal(5.5)                                     //..5
                    &&
                    tf[1].volume.GetAvrVv(180, 0) > tf[1].volume.GetAvrTactsVvMax(180, 12, 1)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 1) > tf[1].volume.GetExpTv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(180, 8, 1)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPriceTactsMax(36, 1) +
                    new decimal(0.0)


                    ,
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(7.0)                                  //..6
                    &&
                    tf[1].volume.total > tf[1].volume.total_p
                    &&
                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(1.0)
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(1.0)
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPriceTactsMax(24, 1)
                    &&
                    tf[1].IsGreenCandle(0)


                    ,
                    tf[1].volume.total + tf[1].volume.total_p > tf[1].volume.total_pp                 //..7  
                    &&
                    tf[1].volume.vector + tf[1].volume.vector_hp > tf[1].volume.vector_hpp
                    &&
                    tf[1].volume.vector + tf[1].volume.vector_hp > -tf[1].volume.vector_lpp
                    &&
                    tf[1].GetCandleHigh(0) > tf[1].GetCandleHigh(2) + new decimal(20.0)
                    &&
                    tf[1].IsGreenCandle(0)
                    &&
                    tf[1].IsGreenCandle(1)


                    ,
                    tf[1].volume.GetExpTv(300, 21, 12) > tf[1].volume.GetExpTv(300, 21, 24)           //..8  
                    &&
                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(300, 21, 12)
                    &&
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(7.0)
                    &&
                    tf[1].volume.GetAvrVv(180, 0) > tf[1].volume.GetAvrTactsVvMax(180, 12, 1)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > 0
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPriceTactsMax(24, 1)
                    &&
                    tf[1].GetCandleHigh(0) > tf[1].GetCandleHigh(1) + new decimal(20.0)


                    ,
                    tf[1].volume.total > tf[1].volume.total_p                                         //..9  
                    &&
                    tf[1].GetCandleHLRange(0) > new decimal(200)
                    &&
                    tf[1].GetLowerShadow(0) > tf[1].GetCandleHLRange(0) * new decimal(0.65)
                    &&
                    tf[1].IsGreenCandle(0)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 1) > tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > tf[1].volume.GetExpVv(300, 21, 1)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 1) > tf[1].volume.GetExpVv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) > tf[1].volume.GetExpVv(180, 8, 1)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) > new decimal(0.3)


                    ,
                    tf[1].volume.GetExpTv(300, 21, 0) > tf[1].volume.GetExpTv(300, 21, 2)            //..10
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpTv(180, 8, 0) > tf[1].volume.GetExpTv(300, 21, 0)
                    &&
                    tf[1].volume.GetAvrTv(180, 0) > new decimal(7.0)

                    &&
                    tf[1].volume.GetAvrVv(180, 0) > tf[1].volume.GetAvrTactsVvMax(180, 12, 1)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > tf[1].volume.GetExpVv(300, 21, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) > tf[1].volume.GetExpVv(180, 8, 2)
                    &&
                    tf[1].volume.GetExpVv(180, 8, 0) > tf[1].volume.GetExpVv(300, 21, 0)
                    &&
                    tf[1].volume.GetExpVv(300, 21, 0) > 0
                    &&
                    tf[1].volume.GetTactRealPrice(0) > tf[1].volume.GetTactRealPriceTactsMax(24, 1)
                    &&
                    tf[1].GetCandleHigh(0) > tf[1].GetCandleHigh(1) + new decimal(20.0)


                    ,
                    tf[1].GetCandleHLRange(1) > new decimal(200)                                     //..11
                    &&
                    tf[1].IsRedCandle(1)
                    &&
                    tf[1].GetCandleBodyRange(0) > tf[1].GetCandleHLRange(1) * new decimal(0.6)
                    &&
                    tf[1].IsGreenCandle(0) 


//                    ,
  //                  Current_Day_PNL > new decimal(800)                                             //..11

            //-//
        };
    }
}
