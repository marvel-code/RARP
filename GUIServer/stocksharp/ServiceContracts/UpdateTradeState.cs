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
        private decimal Position_PNL;
        private decimal Position_PNL_MAX;
        private decimal Current_Price;

        public TradeState updateTradeState(List<TimeFrame> _tf, NeedAction _needAction, PartnerDataObject PD)
        {
            int ruleId;
            TradeState tradeState = new TradeState();
            tradeState.RuleId = 0;
            tradeState.LongOpen = false;
            tradeState.LongClose = false;
            tradeState.ShortOpen = false;
            tradeState.ShortClose = false;
            tf = _tf;
            Position_PNL = PD.Position_PNL;
            Position_PNL_MAX = PD.Position_PNL_MAX;
            Current_Price = tf[0].GetCandle().ClosePrice;

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
