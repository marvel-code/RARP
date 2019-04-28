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

        private bool Is_Position = false;
        private decimal Position_PNL;
        private decimal Position_PNL_MAX;
        private decimal Position_PNL_MIN;
        private decimal Position_AvrTv_MAX = 0;
        private decimal Position_AvrTv_MIN = 0;
        private decimal Position_AvrVv_MAX = 0;
        private decimal Position_AvrVv_MIN = 0;

        private decimal Crocodile_AvrTv1_MAX = 0;
        private decimal Crocodile_AvrTv2_MAX = 0;
        private decimal Crocodile_AvrTv3_MAX = 0;
        private decimal Crocodile_AvrTv1_MIN = 0;
        private decimal Crocodile_AvrTv2_MIN = 0;
        private decimal Crocodile_AvrTv3_MIN = 0;

        private decimal Crocodile_AvrVv1_MAX = 0;
        private decimal Crocodile_AvrVv2_MAX = 0;
        private decimal Crocodile_AvrVv3_MAX = 0;
        private decimal Crocodile_AvrVv1_MIN = 0;
        private decimal Crocodile_AvrVv2_MIN = 0;
        private decimal Crocodile_AvrVv3_MIN = 0;

        public TradeState updateTradeState(List<TimeFrame> _tf, NeedAction _needAction, PartnerDataObject PD)
        {

            // Variables init
            
            int ruleId;
            TradeState tradeState = new TradeState();
            tradeState.RuleId = 0;
            tradeState.LongOpen = false;
            tradeState.LongClose = false;
            tradeState.ShortOpen = false;
            tradeState.ShortClose = false;
            tf = _tf;
            Current_Price = tf[0].GetCandle().ClosePrice;
            Position_PNL = PD.Position_PNL;
            Position_PNL_MAX = PD.Position_PNL_MAX;
            if (_needAction == NeedAction.LongOrShortOpen)
            {
                // Clear block.
                Is_Position = false;
                Position_AvrTv_MAX = Position_AvrTv_MIN = 0;
                Position_AvrVv_MAX = Position_AvrVv_MIN = 0;
                Crocodile_AvrTv1_MAX =
                    Crocodile_AvrTv1_MIN =
                    Crocodile_AvrTv2_MAX =
                    Crocodile_AvrTv2_MIN =
                    Crocodile_AvrTv3_MAX =
                    Crocodile_AvrTv3_MIN = 0;
                Crocodile_AvrVv1_MAX =
                    Crocodile_AvrVv1_MIN =
                    Crocodile_AvrVv2_MAX =
                    Crocodile_AvrVv2_MIN =
                    Crocodile_AvrVv3_MAX =
                    Crocodile_AvrVv3_MIN = 0;
            }
            else if (!Is_Position)
            {
                // Init block.
                Is_Position = true;
                //Position_PNL_MAX = Position_PNL_MIN = 0; These comes from client.
                Position_AvrTv_MAX = Position_AvrTv_MIN = avrTv_4PositionMaxMin;
                Position_AvrVv_MAX = Position_AvrVv_MIN = avrVv_4PositionMaxMin;
                if (Is_Tv_Crocodile)
                {
                    Crocodile_AvrTv1_MAX = Crocodile_AvrTv1_MIN = avrTv1_4CrocodileMaxMin;
                    Crocodile_AvrTv2_MAX = Crocodile_AvrTv2_MIN = avrTv2_4CrocodileMaxMin;
                    Crocodile_AvrTv3_MAX = Crocodile_AvrTv3_MIN = avrTv3_4CrocodileMaxMin;
                }
                if (Is_Vv_Crocodile)
                {
                    Crocodile_AvrVv1_MAX = Crocodile_AvrVv1_MIN = avrVv1_4CrocodileMaxMin;
                    Crocodile_AvrVv2_MAX = Crocodile_AvrVv2_MIN = avrVv2_4CrocodileMaxMin;
                    Crocodile_AvrVv3_MAX = Crocodile_AvrVv3_MIN = avrVv3_4CrocodileMaxMin;
                }
            }

            if (Is_Position)
            {
                // Calc block.
                /* These comes from client.
                // Position PNL MAX\MIN
                if (Position_PNL_MAX < Position_PNL)
                {
                    Position_PNL_MAX = Position_PNL;
                }
                else if (Position_PNL_MIN > Position_PNL)
                {
                    Position_PNL_MIN = Position_PNL;
                }*/
                // Position TV MAX\MIN
                if (avrTv_4PositionMaxMin > Position_AvrTv_MAX)
                {
                    Position_AvrTv_MAX = avrTv_4PositionMaxMin;
                }
                else if (avrTv_4PositionMaxMin < Position_AvrTv_MIN)
                {
                    Position_AvrTv_MIN = avrTv_4PositionMaxMin;
                }
                // Position VV MAX\MIN
                if (avrVv_4PositionMaxMin > Position_AvrVv_MAX)
                {
                    Position_AvrVv_MAX = avrVv_4PositionMaxMin;
                }
                else if (avrVv_4PositionMaxMin < Position_AvrVv_MIN)
                {
                    Position_AvrVv_MIN = avrVv_4PositionMaxMin;
                }
                // Crocodile TV MAX\MIN
                if (!Is_Tv_Crocodile)
                {
                    Crocodile_AvrTv1_MAX =
                        Crocodile_AvrTv1_MIN =
                        Crocodile_AvrTv2_MAX =
                        Crocodile_AvrTv2_MIN =
                        Crocodile_AvrTv3_MAX =
                        Crocodile_AvrTv3_MIN = 0;
                }
                else
                {
                    if (avrTv1_4CrocodileMaxMin > Crocodile_AvrTv1_MAX)
                        Crocodile_AvrTv1_MAX = avrTv1_4CrocodileMaxMin;
                    else if (avrTv1_4CrocodileMaxMin < Crocodile_AvrTv1_MIN)
                        Crocodile_AvrTv1_MIN = avrTv1_4CrocodileMaxMin;
                    if (avrTv2_4CrocodileMaxMin > Crocodile_AvrTv2_MAX)
                        Crocodile_AvrTv2_MAX = avrTv2_4CrocodileMaxMin;
                    else if (avrTv2_4CrocodileMaxMin < Crocodile_AvrTv2_MIN)
                        Crocodile_AvrTv2_MIN = avrTv2_4CrocodileMaxMin;
                    if (avrTv3_4CrocodileMaxMin > Crocodile_AvrTv3_MAX)
                        Crocodile_AvrTv3_MAX = avrTv3_4CrocodileMaxMin;
                    else if (avrTv3_4CrocodileMaxMin < Crocodile_AvrTv3_MIN)
                        Crocodile_AvrTv3_MIN = avrTv3_4CrocodileMaxMin;
                }
                // Crocodile VV MAX\MIN
                if (!Is_Vv_Crocodile)
                {
                    Crocodile_AvrVv1_MAX =
                        Crocodile_AvrVv1_MIN =
                        Crocodile_AvrVv2_MAX =
                        Crocodile_AvrVv2_MIN =
                        Crocodile_AvrVv3_MAX =
                        Crocodile_AvrVv3_MIN = 0;
                }
                else
                {
                    if (avrVv1_4CrocodileMaxMin > Crocodile_AvrVv1_MAX)
                        Crocodile_AvrVv1_MAX = avrVv1_4CrocodileMaxMin;
                    else if (avrVv1_4CrocodileMaxMin < Crocodile_AvrVv1_MIN)
                        Crocodile_AvrVv1_MIN = avrVv1_4CrocodileMaxMin;
                    if (avrVv2_4CrocodileMaxMin > Crocodile_AvrVv2_MAX)
                        Crocodile_AvrVv2_MAX = avrVv2_4CrocodileMaxMin;
                    else if (avrVv2_4CrocodileMaxMin < Crocodile_AvrVv2_MIN)
                        Crocodile_AvrVv2_MIN = avrVv2_4CrocodileMaxMin;
                    if (avrVv3_4CrocodileMaxMin > Crocodile_AvrVv3_MAX)
                        Crocodile_AvrVv3_MAX = avrVv3_4CrocodileMaxMin;
                    else if (avrVv3_4CrocodileMaxMin < Crocodile_AvrVv3_MIN)
                        Crocodile_AvrVv3_MIN = avrVv3_4CrocodileMaxMin;
                }
            }

            // Strategy implementation

            if (_needAction == NeedAction.LongOrShortOpen)
            {
                if (!allow_entry && isEntryAllowedAfterExitRule)
                {
                    allow_entry = true;
                }

                if (allow_entry && !isEntryDenyed)
                {
                    // LONG
                    {
                        ruleId = 0;

                        if (LongCommonRule && !isLongDenyed)
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

                        if (ShortCommonRule && !isShortDenyed)
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
                    {
                        tradeState.RuleId = ruleId;
                        allow_entry = false;

                        Position_AvrTv_MAX = Position_AvrTv_MIN = avrTv_4PositionMaxMin;
                        Position_AvrVv_MAX = Position_AvrVv_MIN = avrVv_4PositionMaxMin;
                        if (Is_Tv_Crocodile)
                        {
                            Crocodile_AvrTv1_MAX = Crocodile_AvrTv1_MIN = avrTv1_4CrocodileMaxMin;
                            Crocodile_AvrTv2_MAX = Crocodile_AvrTv2_MIN = avrTv2_4CrocodileMaxMin;
                            Crocodile_AvrTv3_MAX = Crocodile_AvrTv3_MIN = avrTv3_4CrocodileMaxMin;
                        }
                        if (Is_Vv_Crocodile)
                        {
                            Crocodile_AvrVv1_MAX = Crocodile_AvrVv1_MIN = avrVv1_4CrocodileMaxMin;
                            Crocodile_AvrVv2_MAX = Crocodile_AvrVv2_MIN = avrVv2_4CrocodileMaxMin;
                            Crocodile_AvrVv3_MAX = Crocodile_AvrVv3_MIN = avrVv3_4CrocodileMaxMin;
                        }
                    }
                }
            }
            {
                // Sell
                ruleId = 0;
                if (SellCommonRule)
                {
                    ruleId = SellAdditionalRules.FindIndex(x => x) + 1;
                    if (ruleId != 0)
                    {
                        tradeState.LongClose = true;
                        if (_needAction == NeedAction.LongClose)
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
                        if (_needAction == NeedAction.ShortClose)
                        {
                            tradeState.RuleId = ruleId;
                        }
                    }
                }
            }



            return tradeState;
        }
    }
}
