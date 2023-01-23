using RimWorld;
using System;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ATReforged
{
    public class Dialog_RestrictToPawnType : Window
    {
        public static Vector2 scrollPosition = Vector2.zero;

        private List<CompPawnTypeRestrictable> compRestricts;

        private PawnType pawnTypes;

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);

        private static readonly List<Texture2D> exemplarImages = new List<Texture2D> {Tex.TierOneExemplar, Tex.TierTwoExemplar, Tex.BasicHumanExemplar, Tex.DronePawnTypeRestricted, Tex.AndroidPawnTypeRestricted, Tex.OrganicPawnTypeRestricted};

        public Dialog_RestrictToPawnType()
        {
            forcePause = true;
            compRestricts = new List<CompPawnTypeRestrictable>();
            foreach (object selectedObject in Find.Selector.SelectedObjectsListForReading)
            {
                if (selectedObject is ThingWithComps selectedThing)
                {
                    CompPawnTypeRestrictable compPawnTypeRestrictable = selectedThing.GetComp<CompPawnTypeRestrictable>();
                    if (compPawnTypeRestrictable != null)
                    {
                        compRestricts.Add(compPawnTypeRestrictable);
                    }
                }
            }
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect TitleRect = new Rect(inRect);
            TitleRect.height = Text.LineHeight * 2f;
            Widgets.Label(TitleRect, "ATR_RestrictedPawnTypes".Translate());
            Text.Font = GameFont.Small;
            inRect.yMin = TitleRect.yMax + 4f;
            Rect exemplarRect = inRect;
            exemplarRect.width *= 0.3f;
            exemplarRect.yMax -= ButSize.y + 4f;
            pawnTypes = PawnType.None;
            for (int i = compRestricts.Count - 1; i >= 0; i--)
            {
                pawnTypes |= compRestricts[i].assignedToType;
            }
            DrawExemplars(exemplarRect);
            Rect optionsRect = inRect;
            optionsRect.xMin = exemplarRect.xMax + 10f;
            optionsRect.yMax -= ButSize.y + 4f;
            DrawOptions(optionsRect);
            DrawBottomButtons(inRect);
        }

        private void DrawExemplars(Rect rect)
        {
            Rect rect2 = rect;
            rect2.yMin = rect.yMax - Text.LineHeight * 2f;
            rect.yMax = rect2.yMin - 4f;
            Widgets.BeginGroup(rect);
            Rect position = new Rect(0f, 0, rect.width, rect.height / 3f).ContractedBy(4f);
            GUI.DrawTexture(position, exemplarImages[(pawnTypes & PawnType.Drone) == PawnType.Drone ? 0 : 3]);
            position = new Rect(0f, rect.height / 3f, rect.width, rect.height / 3f).ContractedBy(4f);
            GUI.DrawTexture(position, exemplarImages[(pawnTypes & PawnType.Android) == PawnType.Android ? 1 : 4]);
            position = new Rect(0f, rect.height * 2f / 3f, rect.width, rect.height / 3f).ContractedBy(4f);
            GUI.DrawTexture(position, exemplarImages[(pawnTypes & PawnType.Organic) == PawnType.Organic ? 2 : 5]);
            Widgets.EndGroup();
        }

        private void DrawOptions(Rect rect)
        {

            Listing_Standard listingStandard = new Listing_Standard
            {
                maxOneColumn = true
            };
            listingStandard.Begin(rect);
            if (listingStandard.RadioButton("ATR_PawnTypeNone".Translate(), (PawnType.None | pawnTypes) == PawnType.None, tooltip: "ATR_PawnTypeNoneTooltip".Translate(), tooltipDelay: 0.25f))
            {
                for (int j = compRestricts.Count - 1; j >= 0; j--)
                {
                    compRestricts[j].SwitchToType(PawnType.None);
                }
            }
            for (int i = 1; i < 8; i++)
            {
                if (listingStandard.RadioButton($"ATR_PawnType{(PawnType)i}".Translate(), ((PawnType)i & pawnTypes) == (PawnType)i, tooltip: $"ATR_PawnType{(PawnType)i}Tooltip".Translate(), tooltipDelay: 0.25f))
                {
                    for (int j = compRestricts.Count - 1; j >= 0; j--)
                    {
                        compRestricts[j].SwitchToType((PawnType)i);
                    }
                }
            }
            listingStandard.End();
        }

        private void DrawBottomButtons(Rect inRect)
        {
            if (Widgets.ButtonText(new Rect(inRect.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Reset".Translate()))
            {
                Reset();
                SoundDefOf.Tick_Low.PlayOneShotOnCamera();
            }
            if (Widgets.ButtonText(new Rect(inRect.xMax - ButSize.x, inRect.yMax - ButSize.y, ButSize.x, ButSize.y), "Accept".Translate()))
            {
                Close();
            }
        }

        private void Reset()
        {
            foreach (var compRestrict in compRestricts)
            {
                compRestrict.ResetToDefault();
            }
        }
    }
}