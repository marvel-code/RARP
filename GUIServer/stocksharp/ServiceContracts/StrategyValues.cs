using System;
using System.Collections.Generic;

namespace stocksharp.ServiceContracts
{
    public partial class WorkService
    {
        private class Extremums
        {
            public enum ExtremumType { Max, Min }
            public class Extremum
            {
                private readonly ExtremumType _extremumType;

                public decimal Value { get; private set; }
                public DateTimeOffset StartDateTime { get; private set; }
                public int TactsDuration { get; private set; }

                public Extremum Init(decimal value, DateTimeOffset startDateTime)
                {
                    Value = value;
                    StartDateTime = startDateTime;
                    TactsDuration = 0;
                    return this;
                }
                public Extremum Update(decimal value, DateTimeOffset currentTime)
                {
                    if (_extremumType == ExtremumType.Max && value > Value ||
                        _extremumType == ExtremumType.Min && value < Value)
                    {
                        Value = value;
                        StartDateTime = currentTime;
                    }
                    TactsDuration = (int)(currentTime - StartDateTime).TotalSeconds / MySettings.VV_TACT;
                    return this;
                }

                public Extremum(ExtremumType extremumType)
                {
                    _extremumType = extremumType;
                }
            }

            public Extremum Max = new Extremum(ExtremumType.Max);
            public Extremum Min = new Extremum(ExtremumType.Min);
        }
        private class PositionVelocitiesValues
        {
            public class Velocity
            {
                public Extremums Tv = new Extremums();
                public Extremums Vv = new Extremums();
            }

            public int PeriodSeconds { get; private set; }
            public int ExpN { get; private set; }

            public Velocity Avr = new Velocity();
            public Velocity Exp = new Velocity();

            public PositionVelocitiesValues(int periodSeconds, int expN)
            {
                PeriodSeconds = periodSeconds;
                ExpN = expN;
            }
        }
        private class PositionPricesValues
        {
            public Dictionary<int, Extremums> ExpReal = new Dictionary<int, Extremums>();
            public Extremums Real = new Extremums();
            // TODO
            //public Extremums ByTrades = new Extremums();
            //public Extremums ByPeriod = new Extremums();

            public PositionPricesValues(int[] expNs)
            {
                foreach (int n in expNs)
                {
                    ExpReal.Add(n, new Extremums());
                }
            }
        }

        private decimal Current_Price;
        private decimal Current_Day_PNL;
        private decimal Position_PNL;
        private DateTimeOffset Current_Time;
        // Position
        private bool Is_Position = false;
        private decimal Position_PNL_MAX = 0;
        private decimal Position_PNL_MIN = 0;
        private decimal Position_Price_MAX = 0;
        private decimal Position_Price_MIN = 0;
        private DateTimeOffset StartPositionTime;
        private TimeSpan Position_Duration;
        private List<PositionVelocitiesValues> PositionVelocities;
        private PositionPricesValues PositionPrices;

