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
           300,
           120
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
            new ROC_Configuration(0, 1, CalculationType.Median),
            new ROC_Configuration(0, 2, CalculationType.Median),
        };
        public List<Volume_Configuration> volume_cfgList = new List<Volume_Configuration>
        {
            new Volume_Configuration(0, 2, 1, 1),
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
        // LONG..........................................................................................................
        //
        private bool LongCommonRule =>


                    /*
                    PositionPrices.ExpReal[0].Max.Value - в позиции   

                    PositionPrices.Real.Max.Value       - в позиции

                    tf[0].volume.GetTactExpPriceTactsMax(5, 24, 0) - Максимум экспоненты с коэфициентом 5 цены за 24 такта с 0 такта

                    tf[0].volume.GetAvrTactsTvMax(180, 12, 1) - Максимум средней (за цикл) Скорости Объёмов (180-период счёта; 12-период,
                    в котором есть локал Мах/Мин с 1 такта) 

                    tf[0].volume.GetAvrTactsVvMin(180, 12, 1) - Минимум средней (за цикл) Скорости Векторов (180-период счёта; 12-период,
                    в котором есть локал Мах/Мин с 1 такта) 

                    tf[0].volume.GetExpTv(300, 24, tf[0].volume.GetExpTactsVvMaxTactsDuration(300, 24, 24))  - 
                    значение экспон Tv с момента локального минимума Vv (300-период, 24-коэфф експ, 24-период, в котором есть локал Мах/Мин)
                    */


                    //-// LONG общее условие.......................................................................................


                    tf[0].volume.GetTactRealPrice(0) > tf[0].volume.GetTactExpPrice(50, 0)

                    &&
                    tf[0].volume.GetAvrTv(300, 0) > tf[0].volume.GetAvrTactsTvMax(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrTv(180, 0) > tf[0].volume.GetAvrTactsTvMax(180, 60, 1)

                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) > tf[0].volume.GetExpVv(1800, 50, 12)
                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) > tf[0].volume.GetExpVv(900, 50, 12)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > new decimal(0.0)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrTactsVvMax(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrTactsVvMax(180, 60, 1)

                    &&
                    tf[0].volume.vector > new decimal(200.0)

                    //  true

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
           
//-// LONG добавочные условия........................................................................1

                    new List<bool>
                    {

                    tf[0].volume.GetExpTv(1800, 50, 0) > tf[0].volume.GetExpTv(1800, 50, 12)   //..1


                    ,
                    tf[0].volume.GetExpTv(900, 50, 0) > tf[0].volume.GetExpTv(900, 50, 12)     //..2

                    },

//-// LONG добавочные условия........................................................................2

                    new List<bool>
                    {


                    tf[0].volume.GetAvrTv(180, 0) > new decimal(15.0)                        //..1


                    ,
                    tf[0].volume.vector > new decimal(900.0)                                 //..2
                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) > tf[0].volume.GetExpVv(1800, 50, 24)
                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) > tf[0].volume.GetExpVv(900, 50, 24)
                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) > tf[0].volume.GetExpVv(1800, 50, 0)

                    },

        //-//
                };


        //
        // SHORT.................................................................................................
        //

        private bool ShortCommonRule =>

                    //-// SHORT общее условие................................................................................


                    tf[0].volume.GetTactRealPrice(0) < tf[0].volume.GetTactExpPrice(50, 0)

                    &&
                    tf[0].volume.GetAvrTv(300, 0) > tf[0].volume.GetAvrTactsTvMax(300, 12, 1)
                    &&
                    tf[0].volume.GetAvrTv(180, 0) > tf[0].volume.GetAvrTactsTvMax(180, 12, 1)

                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) < tf[0].volume.GetExpVv(1800, 50, 12)
                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) < tf[0].volume.GetExpVv(900, 50, 12)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < new decimal(0.0)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrTactsVvMin(300, 12, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrTactsVvMin(180, 12, 1)

                    &&
                    tf[0].volume.vector < -new decimal(200.0)

            //           true

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {

//-// SHORT добавочные условия.......................................................................1

                    new List<bool>
                    {


                    tf[0].volume.GetExpTv(1800, 50, 0) > tf[0].volume.GetExpTv(1800, 50, 12)   //..1


                    ,
                    tf[0].volume.GetExpTv(900, 50, 0) > tf[0].volume.GetExpTv(900, 50, 12)     //..2

                    },

//-// SHORT добавочные условия.......................................................................2

                    new List<bool>
                    {

                    tf[0].volume.GetAvrTv(180, 0) > new decimal(15.0)                          //..1


                    ,
                    tf[0].volume.vector < - new decimal(900.0)                                 //..2
                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetExpVv(1800, 50, 0) < tf[0].volume.GetExpVv(1800, 50, 24)
                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) < tf[0].volume.GetExpVv(900, 50, 24)
                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) < tf[0].volume.GetExpVv(1800, 50, 0)

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
                  &&
                  Current_Day_PNL < new decimal(5000)    //  Current_Day_PNL < new decimal(400,500,600,700)
                  &&
                  Current_Time.TimeOfDay > new TimeSpan(10, 10, 0)
                  &&
                  Current_Time.TimeOfDay < new TimeSpan(18, 44, 0)
            // true

            //-//
            && true;

        private bool isShortAllowed =>

                  //-// Разрешение на SHORT...........................................

                  !tf[1].IsExitCandle(0)
                  &&
                  Current_Day_PNL < new decimal(5000)  //  Current_Day_PNL < new decimal(500)
                  &&
                  Current_Time.TimeOfDay > new TimeSpan(10, 10, 0)
                  &&
                  Current_Time.TimeOfDay < new TimeSpan(18, 44, 0)

            // true

            //-//
            && true;


        //
        // SELL..........................................................
        //

        private bool SellCommonRule =>

                     //-// SELL общее условие.........................................

                     true
            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>

        {

//-// SELL дополнительные условия..............................


                    tf[0].IsRedCandle(0)                                                                 //..1
                    &&
                    tf[0].IsGreenCandle(1)

                    &&
                    tf[0].GetCandleLow(0) < tf[0].GetCandleLow(1)

                    &&
                    tf[0].volume.total > tf[0].volume.total_p * new decimal(1.0)

                    &&
                    tf[0].volume.vector_l < -tf[0].volume.vector_hp * new decimal(1.0)

                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) < tf[0].volume.GetExpVv(900, 50, 12)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrTactsVvMin(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrTactsVvMin(180, 60, 1)


                    ,
                    tf[0].IsRedCandle(0)                                                                 //..2
                    &&
                    tf[0].IsRedCandle(1)
                    &&
                    tf[0].IsGreenCandle(2)

                    &&
                    tf[0].GetCandleLow(0) < tf[0].GetCandleLow(1)

                    &&
                    tf[0].volume.total + tf[0].volume.total_p > tf[0].volume.total_pp * new decimal(1.0)

                    &&
                    tf[0].volume.vector_l + tf[0].volume.vector_lp < -tf[0].volume.vector_hpp *
                    new decimal(1.0)

                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) < tf[0].volume.GetExpVv(900, 50, 12)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrTactsVvMin(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrTactsVvMin(180, 60, 1)


                    ,
                    tf[0].IsRedCandle(0)                                                                 //..3

                    &&
                    tf[0].GetCandleBodyRange(0) > tf[0].GetCandleHLRange(1)

                    &&
                    tf[0].volume.total > tf[0].volume.total_p

                    &&
                    tf[0].volume.GetAvrVv(300, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrTactsVvMin(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrTactsVvMin(180, 60, 1)


                    ,
                    tf[0].IsRedCandle(0)                                                                 //..4
                    &&
                    tf[0].IsGreenCandle(1)

                    &&
                    tf[0].volume.total  > tf[0].volume.total_pp
                    &&
                    tf[0].volume.total_p  > tf[0].volume.total_pp

                    &&
                    tf[0].volume.GetAvrVv(300, 0) < tf[0].volume.GetAvrTactsVvMin(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) < tf[0].volume.GetAvrTactsVvMin(180, 60, 1)


    //                ,
   //                 Position_PNL <= Position_PNL_MAX  - new decimal(300)                                           //..9
        };


        //
        // COVER.........................................................
        //

        private bool CoverCommonRule =>

                      //-// COVER общее условие......................................

                      true

            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>

        {

//-// COVER дополнительные условия..............................


                    tf[0].IsGreenCandle(0)                                                        //..1
                    &&
                    tf[0].IsRedCandle(1)

                    &&
                    tf[0].GetCandleHigh(0) > tf[0].GetCandleHigh(1)

                    &&
                    tf[0].volume.total > tf[0].volume.total_p * new decimal(1.0)

                    &&
                    tf[0].volume.vector_h > - tf[0].volume.vector_lp * new decimal(1.0)

                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) > tf[0].volume.GetExpVv(900, 50, 12)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrTactsVvMax(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrTactsVvMax(180, 60, 1)


                    ,
                    tf[0].IsGreenCandle(0)                                                        //..2
                    &&
                    tf[0].IsGreenCandle(1)
                    &&
                    tf[0].IsRedCandle(2)

                    &&
                    tf[0].GetCandleHigh(0) > tf[0].GetCandleHigh(1)

                    &&
                    tf[0].volume.total + tf[0].volume.total_p > tf[0].volume.total_pp * new decimal(1.0)

                    &&
                    tf[0].volume.vector_h + tf[0].volume.vector_hp > - tf[0].volume.vector_lpp *
                    new decimal(1.0)

                    &&
                    tf[0].volume.GetExpVv(900, 50, 0) > tf[0].volume.GetExpVv(900, 50, 12)

                    &&
                    tf[0].volume.GetAvrVv(300, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrTactsVvMax(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrTactsVvMax(180, 60, 1)


                    ,
                    tf[0].IsGreenCandle(0)                                                              //..3

                    &&
                    tf[0].GetCandleBodyRange(0) > tf[0].GetCandleHLRange(1)

                    &&
                    tf[0].volume.total > tf[0].volume.total_p

                    &&
                    tf[0].volume.GetAvrVv(300, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > new decimal(0.0)
                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrTactsVvMax(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrTactsVvMax(180, 60, 1)


                    ,
                    tf[0].IsGreenCandle(0)                                                              //..4
                    &&
                    tf[0].IsRedCandle(1)

                    &&
                    tf[0].volume.total  > tf[0].volume.total_pp
                    &&
                    tf[0].volume.total_p  > tf[0].volume.total_pp

                    &&
                    tf[0].volume.GetAvrVv(300, 0) > tf[0].volume.GetAvrTactsVvMax(300, 60, 1)
                    &&
                    tf[0].volume.GetAvrVv(180, 0) > tf[0].volume.GetAvrTactsVvMax(180, 60, 1)


  //                  ,
  //                  Position_PNL <= Position_PNL_MAX  - new decimal(300) 
            
            //-//
        };
    }
}
