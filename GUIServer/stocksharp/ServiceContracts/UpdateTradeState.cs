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
        private const bool DYNAMIC = false; // true - включить; false - выключить (ДИНАМИЧЕСКОЕ УСЛОВИЕ)
        private List<TimeFrame> tf;
        private bool allow_entry = true;
        private decimal Current_Price;
        private decimal Position_PNL;

        private bool Is_Position = false;
        private decimal Position_PNL_MAX = 0;
        //private decimal Position_PNL_MIN = 0;
        private decimal Position_Price_MAX = 0;
        private decimal Position_Price_MIN = 0;
        private decimal Position_AvrTv1_MAX = 0;
        private decimal Position_AvrTv1_MIN = 0;
        private decimal Position_AvrVv1_MAX = 0;
        private decimal Position_AvrVv1_MIN = 0;
        private decimal Position_AvrTv2_MAX = 0;
        private decimal Position_AvrTv2_MIN = 0;
        private decimal Position_AvrVv2_MAX = 0;
        private decimal Position_AvrVv2_MIN = 0;
        private decimal Position_AvrTv3_MAX = 0;
        private decimal Position_AvrTv3_MIN = 0;
        private decimal Position_AvrVv3_MAX = 0;
        private decimal Position_AvrVv3_MIN = 0;

        private bool CrocodileOfTvVv_Price_IsInited = false;
        private decimal CrocodileOfTvOrVv_Price_MAX = 0;
        private decimal CrocodileOfTvOrVv_Price_MIN = 0;

        private bool Crocodile_AvrTv_IsInited = false;
        private decimal Crocodile_AvrTv1_MAX = 0;
        private decimal Crocodile_AvrTv2_MAX = 0;
        private decimal Crocodile_AvrTv3_MAX = 0;
        private decimal Crocodile_AvrTv1_MIN = 0;
        private decimal Crocodile_AvrTv2_MIN = 0;
        private decimal Crocodile_AvrTv3_MIN = 0;
        private bool Crocodile_AvrVv_IsInited = false;
        private decimal Crocodile_AvrVv1_MAX = 0;
        private decimal Crocodile_AvrVv2_MAX = 0;
        private decimal Crocodile_AvrVv3_MAX = 0;
        private decimal Crocodile_AvrVv1_MIN = 0;
        private decimal Crocodile_AvrVv2_MIN = 0;
        private decimal Crocodile_AvrVv3_MIN = 0;

        // Gists
        private void updateMaxMin(decimal currentValue, ref decimal minValue, ref decimal maxValue, bool isInited = true)
        {
            if (!isInited)
            {
                minValue = maxValue = currentValue;
            }
            else
            {
                if (currentValue > maxValue)
                    maxValue = currentValue;
                else if (currentValue < minValue)
                    minValue = currentValue;
            }
        }

        // Crocodiles
        private void updateCrocodileAvrTvsExtremums()
        {
            if (Is_Tv_Crocodile())
            {
                int k = 0;
                do
                {
                    decimal
                        avrTv1 = tf[0].volume.GetAvrTv(avrTvPeriod_1, k),
                        avrTv2 = tf[0].volume.GetAvrTv(avrTvPeriod_2, k),
                        avrTv3 = tf[0].volume.GetAvrTv(avrTvPeriod_3, k);


                    updateMaxMin(avrTv1, ref Crocodile_AvrTv1_MIN, ref Crocodile_AvrTv1_MAX, Crocodile_AvrTv_IsInited);
                    updateMaxMin(avrTv2, ref Crocodile_AvrTv2_MIN, ref Crocodile_AvrTv2_MAX, Crocodile_AvrTv_IsInited);
                    updateMaxMin(avrTv3, ref Crocodile_AvrTv3_MIN, ref Crocodile_AvrTv3_MAX, Crocodile_AvrTv_IsInited);
                } while (Is_Tv_Crocodile(++k));

                Crocodile_AvrTv_IsInited = true;
            }
            else
            {
                Crocodile_AvrTv1_MAX = Crocodile_AvrTv1_MIN = 0;
                Crocodile_AvrTv2_MAX = Crocodile_AvrTv2_MIN = 0;
                Crocodile_AvrTv3_MAX = Crocodile_AvrTv3_MIN = 0;

                Crocodile_AvrTv_IsInited = false;
            }
        }
        private void updateCrocodileAvrVvsExtremums()
        {
            if (Is_Vv_Crocodile())
            {
                int k = 0;
                do
                {
                    decimal
                        avrVv1 = tf[0].volume.GetAvrVv(avrVvPeriod_1, k),
                        avrVv2 = tf[0].volume.GetAvrVv(avrVvPeriod_2, k),
                        avrVv3 = tf[0].volume.GetAvrVv(avrVvPeriod_3, k);

                    updateMaxMin(avrVv1, ref Crocodile_AvrVv1_MIN, ref Crocodile_AvrVv1_MAX, Crocodile_AvrVv_IsInited);
                    updateMaxMin(avrVv2, ref Crocodile_AvrVv2_MIN, ref Crocodile_AvrVv2_MAX, Crocodile_AvrVv_IsInited);
                    updateMaxMin(avrVv3, ref Crocodile_AvrVv3_MIN, ref Crocodile_AvrVv3_MAX, Crocodile_AvrVv_IsInited);
                } while (Is_Vv_Crocodile(++k));

                Crocodile_AvrVv_IsInited = true;
            }
            else
            {
                Crocodile_AvrVv1_MAX = Crocodile_AvrVv1_MIN = 0;
                Crocodile_AvrVv2_MAX = Crocodile_AvrVv2_MIN = 0;
                Crocodile_AvrVv3_MAX = Crocodile_AvrVv3_MIN = 0;

                Crocodile_AvrVv_IsInited = false;
            }
        }
        private void updateCrocodileOfTvOrVvPriceExtremums()
        {
            if (Is_Tv_Crocodile() || Is_Vv_Crocodile())
            {
                updateMaxMin(Current_Price, ref CrocodileOfTvOrVv_Price_MIN, ref CrocodileOfTvOrVv_Price_MAX, CrocodileOfTvVv_Price_IsInited);

                CrocodileOfTvVv_Price_IsInited = true;
            }
            else
            {
                CrocodileOfTvOrVv_Price_MAX = CrocodileOfTvOrVv_Price_MIN = 0;

                CrocodileOfTvVv_Price_IsInited = false;
            }
        }

        private void updateTradeStateLongShort(ref TradeState tradeState, NeedAction needAction)
        {
            if (needAction == NeedAction.LongOrShortOpen)
            {
                int ruleId;

                // LONG
                {
                    ruleId = 0;

                    if (LongCommonRule && isLongAllowed)
                    {
                        for (int i = 0; i < LongAdditionalRules.Count; i++)
                        {
                            ruleId *= 10;
                            int index = LongAdditionalRules[i].FindIndex(x => x);
                            if (index == -1)
                            {
                                ruleId = 0;
                                break;
                            }
                            ruleId += index + 1;
                        }

                        if (ruleId != 0)
                        {
                            tradeState.LongOpen = true;
                        }
                    }
                }

                // SHORT
                if (!tradeState.LongOpen)
                {
                    ruleId = 0;

                    if (ShortCommonRule && isShortAllowed)
                    {
                        for (int i = 0; i < ShortAdditionalRules.Count; i++)
                        {
                            ruleId *= 10;
                            int index = ShortAdditionalRules[i].FindIndex(x => x);
                            if (index == -1)
                            {
                                ruleId = 0;
                                break;
                            }
                            ruleId += index + 1;
                        }

                        if (ruleId != 0)
                        {
                            tradeState.ShortOpen = true;
                        }
                    }
                }

                if (ruleId != 0)
                    tradeState.RuleId = ruleId;
            }
        }
        private void updateTradeStateSellCover(ref TradeState tradeState, NeedAction needAction)
        {
            int ruleId;

            // Sell
            ruleId = 0;
            if (SellCommonRule)
            {
                ruleId = SellAdditionalRules.FindIndex(x => x) + 1;
                if (ruleId != 0)
                {
                    tradeState.LongClose = true;
                    if (needAction == NeedAction.LongClose)
                    {
                        tradeState.RuleId = ruleId;
                    }
                }
            }

            // Cover
            ruleId = 0;
            if (CoverCommonRule)
            {
                ruleId = CoverAdditionalRules.FindIndex(x => x) + 1;
                if (ruleId != 0)
                {
                    tradeState.ShortClose = true;
                    if (needAction == NeedAction.ShortClose)
                    {
                        tradeState.RuleId = ruleId;
                    }
                }
            }
        }

        bool wasLastExitByStrategy = true;
        public TradeState getTradeState(List<TimeFrame> timeFrames, NeedAction needAction, PartnerDataObject TM)
        {

            // Variables init

            int ruleId;
            TradeState tradeState = new TradeState();
            Current_Price = tf[0].GetCandle().ClosePrice;
            Position_PNL = TM.Position_PNL;
            if (needAction == NeedAction.LongOrShortOpen)
            {
                // Clear block.
                Is_Position = false;
                Position_Price_MAX = Position_Price_MIN = 0;
                Position_AvrTv1_MAX = Position_AvrTv1_MIN = 0;
                Position_AvrVv1_MAX = Position_AvrVv1_MIN = 0;
                Position_AvrTv2_MAX = Position_AvrTv2_MIN = 0;
                Position_AvrVv2_MAX = Position_AvrVv2_MIN = 0;
                Position_AvrTv3_MAX = Position_AvrTv3_MIN = 0;
                Position_AvrVv3_MAX = Position_AvrVv3_MIN = 0;
            }


            // Strategy implementation

            if (needAction == NeedAction.LongOrShortOpen)
            {
                if (!allow_entry && isEntryAllowedAfterExitRule)
                {
                    allow_entry = true;
                }

                if (allow_entry && isEntryAllowed)
                {
                    updateTradeStateLongShort(ref tradeState, needAction);

                    if (tradeState.RuleId != 0)
                    {
                        allow_entry = false;

                        // Init block.
                        Is_Position = true;
                        Position_PNL_MAX = /*Position_PNL_MIN =*/ 0;
                        Position_Price_MAX = Position_Price_MIN = Current_Price;
                        Position_AvrTv1_MAX = Position_AvrTv1_MIN = avrTv_4PositionMaxMin_1;
                        Position_AvrVv1_MAX = Position_AvrVv1_MIN = avrVv_4PositionMaxMin_1;
                        Position_AvrTv2_MAX = Position_AvrTv2_MIN = avrTv_4PositionMaxMin_2;
                        Position_AvrVv2_MAX = Position_AvrVv2_MIN = avrVv_4PositionMaxMin_2;
                        Position_AvrTv3_MAX = Position_AvrTv3_MIN = avrTv_4PositionMaxMin_3;
                        Position_AvrVv3_MAX = Position_AvrVv3_MIN = avrVv_4PositionMaxMin_3;
                    }
                }
            }
            {
                updateTradeStateSellCover(ref tradeState, needAction);
            }

            // Calc block. (after rules)
            if (Is_Position)
            {
                updateMaxMin(Current_Price, ref Position_Price_MIN, ref Position_Price_MAX);
                //updateMaxMin(Position_PNL, ref Position_PNL_MIN, ref Position_PNL_MAX); -- Comes from client
                {
                    Position_PNL_MAX = TM.Position_PNL_MAX;
                    //Position_PNL_MIN = TM.Position_PNL_MIN;
                }
                updateMaxMin(avrTv_4PositionMaxMin_1, ref Position_AvrTv1_MIN, ref Position_AvrTv1_MAX);
                updateMaxMin(avrTv_4PositionMaxMin_2, ref Position_AvrTv2_MIN, ref Position_AvrTv2_MAX);
                updateMaxMin(avrTv_4PositionMaxMin_3, ref Position_AvrTv3_MIN, ref Position_AvrTv3_MAX);
                updateMaxMin(avrVv_4PositionMaxMin_1, ref Position_AvrVv1_MIN, ref Position_AvrVv1_MAX);
                updateMaxMin(avrVv_4PositionMaxMin_2, ref Position_AvrVv2_MIN, ref Position_AvrVv2_MAX);
                updateMaxMin(avrVv_4PositionMaxMin_3, ref Position_AvrVv3_MIN, ref Position_AvrVv3_MAX);
            }
            // Crocodiles
            updateCrocodileAvrTvsExtremums();
            updateCrocodileAvrVvsExtremums();
            updateCrocodileOfTvOrVvPriceExtremums();

            if (needAction == NeedAction.LongOrShortOpen && (tradeState.LongOpen || tradeState.ShortOpen))
            {
                // Entry
                wasLastExitByStrategy = false;
            }
            if (needAction != NeedAction.LongOrShortOpen && (tradeState.LongClose || tradeState.ShortClose))
            {
                // Exit
                wasLastExitByStrategy = true;
            }

            return tradeState;
        }
    }
}
