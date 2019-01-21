using System;
using System.Collections.Generic;
using Ecng.Common;
using StockSharp.BusinessEntities;
using StockSharp.Messages;
using transportDataParrern;
using Ecng.Common;

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

        private bool waitCommonReverse = false;
        private bool waitLongReverse = false;
        private bool waitShortReverse = false;
        public TradeState updateTradeState(List<TimeFrame> tf, NeedAction _needAction, PartnerDataObject PD)
        {
            TradeState tradeState = new TradeState();

            decimal Position_PNL = PD.Position_PNL;
            decimal Position_PNL_MAX = PD.Position_PNL_MAX;

            if (_needAction == NeedAction.LongOrShortOpen)
            {


                /** Условие на общий реверс **/

                if (waitCommonReverse) waitCommonReverse = false;


                /** Условие на LONG реверс **/

                if (waitLongReverse) waitLongReverse = false;


                /** Условие на SHORT реверс **/

                if (waitShortReverse) waitShortReverse = false;
                

/** LONG **///.....................................

                {

                ruleId = 0;
                tradeState.LongOpen = !waitCommonReverse && !waitLongReverse
                    //*//
                &&

                tf[0].IsGreenCandle(0)           //1min
                &&
                tf[0].GetPriceChannelHigh(3, 0) > tf[0].GetPriceChannelHigh(3, 1) +
                new decimal(0)
                &&
                tf[1].IsGreenCandle(0)             //  5min
                &&
                tf[1].GetPriceChannelHigh(1, 0) > tf[1].GetPriceChannelHigh(1, 1) +
                new decimal(0)
                &&
                tf[1].roc[1].val > new decimal(0.0)          //..per=2 
                &&
                tf[1].roc[1].val > tf[1].roc[1].val_p
                &&

//..Add.1..................                   

                    (fix(true) && false

                        || fix() &&
                        tf[1].roc[0].val > new decimal(0.12)          //..per=1          //..1
                        &&
                        tf[1].roc[0].val > tf[1].roc[0].val_p

                        || fix() &&
                        tf[1].roc[1].val > new decimal(0.15)          //..per=2          //..2
                        &&
                        tf[1].roc[1].val > tf[1].roc[1].val_p

                        || fix() &&
                        tf[1].roc[2].val > new decimal(0.15)          //..per=3          //..3
                        &&
                        tf[1].roc[2].val > tf[1].roc[2].val_p

                        || fix() &&
                        tf[1].roc[0].val > new decimal(0.07)          //..per=1          //..4
                        &&
                        tf[1].volume.total > tf[1].volume.total_p * new decimal(2.5)

                        || fix() &&
                        tf[1].roc[0].val > new decimal(0.07)          //..per=1          //..5
                        &&
                        tf[1].GetCandleBodyRange(0) > tf[1].GetCandleHLRange(1) * new decimal(1.0)
                        &&
                        tf[1].volume.total > tf[1].volume.total_p * new decimal(1.0)

                        || fix() &&
                        tf[1].roc[0].val > new decimal(0.03)          //..per=1          //..6
                        &&
                        tf[1].roc[0].val > tf[1].roc[0].val_p
                        &&
                        tf[1].roc[1].val > new decimal(0.03)          //..per=2          
                        &&
                        tf[1].roc[1].val > tf[1].roc[1].val_p
                        &&
                        tf[1].roc[2].val > new decimal(0.03)          //..per=3          
                        &&
                        tf[1].roc[2].val > tf[1].roc[2].val_p
                        &&
                        tf[1].volume.total > tf[1].volume.total_p * new decimal(1.0)

                    )

                    &&

//..Add.2...................

                    (fix(true) && false

                        || fix() &&
                        tf[1].volume.vector > tf[1].volume.vector_p                     //..1
                        &&
                        tf[1].volume.vector_p > 0

                        || fix() &&
                        tf[1].volume.vector > -tf[1].volume.vector_p                   //..2
                        &&
                        tf[1].volume.vector_p < 0

                        || fix() &&
                        tf[1].volume.vector > new decimal(300)                          //..3

                    )
                    //*//
                    && true;
                }
                

  /** SHORT **///...............................................

                if (!tradeState.LongOpen)
                {

                ruleId = 0;
                tradeState.ShortOpen = !waitCommonReverse && !waitShortReverse
                    //*//
                &&
                tf[0].IsRedCandle(0)
                &&
                tf[0].GetPriceChannelLow(3, 0) < tf[0].GetPriceChannelLow(3, 1) -
                new decimal(0)
                &&
                tf[1].IsRedCandle(0)
                &&
                tf[1].GetPriceChannelLow(1, 0) < tf[1].GetPriceChannelLow(1, 1) -
                new decimal(0)
                &&
                tf[1].roc[1].val < new decimal(0.0)          //..per=2 
                &&
                tf[1].roc[1].val < tf[1].roc[1].val_p
                &&

//..Add.1...................

                    (fix(true) && false   
                    
                        || fix() &&
                        tf[1].roc[0].val < - new decimal(0.12)          //..per=1          //..1
                        &&
                        tf[1].roc[0].val < tf[1].roc[0].val_p

                        || fix() &&
                        tf[1].roc[1].val < - new decimal(0.15)          //..per=2          //..2
                        &&
                        tf[1].roc[1].val < tf[1].roc[1].val_p

                        || fix() &&
                        tf[1].roc[2].val < - new decimal(0.15)          //..per=3          //..3
                        &&
                        tf[1].roc[2].val < tf[1].roc[2].val_p

                        || fix() &&
                        tf[1].roc[0].val < - new decimal(0.07)          //..per=1         //..4
                        &&
                        tf[1].volume.total > tf[1].volume.total_p * new decimal(2.5)

                        || fix() &&
                        tf[1].roc[0].val < - new decimal(0.07)          //..per=1         //..5
                        &&
                        tf[1].GetCandleBodyRange(0) > tf[1].GetCandleHLRange(1) * new decimal(1.0)
                        &&
                        tf[1].volume.total > tf[1].volume.total_p * new decimal(1.0)

                        || fix() &&
                        tf[1].roc[0].val < - new decimal(0.03)          //..per=1         //..7
                        &&
                        tf[1].roc[0].val < tf[1].roc[0].val_p
                        &&
                        tf[1].roc[1].val < - new decimal(0.03)          //..per=2          
                        &&
                        tf[1].roc[1].val < tf[1].roc[1].val_p
                        &&
                        tf[1].roc[2].val < - new decimal(0.03)          //..per=3          
                        &&
                        tf[1].roc[2].val < tf[1].roc[2].val_p
                        &&
                        tf[1].volume.total > tf[1].volume.total_p * new decimal(1.0)  

                    )

                    &&

//..Add.2...................                    

                    (fix(true) && false

                        || fix() &&
                        tf[1].volume.vector < tf[1].volume.vector_p                      //..1
                        &&
                        tf[1].volume.vector_p < 0

                        || fix() &&
                        tf[1].volume.vector < -tf[1].volume.vector_p                     //..2
                        &&
                        tf[1].volume.vector_p > 0

                        || fix() &&
                        tf[1].volume.vector < -new decimal(300)

                    )
                    //*//
                    && true;
                }


                /** Разрешение на LONG или SHORT **/

                bool allowEntry = true
                    //*//
                    && !tf[1].IsExitCandle()
                    //*//
                    && true;


                /** Разрешение на LONG **/

                tradeState.LongOpen &= allowEntry
                    //*//
                    && tf[0].IsExitCandle() == false || PD.lastEnterDirection != Sides.Buy.ToString()
                    //*//
                    && true;


                /** Разрешение на SHORT **/

                tradeState.ShortOpen &= allowEntry
                    //*//
                    && tf[0].IsExitCandle() == false || PD.lastEnterDirection != Sides.Sell.ToString()
                    //*//
                    && true;
                

                tradeState.RuleId = ruleId;
                if (tradeState.LongOpen) waitLongReverse = true;
                if (tradeState.ShortOpen) waitShortReverse = true;
            }
            {

/** SELL **///...........................................

                ruleId = 0;
                tradeState.LongClose = false
                    //*//
                    || fix() &&
                    tf[1].IsRedCandle(0)                                               //..1
                    &&
                    tf[1].GetCandleDuration(0) > tf[1].period * new decimal(0.7)

                    || fix() &&
                    tf[1].IsRedCandle(0)                                               //..2
                    &&
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) * new decimal(0.7)

                    || fix() &&
                    tf[1].IsRedCandle(0)                                               //..3
                    &&
                    tf[1].GetPriceChannelLow(1, 0) < tf[1].GetPriceChannelLow(1, 1)

                    || fix() &&
                    Position_PNL <= Position_PNL_MAX - new decimal(150)                //..4 

                    || fix() &&
                    Position_PNL_MAX >= new decimal(100)                               //..5         
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(10)

                    || fix() &&
                    tf[1].IsRedCandle(0)                                               //..6
                    &&
                    tf[1].currentPrice < tf[1].GetCandleHigh(1) - tf[1].GetCandleHLRange(1) *
                    new decimal(0.7)

                    //*//
                    && true;
                    if (_needAction == NeedAction.LongClose)
                    tradeState.RuleId = ruleId;

/** COVER **///...............................................

                ruleId = 0;
                tradeState.ShortClose = false
                    //*//

                    || fix() &&
                    tf[1].IsGreenCandle(0)                                             //..1
                    &&
                    tf[1].GetCandleDuration(0) > tf[1].period * new decimal(0.7)

                    || fix() &&
                    tf[1].IsGreenCandle(0)                                             //..2
                    &&
                    tf[1].GetCandleHLRange(0) > tf[1].GetCandleHLRange(1) * new decimal(0.7)

                    || fix() &&
                    tf[1].IsGreenCandle(0)                                             //..3
                    &&
                    tf[1].GetPriceChannelHigh(1, 0) > tf[1].GetPriceChannelHigh(1, 1)

                    || fix() &&
                    Position_PNL <= Position_PNL_MAX - new decimal(150)                //..4  

                    || fix() &&
                    Position_PNL_MAX >= new decimal(100)                               //..5         
                    &&
                    Position_PNL <= Position_PNL_MAX - new decimal(10)

                    || fix() &&
                    tf[1].IsGreenCandle(0)                                             //..6
                    &&
                    tf[1].currentPrice > tf[1].GetCandleLow(1) + tf[1].GetCandleHLRange(1) *
                    new decimal(0.7)

                    //*//
                    && true;

                    if (_needAction == NeedAction.ShortClose)
                    tradeState.RuleId = ruleId;
            }

            /*
            if (_currentUser == "ro#9019")
            {

                GUIServer.MainWindow.Instance.UpdateInfoLabel(string.Format("_{0}_ _{1}_ _{2}_ _{3}_ _{4}_ _{5}_ _{6}_",
                    Math.Round(tf[0].Volume[0].GetAvrBv(120), 1),
                    Math.Round(tf[0].Volume[0].GetAvrSv(120), 1),
                    Math.Round(tf[0].Volume[0].GetAvrTv(120), 1),
                    Math.Round(tf[0].Volume[0].GetAvrVv(120), 1),
                   Math.Round(tf[0].Volume[0].GetTv(120), 1),
                    Math.Round(tf[0].Volume[0].GetVv(120), 1),
                   Math.Round(tf[0].Volume[0].sv, 1)
                   ));
            }
            */

            return tradeState;
        }
    }
}
