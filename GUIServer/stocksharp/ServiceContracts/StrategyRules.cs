using System;
using System.Collections.Generic;
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
                            tf[0].getCandle().Time.TimeOfDay.TotalMinutes > 10 * 60 + 30
                            &&
                            tf[0].getCandle().Time.Hour < 19
                            &&
                            tf[0].adx[0].dip > tf[0].adx[0].dim + new decimal(0)  //10-15-20
                            &&
                            tf[0].adx[0].dip >= tf[0].adx[0].dip_p               //adx[0]-adx[1]
                            &&
                            tf[0].adx[0].val >= tf[0].adx[0].val_p
                            &&
                            tf[0].volume[0].tv > new decimal(10)       //5-10-15-20  
                            &&
                            tf[0].volume[0].actv > tf[0].volume[0].actv_p
                            &&
                            tf[0].volume[0].bv > tf[0].volume[0].sv
                            &&
                            tf[0].volume[0].bo > tf[0].volume[0].so
                            &&
                            (fix(true) && false

                                || fix() &&
                                tf[0].adx[0].val > new decimal(50)   //30-40-50-60           //.......1
                                &&
                                tf[0].roc[0].val > new decimal(0.15)   //0.13-0.15-0.17    

                                || fix() &&
                                tf[0].adx[0].dip > tf[0].adx[0].val
                                &&
                                tf[0].adx[0].val > tf[0].adx[0].val_p + new decimal(7)
                                &&
                                tf[0].roc[0].val > new decimal(0.15)   //0.13-0.15-0.17    

                                || fix() &&
                                tf[0].adx[0].val > new decimal(50)   //30-40-50-60           //.......1
                                &&
                                tf[0].roc[1].val > new decimal(0.15)  //0.13-0.15-0.17

                                || fix() &&
                                tf[0].adx[0].dip > tf[0].adx[0].val
                                &&
                                tf[0].adx[0].val > tf[0].adx[0].val_p + new decimal(7)
                                &&
                                tf[0].roc[1].val > new decimal(0.15)  //0.13-0.15-0.17

                                || fix() &&
                                tf[0].roc[0].val > new decimal(0.15)  //0.10-0.12-0.15      //........3  
                                &&
                                tf[0].volume[0].actv > tf[0].volume[0].actv_p * new decimal(1.5)  //1.2-1.5-2.0-2.5
                                &&
                                tf[0].volume[0].tv > new decimal(20)    //15-20-30      period=60sec -30-120-180
                                &&
                                tf[0].volume[0].bv > tf[0].volume[0].sv
                                &&
                                tf[0].getCandleHLRange(0) > tf[0].getCandleHLRange(1) * new decimal(1.1)  // 1.0-1.1-1.2
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
                    tradeState.ShortOpen = true
                            //*//
                            &&
                            tf[0].getCandle().Time.TimeOfDay.TotalMinutes > 10 * 60 + 30
                            &&
                            tf[0].getCandle().Time.Hour < 19
                            &&
                            tf[0].adx[0].dim > tf[0].adx[0].dip + new decimal(0)  //10-15-20
                            &&
                            tf[0].adx[0].dim >= tf[0].adx[0].dim_p               //adx[0]-adx[1]
                            &&
                            tf[0].adx[0].val >= tf[0].adx[0].val_p
                            &&
                            tf[0].volume[0].tv > new decimal(10)       //5-10-15-20  
                            &&
                            tf[0].volume[0].actv > tf[0].volume[0].actv_p
                            &&
                            tf[0].volume[0].sv > tf[0].volume[0].bv
                            &&
                            tf[0].volume[0].so > tf[0].volume[0].bo
                            &&
                            (fix(true) && false

                                || fix() &&
                                tf[0].adx[0].val > new decimal(50)   //30-40-50-60           //.......1
                                &&
                                -tf[0].roc[0].val > new decimal(0.15)   //0.13-0.15-0.17    

                                || fix() &&
                                tf[0].adx[0].dim > tf[0].adx[0].val
                                &&
                                tf[0].adx[0].val > tf[0].adx[0].val_p + new decimal(7)
                                &&
                                -tf[0].roc[0].val > new decimal(0.15)   //0.13-0.15-0.17    

                                || fix() &&
                                tf[0].adx[0].val > new decimal(50)   //30-40-50-60           //.......1
                                &&
                                -tf[0].roc[1].val > new decimal(0.15)  //0.13-0.15-0.17

                                || fix() &&
                                tf[0].adx[0].dim > tf[0].adx[0].val
                                &&
                                tf[0].adx[0].val > tf[0].adx[0].val_p + new decimal(7)
                                &&
                                -tf[0].roc[1].val > new decimal(0.15)  //0.13-0.15-0.17

                                || fix() &&
                                -tf[0].roc[0].val > new decimal(0.15)  //0.10-0.12-0.15      //........3  
                                &&
                                tf[0].volume[0].actv > tf[0].volume[0].actv_p * new decimal(1.5)  //1.2-1.5-2.0-2.5
                                &&
                                tf[0].volume[0].tv > new decimal(20)    //15-20-30      period=60sec -30-120-180
                                &&
                                tf[0].volume[0].sv > tf[0].volume[0].bv
                                &&
                                tf[0].getCandleHLRange(0) > tf[0].getCandleHLRange(1) * new decimal(1.1)  // 1.0-1.1-1.2
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
            }
            {
                /// LONG CLOSE
                ruleId = 0;
                tradeState.LongClose = false
                        //*//
                        || fix() &&
                        tf[0].adx[0].dim > tf[0].adx[0].dip                       //.......1

                        || fix() &&
                        -tf[0].volume[0].vector > new decimal(1300)
                        &&
                        partnerData.Position_PNL_Max < 300

                        || fix() &&
                        tf[0].adx[0].val < tf[0].adx[0].val_p                     //.......3
                        &&
                        tf[0].adx[0].val < new decimal(50)
                    //*//
                    && true;
                if (_needAction == NeedAction.LongClose)
                    tradeState.RuleId = ruleId;

                /// SHORT CLOSE
                ruleId = 0;
                tradeState.ShortClose = false
                        //*//
                        || fix() &&
                        tf[0].adx[0].dip > tf[0].adx[0].dim                       //.......1

                        || fix() &&
                        tf[0].volume[0].vector > new decimal(1300)
                        &&
                        partnerData.Position_PNL_Max < 300

                        || fix() &&
                        tf[0].adx[0].val < tf[0].adx[0].val_p                     //.......3
                        &&
                        tf[0].adx[0].val < new decimal(50)
                    //*//
                    && true;
                if (_needAction == NeedAction.ShortClose)
                    tradeState.RuleId = ruleId;
            }


            if (_currentUser == "ro#9019")
            {
                //GUIServer.MainWindow.Instance.UpdateInfoLabel(string.Format("{0} = {1} + {2}", tf[0].volume[0].actv, tf[0].volume[0].acbv, tf[0].volume[0].acsv));
            }

            return tradeState;
        }
    }
}
