using System;

using StockSharp.BusinessEntities;

using transportDataParrern;

namespace stocksharp.ServiceContracts
{
    public partial class WorkService
    {
        private int ruleId;
        private bool fix(bool _newBlock = false)
        {
            if (_newBlock)
                //.
                ruleId *= 10;
            else
            {
                // Работаем в 10 системе счисления
                if (ruleId % 10 == 9)
                {
                    //addLogMessage("Незадача то какая.. Больше 9 условий в блоке!");
                    //mw.stopTrading();
                }
                // ..
                ruleId++;
            }

            return true;
        }

        private const bool DYNAMIC = false; // true - включить; false - выключить (ДИНАМИЧЕСКОЕ УСЛОВИЕ)
        public TradeState updateTradeState(NeedAction _needAction)
        {
            TradeState tradeState = new TradeState();

            // ENTRY
            if (_needAction == NeedAction.LongOrShortOpen)
            {
                /**
  * LONG OPEN
                 **/

                {
                    // Refresh
                    ruleId = 0;
                    // Динамические условия
                    if (DYNAMIC)
                    { }
                    /// CONDITION
                    tradeState.LongOpen = true
                            /*~*/
                            &&
                            tf[0].adx[0].dip > tf[0].adx[0].dim                      //per=2
                            &&
                            tf[0].adx[0].dip > tf[0].adx[0].dip_p 
                            &&
                            (
                                tf[0].roc[0].val > new decimal(0.08)                   //...per=1
                                ||
                                tf[0].roc[1].val > new decimal(0.1)                   //...per=2                       
                            )
                            &&
                            (
                                    tf[0].adx[0].val > new decimal(42)                    //...per=2
                                    &&
                                    tf[0].adx[0].val > tf[0].adx[0].val_p
                                    ||
                                    tf[0].adx[0].dip > new decimal(42)                    //...per=2 
                            )
                        /*~*/
                        && true;
                }

                /**
                 * SHORT OPEN
                 **/

                if (!tradeState.LongOpen)
                {
                    // Refresh
                    ruleId = 0;
                    // Динамические условия
                    if (DYNAMIC)
                    { }
                    /// CONDITION
                    tradeState.ShortOpen = true
                            /*~*/
                            &&
                            tf[0].adx[0].dim > tf[0].adx[0].dip                      //per=2
                            &&
                            tf[0].adx[0].dim > tf[0].adx[0].dim_p  
                            &&
                            (
                                tf[0].roc[0].val < - new decimal(0.08)                   //...per=1
                                ||
                                tf[0].roc[1].val < - new decimal(0.1)                   //...per=2                            
                            )
                            &&
                            (
                                    tf[0].adx[0].val > new decimal(42)                    //...per=5
                                    &&
                                    tf[0].adx[0].val > tf[0].adx[0].val_p
                                    ||
                                    tf[0].adx[0].dim > new decimal(42)                    //...per=2                             
                            )
                        /*~*/
                        && true;
                }

                /**
                 * Allow
                 **/

                // Разрешить ВХОД
                bool allowEntry = true
                    /*~*/
                    // Запрет входа в позицию на свече выхода........(убрать комменты в стр ниже)..............
                //    && !tf[0].IsExitCandle()
                    /*
                    ||

                    tf[0].Get_Candle_HLRange() > 500
                    
                    /*~*/
                    && true;
                // Разрешить LONG
                tradeState.LongOpen &= allowEntry
                    /*~*/

                    /*~*/
                    && true;
                // Разрешить SHORT
                tradeState.ShortOpen &= allowEntry
                    /*~*/

                    /*~*/
                    && true;

                // ..
                tradeState.RuleId = ruleId;
            }
            // EXIT
            {
                /**
                 * LONG CLOSE
                 **/

                ruleId = 0;
                /// CONDITION
                tradeState.LongClose = true
                        /*~*/
                        &&
                        tf[0].adx[0].dip < tf[0].adx[0].dim 
                    /*~*/
                    && true;
                // Save RuleId
                if (_needAction == NeedAction.LongClose)
                    tradeState.RuleId = ruleId;

                /**
                 * SHORT CLOSE
                 **/

                ruleId = 0;
                /// CONDITION
                tradeState.ShortClose = true
                        /*~*/
                        &&
                        tf[0].adx[0].dip > tf[0].adx[0].dim 
                    /*~*/
                    && true;
                // Save RuleId
                if (_needAction == NeedAction.ShortClose)
                    tradeState.RuleId = ruleId;
            }

            return tradeState;
        }
    }
}
