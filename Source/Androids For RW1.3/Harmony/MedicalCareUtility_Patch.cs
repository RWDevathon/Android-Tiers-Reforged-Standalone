using Verse;
using Verse.AI;
using Verse.AI.Group;
using HarmonyLib;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Verse.Sound;

namespace ATReforged
{
    internal class MedicalCareUtility_Patch
    {
        [HarmonyPatch(typeof(MedicalCareUtility), "MedicalCareSelectButton")]
        public class MedicalCareSelectButton_Patch
        {
            [HarmonyPrefix]
            public static bool Listener(Rect rect, Pawn pawn)
            {
                try
                {
                    if (Utils.IsConsideredMechanical(pawn))
                    {
                        Func<Pawn, MedicalCareCategory> getPayload = new Func<Pawn, MedicalCareCategory>(MedicalCareSelectButton_GetMedicalCare);
                        Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>> menuGenerator = new Func<Pawn, IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>>>(MedicalCareSelectButton_GenerateMenu);
                        Texture2D buttonIcon;
                        int index = (int)pawn.playerSettings.medCare;
                        if (index == 0)
                            buttonIcon = Tex.NoCare;
                        else if (index == 1)
                            buttonIcon = Tex.NoMed;
                        else if (index == 2)
                            buttonIcon = Tex.RepairStimSimple;
                        else if (index == 3)
                            buttonIcon = Tex.RepairStimIntermediate;
                        else
                            buttonIcon = Tex.RepairStimAdvanced;

                        Widgets.Dropdown(rect, pawn, getPayload, menuGenerator, null, buttonIcon, null, null, null, true);
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }
                catch(Exception e)
                {
                    Log.Message("[ATPP] MedicalCareUtility.MedicalCareSelectButton : " + e.Message + " - " + e.StackTrace);
                    return true;
                }
            }

            private static MedicalCareCategory MedicalCareSelectButton_GetMedicalCare(Pawn pawn)
            {
                return pawn.playerSettings.medCare;
            }

            private static IEnumerable<Widgets.DropdownMenuElement<MedicalCareCategory>> MedicalCareSelectButton_GenerateMenu(Pawn p)
            {
                for (int i = 0; i < 5; i++)
                {
                    MedicalCareCategory mc = (MedicalCareCategory)i;
                    yield return new Widgets.DropdownMenuElement<MedicalCareCategory>
                    {
                        option = new FloatMenuOption(mc.GetLabel(), delegate
                        {
                            p.playerSettings.medCare = mc;
                        }, MenuOptionPriority.Default, null, null, 0f, null, null),
                        payload = mc
                    };
                }
            }
        }
    }




}