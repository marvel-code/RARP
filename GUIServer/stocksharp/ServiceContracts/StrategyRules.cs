using System;
using System.Collections.Generic;
using Ecng.Common;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using transportDataParrern;

namespace stocksharp.ServiceContracts
{
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
        // LONG
        //

        private bool LongCommonRule =>
                    //-// LONG общее условие

                    true

                    //-//
                    && true;

        private List<List<bool>> LongAdditionalRules => new List<List<bool>>
                {
            //-// LONG добавочные условия

                    new List<bool>
                    {
                        tf[0].adx[0].dip > tf[0].adx[0].dim
                    },
                    
        //-//
                };


        //
        // SHORT
        //

        private bool ShortCommonRule =>
                        //-// SHORT общее условие

                        true

            //-//
            && true;

        private List<List<bool>> ShortAdditionalRules => new List<List<bool>>
        {
            //-// SHORT добавочные условия

                    new List<bool>
                    {
                        tf[0].adx[0].dip < tf[0].adx[0].dim
                    },

            //-//
        };


        //
        // Запреты на вход
        //

        private bool isEntryDenyed =>
            //-// Запрет на вход 

            tf[0].IsExitCandle()

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
        // SELL
        //

        private bool SellCommonRule =>
            //-// SELL общее условие

            true

            //-//
            && true;

        private List<bool> SellAdditionalRules => new List<bool>
        {
            //-// SELL дополнительные условия
            
                tf[0].adx[0].dip < tf[0].adx[0].dim

            //-//
        };


        //
        // COVER
        //

        private bool CoverCommonRule =>
            //-// COVER общее условие

            true

            //-//
            && true;

        private List<bool> CoverAdditionalRules => new List<bool>
        {
            //-// COVER дополнительные условия
            
                tf[0].adx[0].dip > tf[0].adx[0].dim

            //-//
        };
    }
}
