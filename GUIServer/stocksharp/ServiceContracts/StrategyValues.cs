using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace stocksharp.ServiceContracts
{
    public partial class WorkService
    {
        private decimal Current_Price;
        private decimal Current_Day_PNL;
        private decimal Position_PNL;
        private DateTimeOffset Current_Time;
        private DateTimeOffset StartPositionTime;
        private TimeSpan PositionDuration;

        private bool Is_Position = false;
        private decimal Position_PNL_MAX = 0;
        private decimal Position_PNL_MIN = 0;
        private decimal Position_Price_MAX = 0;
        private decimal Position_Price_MIN = 0;
        private decimal Position_AvrTv1_MAX = 0;
        private decimal Position_AvrTv1_MIN = 0;
        private decimal Position_AvrTv2_MAX = 0;
        private decimal Position_AvrTv2_MIN = 0;
        private decimal Position_AvrTv3_MAX = 0;
        private decimal Position_AvrTv3_MIN = 0;
        private decimal Position_AvrVv1_MAX = 0;
        private decimal Position_AvrVv1_MIN = 0;
        private decimal Position_AvrVv2_MAX = 0;
        private decimal Position_AvrVv2_MIN = 0;
        private decimal Position_AvrVv3_MAX = 0;
        private decimal Position_AvrVv3_MIN = 0;
        private bool CrocodileTvVv_PriceExtremums_IsInited = false;
        private decimal CrocodileOfTvOrVv_Price_MAX = 0;
        private decimal CrocodileOfTvOrVv_Price_MIN = 0;
        private bool CrocodileTv_PriceExtremums_IsInited = false;
        private decimal CrocodileOfTv_Price_MAX = 0;
        private decimal CrocodileOfTv_Price_MIN = 0;
        private bool Crocodile_Tv_IsInited = false;
        private bool Is_Tv_Crocodile(int shift = 0) =>
                tf[0].volume.GetAvrTv(avrTvPeriod_3, shift) > tf[0].volume.GetAvrTv(avrTvPeriod_2, shift)
                ||
                tf[0].volume.GetAvrTv(avrTvPeriod_2, shift) > tf[0].volume.GetAvrTv(avrTvPeriod_1, shift);
        private decimal Crocodile_2Tv1_MAX = 0;
        private decimal Crocodile_2Tv2_MAX = 0;
        private decimal Crocodile_2Tv3_MAX = 0;
        private decimal Crocodile_2Tv1_MIN = 0;
        private decimal Crocodile_2Tv2_MIN = 0;
        private decimal Crocodile_2Tv3_MIN = 0;
        private bool Crocodile_AvrVv_IsInited = false;
        private bool Is_Vv_Crocodile(int shift = 0) => // TODO: IT'S NOT VV CROCODILE
                tf[0].volume.GetAvrVv(avrVvPeriod_1, shift) > 0
                &&
                tf[0].volume.GetAvrVv(avrVvPeriod_2, shift) > tf[0].volume.GetAvrVv(avrVvPeriod_1, shift)
                ||
                tf[0].volume.GetAvrVv(avrVvPeriod_1, shift) < 0
                &&
                tf[0].volume.GetAvrVv(avrVvPeriod_2, shift) < tf[0].volume.GetAvrVv(avrVvPeriod_1, shift);
        private decimal Crocodile_AvrVv1_MAX = 0;
        private decimal Crocodile_AvrVv2_MAX = 0;
        private decimal Crocodile_AvrVv3_MAX = 0;
        private decimal Crocodile_AvrVv1_MIN = 0;
        private decimal Crocodile_AvrVv2_MIN = 0;
        private decimal Crocodile_AvrVv3_MIN = 0;

        /* Gists */

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

        /* Crocodiles */

        // Updaters
        private void updateCrocodileTvsExtremums()
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

                    updateMaxMin(avrTv1, ref Crocodile_2Tv1_MIN, ref Crocodile_2Tv1_MAX, Crocodile_Tv_IsInited);
                    updateMaxMin(avrTv2, ref Crocodile_2Tv2_MIN, ref Crocodile_2Tv2_MAX, Crocodile_Tv_IsInited);
                    updateMaxMin(avrTv3, ref Crocodile_2Tv3_MIN, ref Crocodile_2Tv3_MAX, Crocodile_Tv_IsInited);
                } while (Is_Tv_Crocodile(++k));

                Crocodile_Tv_IsInited = true;
            }
            else
            {
                Crocodile_2Tv1_MAX = Crocodile_2Tv1_MIN = 0;
                Crocodile_2Tv2_MAX = Crocodile_2Tv2_MIN = 0;
                Crocodile_2Tv3_MAX = Crocodile_2Tv3_MIN = 0;

                Crocodile_Tv_IsInited = false;
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
        private void updateCrocodileTvVvPriceExtremums()
        {
            if (Is_Tv_Crocodile() || Is_Vv_Crocodile())
            {
                updateMaxMin(Current_Price, ref CrocodileOfTvOrVv_Price_MIN, ref CrocodileOfTvOrVv_Price_MAX, CrocodileTvVv_PriceExtremums_IsInited);

                CrocodileTvVv_PriceExtremums_IsInited = true;
            }
            else
            {
                CrocodileOfTvOrVv_Price_MAX = CrocodileOfTvOrVv_Price_MIN = 0;

                CrocodileTvVv_PriceExtremums_IsInited = false;
            }
        }
        private void updateCrocodileTvPriceExtremums()
        {
            if (Is_Tv_Crocodile())
            {
                updateMaxMin(Current_Price, ref CrocodileOfTv_Price_MIN, ref CrocodileOfTv_Price_MAX, CrocodileTv_PriceExtremums_IsInited);

                CrocodileTv_PriceExtremums_IsInited = true;
            }
            else
            {
                CrocodileOfTvOrVv_Price_MAX = CrocodileOfTvOrVv_Price_MIN = 0;

                CrocodileTv_PriceExtremums_IsInited = false;
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
            // Crocodiles
            updateCrocodileTvVvPriceExtremums();
            updateCrocodileTvPriceExtremums();
            updateCrocodileTvsExtremums();
            updateCrocodileAvrVvsExtremums();
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
            PositionDuration = Current_Time - StartPositionTime;
        }
        private void updatePostExitRulesBlock()
        {
            updateMaxMin(Current_Price, ref Position_Price_MIN, ref Position_Price_MAX);
            updateMaxMin(Position_PNL, ref Position_PNL_MIN, ref Position_PNL_MAX);
            updateMaxMin(avrTv_4PositionMaxMin_1, ref Position_AvrTv1_MIN, ref Position_AvrTv1_MAX);
            updateMaxMin(avrTv_4PositionMaxMin_2, ref Position_AvrTv2_MIN, ref Position_AvrTv2_MAX);
            updateMaxMin(avrTv_4PositionMaxMin_3, ref Position_AvrTv3_MIN, ref Position_AvrTv3_MAX);
            updateMaxMin(avrVv_4PositionMaxMin_1, ref Position_AvrVv1_MIN, ref Position_AvrVv1_MAX);
            updateMaxMin(avrVv_4PositionMaxMin_2, ref Position_AvrVv2_MIN, ref Position_AvrVv2_MAX);
            updateMaxMin(avrVv_4PositionMaxMin_3, ref Position_AvrVv3_MIN, ref Position_AvrVv3_MAX);
            updateCrocodileTvVvPriceExtremums();
            updateCrocodileTvPriceExtremums();
            updateCrocodileTvsExtremums();
            updateCrocodileAvrVvsExtremums();
        }
        // Position
        private void updatePrePositionBlock()
        {
            Is_Position = true;
            StartPositionTime = Current_Time;
            Position_PNL_MAX = Position_PNL_MIN = 0;
            Position_Price_MAX = Position_Price_MIN = Current_Price;
            Position_AvrTv1_MAX = Position_AvrTv1_MIN = avrTv_4PositionMaxMin_1;
            Position_AvrTv2_MAX = Position_AvrTv2_MIN = avrTv_4PositionMaxMin_2;
            Position_AvrTv3_MAX = Position_AvrTv3_MIN = avrTv_4PositionMaxMin_3;
            Position_AvrVv1_MAX = Position_AvrVv1_MIN = avrVv_4PositionMaxMin_1;
            Position_AvrVv2_MAX = Position_AvrVv2_MIN = avrVv_4PositionMaxMin_2;
            Position_AvrVv3_MAX = Position_AvrVv3_MIN = avrVv_4PositionMaxMin_3;
        }
        private void updatePostPositionBlock()
        {
            Is_Position = false;
            Position_Price_MAX = Position_Price_MIN = 0;
            Position_AvrTv1_MAX = Position_AvrTv1_MIN = 0;
            Position_AvrTv2_MAX = Position_AvrTv2_MIN = 0;
            Position_AvrTv3_MAX = Position_AvrTv3_MIN = 0;
            Position_AvrVv1_MAX = Position_AvrVv1_MIN = 0;
            Position_AvrVv2_MAX = Position_AvrVv2_MIN = 0;
            Position_AvrVv3_MAX = Position_AvrVv3_MIN = 0;
        }

    }
}
