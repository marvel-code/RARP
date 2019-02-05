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
            60,
            1800
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
            new MA_Configuration(
                    0, 2, MaType.Simple, CalculationType.Median
                ),
        };
        public List<ROC_Configuration> roc_cfgList = new List<ROC_Configuration>
        {
            new ROC_Configuration(1, 1, CalculationType.Median),
            new ROC_Configuration(1, 2, CalculationType.Median),
        };
        public List<Volume_Configuration> volume_cfgList = new List<Volume_Configuration>
        {
            new Volume_Configuration(0, 2, 1, 60),
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
        // LONG.......................................
        //

        private bool LongCommonRule =>

                    //-// LONG общее условие......................

                    true

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
//-// LONG добавочные условия..................

/// Add.1...........................................................................
 
                    new List<bool>
                    {

                    tf[1].roc[0].val > new decimal(0.0)          //..per=1          //..1
                    &&
                    tf[1].roc[0].val > tf[1].roc[0].val_p
                    &&
                    tf[1].roc[1].val > tf[1].roc[1].val_p
                    &&
                    tf[1].roc[1].val > new decimal(0.0)          //..per=2   
                        
                    ,
                    tf[1].roc[0].val > new decimal(0.0)          //..per=1          //..2
                    &&
                    tf[1].roc[0].val > tf[1].roc[0].val_p
                    &&
                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(1.5)
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(1.5)
                      
                    ,
                    tf[1].roc[1].val > tf[1].roc[1].val_p                           //..3
                    &&
                    tf[1].roc[1].val > new decimal(0.0)          //..per=2   
                    &&
                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(1.5)
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(1.5)

                    },

/// Add.2...........................................................................
 
                    new List<bool>
                    {

                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(1)
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(1)

                    },

/// Add.3...........................................................................
 
                    new List<bool>
                    {

                    tf[1].GetPriceChannelHigh(4, 0) > tf[1].GetPriceChannelHigh(4, 1)

                    ,
                    tf[1].volume.vector > tf[1].volume.vector_hp * new decimal(1.5)
                    &&
                    tf[1].volume.vector > -tf[1].volume.vector_lp * new decimal(1.5)

                    },

           //-//
                };


        //
        // SHORT......................................
        //

        private bool ShortCommonRule =>

                    //-// SHORT общее условие....................

                    tf[0].GetPriceChannelLow(3, 0) < tf[0].GetPriceChannelLow(3, 1)  //..1min

                    &&

                    tf[1].IsRedCandle(0)                                             //..30min
                    &&
                    tf[1].kama[0].val < tf[1].kama[0].val_p

            //true

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {
//-// SHORT добавочные условия................

/// Add.1...........................................................................
 
                    new List<bool>
                    {

                    tf[1].roc[0].val < - new decimal(0.0)          //..per=1          //..1
                    &&
                    tf[1].roc[0].val < tf[1].roc[0].val_p
                    &&
                    tf[1].roc[1].val < tf[1].roc[1].val_p
                    &&
                    tf[1].roc[1].val < - new decimal(0.0)          //..per=2          

                    ,
                    tf[1].roc[0].val < - new decimal(0.0)          //..per=1          //..2
                    &&
                    tf[1].roc[0].val < tf[1].roc[0].val_p
                    &&
                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(1.5)
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(1.5)

                    ,
                    tf[1].roc[1].val < tf[1].roc[1].val_p                             //..3
                    &&
                    tf[1].roc[1].val < - new decimal(0.0)          //..per=2   
                    &&
                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(1.5)
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(1.5)

                    },

/// Add.2...........................................................................
 
                    new List<bool>
                    {

                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(1)
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(1)

                    },

/// Add.2...........................................................................
 
                    new List<bool>
                    {

                    tf[1].GetPriceChannelLow(4, 0) < tf[1].GetPriceChannelLow(4, 1)

                    ,
                    tf[1].volume.vector < tf[1].volume.vector_lp * new decimal(1.5)
                    &&
                    tf[1].volume.vector < -tf[1].volume.vector_hp * new decimal(1.5)

                    },

            //-//
        };


        //
        // Запреты на вход
        //

        private bool isEntryDenyed =>
               //-// Запрет на вход 

               false
            // tf[1].isExitCandle(1)

            //-//
            && true;

        private bool isLongDenyed =>
            //-// Запрет на LONG

            false

            //-//
            && true;

        private bool isShortDenyed =>
            //-// Запрет на SHORT

            false

            //-//
            && true;


        //
        // SELL....................................
        //

        private bool SellCommonRule =>
                    //-// SELL общее условие...................

                    true

            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>
        {
//-// SELL дополнительные условия.........
           
                    tf[1].roc[0].val < tf[1].roc[0].val_p                               //..1
                    &&
                    tf[1].roc[1].val < tf[1].roc[1].val_p
                    &&
                   (
                    tf[1].volume.vector < tf[1].volume.vector_hp * new decimal(1.5)
                    ||
                    tf[1].volume.vector < -tf[1].volume.vector_lp * new decimal(1.5)
                   )

                    ,
                    tf[1].IsRedCandle(0)
                    &&
                    tf[1].volume.vector_p > new decimal(1000)
                    &&
                    tf[1].volume.vector < - tf[1].volume.vector_p * new decimal(0.5)

                    ,
                    Position_PNL <= Position_PNL_MAX - new decimal(400)                 //..2 

                    ,
                    Position_PNL_MAX >= new decimal(1000)                               //..3         
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(10)

            //-//
        };


        //
        // COVER....................................
        //

        private bool CoverCommonRule =>
                   //-// COVER общее условие..................

                   true

            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>
        {
//-// COVER дополнительные условия..........

                    tf[1].roc[0].val > tf[1].roc[0].val_p                                 //..1
                    &&
                    tf[1].roc[1].val > tf[1].roc[1].val_p
                    &&
                    (
                    tf[1].volume.vector > tf[1].volume.vector_lp * new decimal(1.5)
                    ||
                    tf[1].volume.vector > -tf[1].volume.vector_hp * new decimal(1.5)
                    )

                    ,
                    tf[1].IsGreenCandle(0)
                    &&
                    tf[1].volume.vector_p < - new decimal(1000)
                    &&
                    tf[1].volume.vector > - tf[1].volume.vector_p * new decimal(0.5)

                    ,
                    Position_PNL <= Position_PNL_MAX - new decimal(400)                   //..2 

                    ,
                    Position_PNL_MAX >= new decimal(1000)                                 //..3         
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(10)

            //-//
        };
    }
}
