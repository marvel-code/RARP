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

                    true

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
           
//-// LONG добавочные условия........................................................................1

                    new List<bool>
                    {
                        tf[0].adx[0].dip > tf[0].adx[0].dim,
                    },
                };


        //
        // SHORT.................................................................................................
        //

        private bool ShortCommonRule =>

                    //-// SHORT общее условие................................................................................


            true

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {

//-// SHORT добавочные условия.......................................................................1

                    new List<bool>
                    {

                        tf[0].adx[0].dip < tf[0].adx[0].dim,

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

            // true

            //-//
            && true;

        private bool isShortAllowed =>

                  //-// Разрешение на SHORT...........................................

                  !tf[1].IsExitCandle(0)

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

                        tf[0].adx[0].dip < tf[0].adx[0].dim,

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


                        tf[0].adx[0].dip > tf[0].adx[0].dim,
            //-//
        };
    }
}
