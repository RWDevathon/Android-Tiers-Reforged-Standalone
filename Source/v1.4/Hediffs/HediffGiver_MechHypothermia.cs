using UnityEngine;
using Verse;

namespace ATReforged
{
    // Replicate HediffGiver_Hypothermia but do not apply frostbite effects.
    public class HediffGiver_HypothermiaNoFrostbite : HediffGiver_Hypothermia
    {
        public override void OnIntervalPassed(Pawn pawn, Hediff cause)
        {
            if (pawn.Dead)
            {
                return;
            }

            float ambientTemperature = pawn.AmbientTemperature;
            float safeRangeMin = pawn.SafeTemperatureRange().min;
            if (ambientTemperature < safeRangeMin)
            {
                float severity = Mathf.Abs(ambientTemperature - safeRangeMin) * 6.45E-05f;
                severity = Mathf.Max(severity, 0.00075f);
                HealthUtility.AdjustSeverity(pawn, hediff, severity);
            }
            else if (ambientTemperature > pawn.ComfortableTemperatureRange().min)
            {
                HealthUtility.AdjustSeverity(pawn, hediff, -0.0075f);
            }
        }
    }
}