        private void updateMaxMin(decimal currentValue, ref decimal minValue, ref decimal maxValue, bool isInited = true)
        {
            if (!isInited)
            {
                minValue = maxValue = currentValue;
            }
            else
                if (currentValue > maxValue)
            {
                maxValue = currentValue;
            }
            else if (currentValue < minValue)
            {
                minValue = currentValue;
            }
        }
        private void initPositionValues()
        {
            // Velocities
            PositionVelocities = new List<PositionVelocitiesValues>();
            foreach (KeyValuePair<int, int> vs in MySettings.VELOCITIES_SETTINGS)
            {
                PositionVelocities.Add(new PositionVelocitiesValues(vs.Key, vs.Value));
            }

            foreach (PositionVelocitiesValues pv in PositionVelocities)
            {
                pv.Avr.Tv.Max.Init(tf[0].volume.GetAvrTv(pv.PeriodSeconds), Current_Time);
                pv.Avr.Tv.Min.Init(tf[0].volume.GetAvrTv(pv.PeriodSeconds), Current_Time);
                pv.Avr.Vv.Max.Init(tf[0].volume.GetAvrVv(pv.PeriodSeconds), Current_Time);
                pv.Avr.Vv.Min.Init(tf[0].volume.GetAvrVv(pv.PeriodSeconds), Current_Time);
                pv.Exp.Tv.Max.Init(tf[0].volume.GetExpTv(pv.PeriodSeconds, pv.ExpN), Current_Time);
                pv.Exp.Tv.Min.Init(tf[0].volume.GetExpTv(pv.PeriodSeconds, pv.ExpN), Current_Time);
                pv.Exp.Vv.Max.Init(tf[0].volume.GetExpVv(pv.PeriodSeconds, pv.ExpN), Current_Time);
                pv.Exp.Vv.Min.Init(tf[0].volume.GetExpVv(pv.PeriodSeconds, pv.ExpN), Current_Time);
            }
            // Prices
            PositionPrices = new PositionPricesValues(MySettings.PRICE_SETTINGS);
            PositionPrices.Real.Max.Init(tf[0].volume.GetTactRealPrice(), Current_Time);
            PositionPrices.Real.Min.Init(tf[0].volume.GetTactRealPrice(), Current_Time);
            foreach (KeyValuePair<int, Extremums> pv in PositionPrices.ExpReal)
            {
                pv.Value.Max.Init(tf[0].volume.GetTactExpPrice(pv.Key), Current_Time);
                pv.Value.Min.Init(tf[0].volume.GetTactExpPrice(pv.Key), Current_Time);
            }
        }
        private void updatePositionValues()
        {
            // Velocities
            foreach (PositionVelocitiesValues pv in PositionVelocities)
            {
                pv.Avr.Tv.Max.Update(tf[0].volume.GetAvrTv(pv.PeriodSeconds), Current_Time);
                pv.Avr.Tv.Min.Update(tf[0].volume.GetAvrTv(pv.PeriodSeconds), Current_Time);
                pv.Avr.Vv.Max.Update(tf[0].volume.GetAvrVv(pv.PeriodSeconds), Current_Time);
                pv.Avr.Vv.Min.Update(tf[0].volume.GetAvrVv(pv.PeriodSeconds), Current_Time);
                pv.Exp.Tv.Max.Update(tf[0].volume.GetExpTv(pv.PeriodSeconds, pv.ExpN), Current_Time);
                pv.Exp.Tv.Min.Update(tf[0].volume.GetExpTv(pv.PeriodSeconds, pv.ExpN), Current_Time);
                pv.Exp.Vv.Max.Update(tf[0].volume.GetExpVv(pv.PeriodSeconds, pv.ExpN), Current_Time);
                pv.Exp.Vv.Min.Update(tf[0].volume.GetExpVv(pv.PeriodSeconds, pv.ExpN), Current_Time);
            }

            // Prices
            PositionPrices.Real.Max.Update(tf[0].volume.GetTactRealPrice(), Current_Time);
            PositionPrices.Real.Min.Update(tf[0].volume.GetTactRealPrice(), Current_Time);
            foreach (KeyValuePair<int, Extremums> pv in PositionPrices.ExpReal)
            {
                pv.Value.Max.Update(tf[0].volume.GetTactExpPrice(pv.Key), Current_Time);
                pv.Value.Min.Update(tf[0].volume.GetTactExpPrice(pv.Key), Current_Time);
            }
        }

        /* Blocks */

        // Rules
        private void updatePreRulesBlock()
        {
            Current_Day_PNL = TM.Day_PNL;
            Current_Price = TM.Current_Price;
            Current_Time = TM.TerminalTime;
        }
        private void updatePostRulesBlock()
        {
        }
        // Open rules
        private void updatePreOpenRulesBlock()
        {
        }
        private void updatePostOpenRulesBlock()
        {
        }
        // Exit rules
        private void updatePreExitRulesBlock()
        {
            Position_PNL = TM.Position_PNL;
            Position_Duration = Current_Time - StartPositionTime;
        }
        private void updatePostExitRulesBlock()
        {
            updateMaxMin(Current_Price, ref Position_Price_MIN, ref Position_Price_MAX);
            updateMaxMin(Position_PNL, ref Position_PNL_MIN, ref Position_PNL_MAX);
            updatePositionValues();
        }
        // Position
        private void updatePrePositionBlock()
        {
            Is_Position = true;
            StartPositionTime = Current_Time;
            Position_PNL_MAX = Position_PNL_MIN = 0;
            Position_Price_MAX = Position_Price_MIN = Current_Price;
            initPositionValues();
        }
        private void updatePostPositionBlock()
        {
            Is_Position = false;
        }
    }
}
