using System;
using System.Collections.Generic;
using Ecng.Common;
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
        public TradeState updateTradeState(List<TimeFrame> tf, NeedAction _needAction, PartnerDataObject partnerData)
        {
            TradeState tradeState = new TradeState();
            
            if (_needAction == NeedAction.LongOrShortOpen)
            {
                /// LONG
                {
                    ruleId = 0;
                    tradeState.LongOpen = true
                        //*//
                        &&
                        tf[0].GetCandle().Time.TimeOfDay.TotalMinutes >= 10 * 60 + 15
                        &&
                        tf[0].GetCandle().Time.Hour < 19

                        &&

                        tf[0].adx[0].dip > tf[0].adx[0].dim        //3m, per=3
                        &&
                        tf[0].adx[0].dip >= tf[0].adx[0].dip_p
                        &&
                        tf[0].adx[0].dim <= tf[0].adx[0].dim + new decimal(5)

                        &&

                        tf[0].roc[0].val > new decimal(0.12)     //per=1 

                        &&
                        tf[0].volume.vector > tf[0].volume.vector_p.Abs()

                            &&

                            tf[1].adx[0].dip > tf[1].adx[0].dim
                            //*//
                            && true;
                }

                /// SHORT
                if (!tradeState.LongOpen)
                {
                    // Refresh
                    ruleId = 0;
                    // Динамические условия
                    if (DYNAMIC)
                    { }
                    /// CONDITION
                    tradeState.ShortOpen = true
                        //*//
                        &&
                        tf[0].GetCandle().Time.TimeOfDay.TotalMinutes >= 10 * 60 + 15
                        &&
                        tf[0].GetCandle().Time.Hour < 19

                        &&

                        tf[0].adx[0].dim > tf[0].adx[0].dip
                        &&
                        tf[0].adx[0].dim >= tf[0].adx[0].dim_p               //adx[0]-adx[1]
                        &&
                        tf[0].adx[0].dip <= tf[0].adx[0].dip_p + new decimal(10)

                        &&
                        

                        tf[0].roc[0].val < -new decimal(0.12)     //per=1 

                        &&
                        -tf[0].volume.vector > tf[0].volume.vector_p.Abs()

                            &&

                            tf[1].adx[0].dim > tf[1].adx[0].dip
                            //*//
                            && true;
                }

                /// Общее разрешение на вход
                bool allowEntry = true
                    //*//
                    // Запрет входа в позицию на свече выхода........(убрать комменты в стр ниже)..............
                    && !tf[0].IsExitCandle()
                    //*//
                    && true;
                /// LONG разрешение на вход
                tradeState.LongOpen &= allowEntry
                    //*//
                    // <- Пишем внутри
                    //*//
                    && true;
                /// SHORT разрешение на вход
                tradeState.ShortOpen &= allowEntry
                    //*//
                    // <- Пишем внутри
                    //*//
                    && true;
                
                tradeState.RuleId = ruleId;
            }
            {
                /// LONG CLOSE
                ruleId = 0;
                tradeState.LongClose = false
                        //*//
                        || fix() &&
                        tf[0].adx[0].dim > tf[0].adx[0].dim_p + new decimal(10)  //..1

                        || fix() &&
                        tf[0].adx[0].dim > tf[0].adx[0].dip              //.......2
                                                                         //*//
                    && true;
                if (_needAction == NeedAction.LongClose)
                    tradeState.RuleId = ruleId;

                /// SHORT CLOSE
                ruleId = 0;
                tradeState.ShortClose = false
                        //*//

                        || fix() &&
                        tf[0].adx[0].dip > tf[0].adx[0].dip_p + new decimal(10)  //..1

                        || fix() &&
                        tf[0].adx[0].dip > tf[0].adx[0].dim              //.......2
                                                                         //*//
                    && true;
                if (_needAction == NeedAction.ShortClose)
                    tradeState.RuleId = ruleId;
            }


            if (_currentUser == "ro#9019")
            {
                GUIServer.MainWindow.Instance.UpdateInfoLabel(string.Format("_{0}_ _{1}_ _{2}_ _{3}_ _{4}_ _{5}_ _{6}_",
                    Math.Round(tf[0].Volume[0].act, 1),
                    Math.Round(tf[0].Volume[0].act_p, 1),
                    Math.Round(tf[0].Volume[0].tv, 1),
                    Math.Round(tf[0].Volume[0].bo, 1),
                   Math.Round(tf[0].Volume[0].so, 1),
                    Math.Round(tf[0].Volume[0].bv, 1),
                   Math.Round(tf[0].Volume[0].sv, 1)
                   ));
            }

            return tradeState;
        }
    }
}
