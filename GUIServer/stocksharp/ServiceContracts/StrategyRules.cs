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
                

                /** LONG **/

                {
                    ruleId = 0;
                    tradeState.LongOpen = !waitCommonReverse && !waitLongReverse
                        //*//
                        &&
                        tf[0].adx[0].dip > tf[0].adx[0].dim
                        //*//
                        && true;
                }
                

                /** SHORT **/

                if (!tradeState.LongOpen)
                {
                    ruleId = 0;
                    tradeState.ShortOpen = !waitCommonReverse && !waitShortReverse
                        //*//
                        &&
                        tf[0].adx[0].dip < tf[0].adx[0].dim
                        //*//
                        && true;
                }


                /** Разрешение на LONG или SHORT **/

                bool allowEntry = true
                    //*//
                    && !tf[0].IsExitCandle()
                    //*//
                    && true;


                /** Разрешение на LONG **/

                tradeState.LongOpen &= allowEntry
                    //*//
                    //*//
                    && true;


                /** Разрешение на SHORT **/

                tradeState.ShortOpen &= allowEntry
                    //*//
                    //*//
                    && true;
                

                tradeState.RuleId = ruleId;
                if (tradeState.LongOpen) waitLongReverse = true;
                if (tradeState.ShortOpen) waitShortReverse = true;
            }
            {
                /** SELL **/
                ruleId = 0;
                tradeState.LongClose = false
                    //*//
                    //*//
                    && true;
                if (_needAction == NeedAction.LongClose)
                    tradeState.RuleId = ruleId;

                /** COVER **/
                ruleId = 0;
                tradeState.ShortClose = false
                    //*//
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
