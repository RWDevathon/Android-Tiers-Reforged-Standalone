using RimWorld;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace ATReforged
{
    static class SettingsUIExtensions
    {
        public const float IconSize = 32f;
        public const float IconGap = 1f;
        public static readonly Color iconBaseColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        public static readonly Color iconMouseOverColor = new Color(0.6f, 0.6f, 0.4f, 1f);
        public const float PawnListSize = (IconGap + IconSize) * 5;

        public static void PawnSelector(this Listing_Standard instance, IEnumerable<ThingDef> pawnOptions, HashSet<ThingDef> selectedPawns, string selectedLabel, string unselectedLabel, Action onChange = null)
        {
            var unselectedPawns = pawnOptions.Where(w => !selectedPawns.Contains(w));

            TextAnchor anchorSave = Text.Anchor;
            Color colorSave = GUI.color;
            GUI.color = Color.white;

            var width = instance.ColumnWidth;
            var fullRect = instance.GetRect(0);
            Rect leftRect = fullRect.LeftHalf();
            Rect rightRect = fullRect.RightHalf();

            float selectedLabelHeight = Text.CalcHeight(selectedLabel, leftRect.width);
            float unselectedLabelHeight = Text.CalcHeight(unselectedLabel, rightRect.width);

            leftRect.height = rightRect.height = Mathf.Max(selectedLabelHeight, unselectedLabelHeight);

            Text.Anchor = TextAnchor.LowerCenter;
            Widgets.Label(leftRect, $"{selectedLabel}");
            Widgets.Label(rightRect, $"{unselectedLabel}");
            instance.GetRect(leftRect.height);

            leftRect.y += leftRect.height;
            rightRect.y += rightRect.height;

            int iconsPerLeftRow = (int)(leftRect.width / (IconGap + IconSize));
            int leftRows = (selectedPawns.Count() / iconsPerLeftRow) + 1;
            int iconsPerRightRow = (int)(rightRect.width / (IconGap + IconSize));
            int rightRows = (unselectedPawns.Count() / iconsPerRightRow) + 1;

            leftRect.height = ((leftRows * (IconSize + IconGap)) - IconGap);
            rightRect.height = ((rightRows * (IconSize + IconGap)) - IconGap);

            instance.GetRect((Mathf.Max(leftRows, rightRows) * (IconSize + IconGap)) - IconGap);

            var orderedUnselectedPawns = unselectedPawns.ToList().OrderBy(w => w.label).ToList();
            var orderedSelectedPawns = selectedPawns.ToList().OrderBy(w => w.label).ToList();

            for (int i = 0; i < orderedSelectedPawns.Count(); i++)
            {
                int collum = (i % iconsPerLeftRow);
                int row = (i / iconsPerLeftRow);
                bool interacted = DrawIconForPawn(orderedSelectedPawns[i], leftRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap));
                if (interacted)
                {
                    selectedPawns.Remove(orderedSelectedPawns[i]);
                    onChange?.Invoke();
                }
            }

            for (int i = 0; i < orderedUnselectedPawns.Count; i++)
            {
                int collum = (i % iconsPerRightRow);
                int row = (i / iconsPerRightRow);
                bool interacted = DrawIconForPawn(orderedUnselectedPawns[i], rightRect, new Vector2(IconSize * collum + collum * IconGap, IconSize * row + row * IconGap));
                if (interacted)
                {
                    selectedPawns.Add(orderedUnselectedPawns[i]);
                    onChange?.Invoke();
                }
            }
        }
        public static bool DrawIconForPawn(ThingDef pawnDef, Rect contentRect, Vector2 iconOffset, bool isBackground = false)
        {
            if (pawnDef == null)
            {
                Log.Warning("Tried to draw an icon for a null pawn!");
                var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);
                GUI.color = Color.white;
                GUI.DrawTexture(iconRect, Tex.SkillWorkshopHeader);
                if (!isBackground)
                    return Widgets.ButtonInvisible(iconRect, true);
                else
                    return false;
            } 
            else
            {
                Graphic g = pawnDef.graphic;
                Color color = pawnDef.graphic.Color;
                Color colorTwo = pawnDef.graphic.ColorTwo;
                Graphic g2 = pawnDef.graphicData.Graphic.GetColoredVersion(g.Shader, color, colorTwo);

                var iconRect = new Rect(contentRect.x + iconOffset.x, contentRect.y + iconOffset.y, IconSize, IconSize);

                if (!isBackground)
                {
                    string label = pawnDef.label;

                    TooltipHandler.TipRegion(iconRect, label);
                    MouseoverSounds.DoRegion(iconRect, SoundDefOf.Mouseover_Command);
                    if (Mouse.IsOver(iconRect))
                    {
                        GUI.color = iconMouseOverColor;
                        GUI.DrawTexture(iconRect, Tex.DrawPocket);
                    }
                    else
                    {
                        GUI.color = iconBaseColor;
                        GUI.DrawTexture(iconRect, Tex.DrawPocket);
                        GUI.DrawTextureWithTexCoords(iconRect, Tex.DrawPocket, new Rect(0, 0, 1, 1));
                    }
                }

                Texture resolvedIcon;
                if (!pawnDef.uiIconPath.NullOrEmpty())
                {
                    resolvedIcon = pawnDef.uiIcon;
                }
                else
                {
                    resolvedIcon = g2.MatSingle.mainTexture;
                }
                GUI.color = color;
                GUI.DrawTexture(iconRect, resolvedIcon);
                GUI.color = Color.white;

                if (!isBackground)
                {
                    if (Widgets.ButtonInvisible(iconRect, true))
                    {
                        return true;
                    }
                }
                return false;
            }
        }
    }
}
