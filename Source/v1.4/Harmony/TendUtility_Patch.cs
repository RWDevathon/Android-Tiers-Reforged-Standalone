using Verse;
using HarmonyLib;
using RimWorld;
using System;
using UnityEngine;

namespace ATReforged
{
    internal class TendUtility_Patch

    {
        // Calculate mechanist tend quality (hijacking medical tend quality)
        [HarmonyPatch(typeof(TendUtility), "CalculateBaseTendQuality")]
        [HarmonyPatch(new Type[] { typeof(Pawn), typeof(Pawn), typeof(float), typeof(float) }, new ArgumentType[] { ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal, ArgumentType.Normal })]
        public class CalculateBaseTendQuality_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Pawn doctor, Pawn patient, float medicinePotency, float medicineQualityMax, ref float __result)
            {
                // Nothing to hijack if the patient isn't mechanical.
                if (!Utils.IsConsideredMechanical(patient))
                    return true;

                // Essentially vanilla TendQuality but using mechanical tend quality rather than medicinal.
                float tendQuality;
                if (doctor != null)
                {
                    tendQuality = doctor.GetStatValue(ATR_StatDefOf.ATR_MechanicalTendQuality, true);
                }
                else
                {
                    tendQuality = 0.75f;
                }
                tendQuality *= medicinePotency;
                Building_Bed building_Bed = patient?.CurrentBed();
                if (building_Bed != null)
                {
                    tendQuality += building_Bed.GetStatValue(ATR_StatDefOf.ATR_MechanicalTendQualityOffset, true);
                }
                if (doctor == patient && doctor != null)
                {
                    tendQuality *= 0.7f;
                }
                __result = Mathf.Clamp(tendQuality, 0f, medicineQualityMax);
                return false;
            }
        }
    }
}