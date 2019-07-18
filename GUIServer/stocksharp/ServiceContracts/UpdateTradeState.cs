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
        PartnerDataObject TM;
        private List<TimeFrame> tf;
        bool isPostPositionBlockUpdated = false;
        private bool allow_entry = true;
        private NeedAction needAction_;
        bool wasLastExitByStrategy = true;
        private const bool DYNAMIC = false; // true - включить; false - выключить (ДИНАМИЧЕСКОЕ УСЛОВИЕ)
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
        public TradeState getTradeState(List<TimeFrame> timeFrames, NeedAction needAction, PartnerDataObject TM_)
        {
            tf = timeFrames;
            TM = TM_;

            if (needAction == NeedAction.LongOrShortOpen && !wasLastExitByStrategy && !isPostPositionBlockUpdated)
            {
                updatePostPositionBlock();
                isPostPositionBlockUpdated = true;
            }


            // Variables init

            needAction_ = needAction;
            TradeState tradeState = new TradeState();

            updatePreRulesBlock();

            // Strategy implementation

            if (needAction == NeedAction.LongOrShortOpen)
            {
                updatePreOpenRulesBlock();

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

                        updatePrePositionBlock();
                    }
                }

                updatePostOpenRulesBlock();
            }
            {
                if (needAction != NeedAction.LongOrShortOpen)
                    updatePreExitRulesBlock();

                updateTradeStateSellCover(ref tradeState, needAction);

                if (needAction != NeedAction.LongOrShortOpen)
                    updatePostExitRulesBlock();
            }

            // Other

            updatePostRulesBlock();

            if (needAction == NeedAction.LongOrShortOpen && (tradeState.LongOpen || tradeState.ShortOpen))
            {
                // Entry
                wasLastExitByStrategy = false;
                updatePrePositionBlock();
                isPostPositionBlockUpdated = false;
            }
            if (needAction != NeedAction.LongOrShortOpen && (tradeState.LongClose || tradeState.ShortClose))
            {
                // Exit
                wasLastExitByStrategy = true;
                updatePostPositionBlock();
            }

            return tradeState;
        }
    }
}
