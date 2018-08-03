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

        private bool waitLongReverse = false;
        private bool waitShortReverse = false;
        public TradeState updateTradeState(List<TimeFrame> tf, NeedAction _needAction, PartnerDataObject PD)
        {
            TradeState tradeState = new TradeState();
            
            if (_needAction == NeedAction.LongOrShortOpen)
            {
                if (waitLongReverse) waitLongReverse = tf[0].adx[0].dip > tf[0].adx[0].dim;
                if (waitShortReverse) waitShortReverse = tf[0].adx[0].dip < tf[0].adx[0].dim;

                /// LONG
                {
                    ruleId = 0;
                    tradeState.LongOpen = !waitLongReverse
                        //*//
                        &&
                        tf[0].GetCandle().Time.TimeOfDay.TotalMinutes >= 10 * 60 + 30
                        &&
                        tf[0].GetCandle().Time.Hour < 19
                        &&
                        tf[0].volume.Get_Candle_Duration(0) > tf[0].period * new decimal(0.07)
                        &&

                        tf[1].ma[0].val > tf[1].ma[0].val_p
                        &&
                        tf[1].ma[1].val > tf[1].ma[1].val_p
                        &&
                        tf[1].ma[0].val > tf[1].ma[1].val
                        &&
                        (fix(true) && false

                            || fix() &&
                            tf[1].adx[0].val >= tf[1].adx[0].val_p

                            || fix() &&
                            tf[1].roc[0].val > 0.15m
                        )
                        &&
                        (fix(true) && false

                            || fix() &&
                            tf[0].adx[0].dip > tf[0].adx[0].dim + new decimal()
                            &&
                            tf[0].adx[0].dip >= tf[0].adx[0].dip_p
                            &&
                            tf[0].roc[0].val > tf[0].roc[0].val_p

                            || fix() &&
                            tf[0].adx[0].val > new decimal(70)
                            &&
                            tf[0].bbw[2].val > tf[0].bbw[2].val_p
                            &&
                            tf[0].GetUpperShadow(0) < new decimal(100)
                            &&
                            tf[0].ComparePrevCandlesTails(Sides.Buy, 2, 50)
                            &&
                            tf[0].volume.acb > tf[0].volume.acb_p
                            &&
                            tf[0].volume.vv > new decimal(5)
                            &&
                            tf[0].volume.vv > tf[0].volume.acv_p.Abs()
                            &&
                            tf[0].volume.GetTv(15) > tf[0].volume.GetTv(30)
                            &&
                            tf[0].volume.GetTv(30) > tf[0].volume.GetTv(60)
                            &&
                            tf[0].volume.GetBv(15) > tf[0].volume.GetBv(30)
                            &&
                            tf[0].volume.GetBv(30) > tf[0].volume.GetBv(60)
                            &&
                            tf[1].bbw[0].val > new decimal(200)
                        )
                        &&
                        (fix(true) && false

                            || fix() &&
                            tf[0].adx[0].val > tf[0].adx[0].val_p          //.......1
                            &&
                            tf[0].adx[0].val > new decimal(45)
                        )
                        &&
                        (fix(true) && false

                                || fix() &&
                                tf[0].bbw[0].val > tf[0].bbw[0].val_p      //.......1
                                &&
                                tf[0].bbw[1].val > tf[0].bbw[1].val_p
                                &&
                                tf[0].bbw[1].val > new decimal(200)

                                || fix() &&
                                tf[0].roc[0].val > new decimal(0.10)       //.......2   
                                &&
                                tf[0].volume.vector > tf[0].volume.vector_hp
                                &&
                                tf[0].volume.vector > -tf[0].volume.vector_lp
                                &&
                                tf[0].volume.vector > tf[0].volume.vector_hpp
                                &&
                                tf[0].volume.vector > -tf[0].volume.vector_lpp
                        )
                        &&
                        (fix(true) && false


                                    || fix() &&
                                    tf[0].volume.vector > tf[0].volume.vector_hp * new decimal(1.1)      //.......1
                                    &&
                                    tf[0].volume.vector > -tf[0].volume.vector_lp * new decimal(1.1)
                        )
                        &&
                        (fix(true) && false

                                || fix() &&
                                tf[1].bbw[0].val > tf[1].bbw[0].val_p        //........1

                                || fix() &&
                                tf[1].bbw[1].val > tf[1].bbw[1].val_p        //........2

                                || fix() &&
                                tf[1].volume.vector > new decimal(3000)      //........3
                                &&
                                tf[1].volume.vector > tf[1].volume.vector_hp
                                &&
                                tf[1].volume.vector > -tf[1].volume.vector_lp

                                || fix() &&
                                tf[1].bbw[0].val > new decimal(500)         //.........4
                        )
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
                    tradeState.ShortOpen = !waitShortReverse
                        //*//
                        &&
                        tf[0].GetCandle().Time.TimeOfDay.TotalMinutes >= 10 * 60 + 30
                        &&
                        tf[0].GetCandle().Time.Hour < 19
                        &&
                        tf[0].volume.Get_Candle_Duration(0) > tf[0].period * new decimal(0.07)
                        &&

                        tf[1].ma[0].val < tf[1].ma[0].val_p
                        &&
                        tf[1].ma[1].val < tf[1].ma[1].val_p
                        &&
                        tf[1].ma[0].val < tf[1].ma[1].val
                        &&
                        (fix(true) && false

                            || fix() &&
                            tf[1].adx[0].val >= tf[1].adx[0].val_p

                            || fix() &&
                            -tf[1].roc[0].val > 0.15m
                        )
                        &&
                        (fix(true) && false

                            || fix() &&
                            tf[0].adx[0].dim > tf[0].adx[0].dip + new decimal()
                            &&
                            tf[0].adx[0].dim >= tf[0].adx[0].dim_p
                            &&
                            tf[0].roc[0].val < tf[0].roc[0].val_p

                            || fix() &&
                            tf[0].adx[0].val > new decimal(70)
                            &&
                            tf[0].bbw[2].val > tf[0].bbw[2].val_p
                            &&
                            tf[0].GetLowerShadow(0) < new decimal(100)
                            &&
                            tf[0].ComparePrevCandlesTails(Sides.Sell, 2, 50)
                            &&
                            tf[0].volume.acs > tf[0].volume.acs_p
                            &&
                            tf[0].volume.vv < -new decimal(5)
                            &&
                            tf[0].volume.vv < -tf[0].volume.acv_p.Abs()
                            &&
                            tf[0].volume.GetTv(15) > tf[0].volume.GetTv(30)
                            &&
                            tf[0].volume.GetTv(30) > tf[0].volume.GetTv(60)
                            &&
                            tf[0].volume.GetSv(15) > tf[0].volume.GetSv(30)
                            &&
                            tf[0].volume.GetSv(30) > tf[0].volume.GetSv(60)
                            &&
                            tf[1].bbw[0].val > new decimal(200)
                        )
                        &&
                        (fix(true) && false

                                || fix() &&
                                tf[0].adx[0].val > tf[0].adx[0].val_p          //.......1
                                &&
                                tf[0].adx[0].val > new decimal(45)
                        )
                        &&
                        (fix(true) && false

                                || fix() &&
                                tf[0].bbw[0].val > tf[0].bbw[0].val_p        //.........1
                                &&
                                tf[0].bbw[1].val > tf[0].bbw[1].val_p
                                &&
                                tf[0].bbw[1].val > new decimal(200)

                                || fix() &&
                                tf[0].roc[0].val < -new decimal(0.10)         //.......2  
                                &&
                                tf[0].volume.vector < tf[0].volume.vector_lp
                                &&
                                tf[0].volume.vector < -tf[0].volume.vector_hp
                                &&
                                tf[0].volume.vector < tf[0].volume.vector_lpp
                                &&
                                tf[0].volume.vector < -tf[0].volume.vector_hpp
                        )
                        &&
                        (fix(true) && false

                                || fix() &&
                                tf[0].volume.vector < tf[0].volume.vector_lp * new decimal(1.1) //..1
                                &&
                                tf[0].volume.vector < -tf[0].volume.vector_hp * new decimal(1.1)
                        )
                        &&
                        (fix(true) && false

                                || fix() &&
                                tf[1].bbw[0].val > tf[1].bbw[0].val_p        //........1

                                || fix() &&
                                tf[1].bbw[1].val > tf[1].bbw[1].val_p        //........2

                                || fix() &&
                                -tf[1].volume.vector > new decimal(3000)      //........3
                                &&
                                tf[1].volume.vector < -tf[1].volume.vector_hp
                                &&
                                tf[1].volume.vector < tf[1].volume.vector_lp

                                || fix() &&
                                tf[1].bbw[0].val > new decimal(500)         //.........4
                        )
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

                if (tradeState.LongOpen) waitLongReverse = true;
                if (tradeState.ShortOpen) waitShortReverse = true;
            }
            {
                /// LONG CLOSE
                ruleId = 0;
                tradeState.LongClose = false
                        //*//
                        || fix() &&
                        PD.Position_PNL <= PD.Position_PNL_MAX - new decimal(200)

                        || fix() &&
                        tf[0].adx[0].dim > tf[0].adx[0].dip
                        &&
                        tf[0].volume.vector < tf[0].volume.vector_lp
                        &&
                        tf[0].volume.vector < -tf[0].volume.vector_hp

                        || fix() &&
                        tf[0].IsRedCandle(0)
                        &&
                        tf[0].GetCandleHLRange(0) > new decimal(200)
                        &&
                        tf[0].GetCandleBodyRange(0) > new decimal(100)
                        &&
                        tf[0].GetCandleHLRange(0) / tf[0].volume.total >
                        tf[0].GetCandleHLRange(1) / tf[0].volume.total_p * new decimal(1.8)

                        || fix() &&
                        PD.Position_PNL_MAX > new decimal(150)
                        &&
                        tf[0].IsRedCandle(0)
                    //*//
                    && true;
                if (_needAction == NeedAction.LongClose)
                    tradeState.RuleId = ruleId;

                /// SHORT CLOSE
                ruleId = 0;
                tradeState.ShortClose = false
                        //*//
                        || fix() &&
                        PD.Position_PNL <= PD.Position_PNL_MAX - new decimal(200)

                        || fix() &&
                        tf[0].adx[0].dip > tf[0].adx[0].dim
                        &&
                        tf[0].volume.vector > tf[0].volume.vector_hp
                        &&
                        tf[0].volume.vector > -tf[0].volume.vector_lp

                        || fix() &&
                        tf[0].IsGreenCandle(0)
                        &&
                        tf[0].GetCandleHLRange(0) > new decimal(200)
                        &&
                        tf[0].GetCandleBodyRange(0) > new decimal(100)
                        &&
                        tf[0].GetCandleHLRange(0) / tf[0].volume.total >
                        tf[0].GetCandleHLRange(1) / tf[0].volume.total_p * new decimal(1.8)

                        || fix() &&
                        PD.Position_PNL_MAX > new decimal(150)
                        &&
                        tf[0].IsGreenCandle(0)
                    //*//
                    && true;
                if (_needAction == NeedAction.ShortClose)
                    tradeState.RuleId = ruleId;
            }


            if (_currentUser == "ro#9019")
            {
                /*GUIServer.MainWindow.Instance.UpdateInfoLabel(string.Format("_{0}_ _{1}_ _{2}_ _{3}_ _{4}_ _{5}_ _{6}_",
                    Math.Round(tf[0].Volume[0].act, 1),
                    Math.Round(tf[0].Volume[0].act_p, 1),
                    Math.Round(tf[0].Volume[0].tv, 1),
                    Math.Round(tf[0].Volume[0].bo, 1),
                   Math.Round(tf[0].Volume[0].so, 1),
                    Math.Round(tf[0].Volume[0].bv, 1),
                   Math.Round(tf[0].Volume[0].sv, 1)
                   ));*/
            }

            return tradeState;
        }
    }
}
