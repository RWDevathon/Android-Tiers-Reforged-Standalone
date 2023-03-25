using RimWorld;
using System.Collections.Generic;
using UnityEngine;
using Verse;
using Verse.Sound;
using AlienRace;

namespace ATReforged
{
    public class Dialog_Repaint : Window
    {
        public static Vector2 scrollPosition = Vector2.zero;

        private Pawn pawn;

        private List<Color> colors;

        private Color initialColor;

        private Color targetColor;

        private static readonly Vector2 ButSize = new Vector2(200f, 40f);

        private static readonly Vector3 PortraitOffset = new Vector3(0f, 0f, 0.15f);

        private float colorsHeight;

        private List<Color> Colors
        {
            get
            {
                if (colors != null)
                {
                    return colors;
                }
                else
                {
                    colors = new List<Color>();
                    if (pawn.story?.favoriteColor.HasValue == true)
                    {
                        colors.Add(pawn.story.favoriteColor.Value);
                    }
                    if (pawn.Ideo != null && !Find.IdeoManager.classicMode)
                    {
                        colors.Add(pawn.Ideo.ApparelColor);
                    }
                    foreach (ColorDef colorDef in DefDatabase<ColorDef>.AllDefs)
                    {
                        if (colorDef.colorType == ColorType.Misc || colorDef.colorType == ColorType.Ideo)
                        {
                            colors.Add(colorDef.color);
                        }
                    }
                    colors.SortByColor((Color color) => color);
                    return colors;
                }
            }
        }

        public Dialog_Repaint(Pawn target)
        {
            forcePause = true;
            pawn = target;
            initialColor = pawn.GetComp<AlienPartGenerator.AlienComp>().ColorChannels["skin"].first;
            targetColor = initialColor;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Text.Font = GameFont.Medium;
            Rect TitleRect = new Rect(inRect);
            TitleRect.height = Text.LineHeight * 2f;
            Widgets.Label(TitleRect, "StylePawn".Translate().CapitalizeFirst() + ": " + Find.ActiveLanguageWorker.WithDefiniteArticle(pawn.Name.ToStringShort, pawn.gender, plural: false, name: true).ApplyTag(TagType.Name));
            Text.Font = GameFont.Small;
            inRect.yMin = TitleRect.yMax + 4f;
            Rect PortraitRect = inRect;
            PortraitRect.width *= 0.3f;
            PortraitRect.yMax -= ButSize.y + 4f;
            DrawPawn(PortraitRect);
            Rect ColorsRect = inRect;
            ColorsRect.xMin = PortraitRect.xMax + 10f;
            ColorsRect.yMax -= ButSize.y + 4f;
            DrawSkinColors(ColorsRect);
            DrawBottomButtons(inRect);
        }

        private void DrawPawn(Rect rect)
        {
            Rect rect2 = rect;
            rect2.yMin = rect.yMax - Text.LineHeight * 2f;
            rect.yMax = rect2.yMin - 4f;
            Widgets.BeginGroup(rect);
            for (int i = 0; i < 3; i++)
            {
                Rect position = new Rect(0f, rect.height / 3f * i, rect.width, rect.height / 3f).ContractedBy(4f);
                RenderTexture image = PortraitsCache.Get(pawn, new Vector2(position.width, position.height), new Rot4(2 - i), PortraitOffset, 1.1f, supersample: true, compensateForUIScale: true, false, false, stylingStation: true);
                GUI.DrawTexture(position, image);
            }
            Widgets.EndGroup();
        }

        private void DrawSkinColors(Rect rect)
        {
            Widgets.ColorSelector(new Rect(rect.x, rect.y, rect.width, colorsHeight), ref targetColor, Colors, out colorsHeight);
            if (targetColor != pawn.GetComp<AlienPartGenerator.AlienComp>().ColorChannels["skin"].first)
            {
                pawn.GetComp<AlienPartGenerator.AlienComp>()?.OverwriteColorChannel("skin", targetColor);
                pawn.story.SkinColorBase = targetColor;
                pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
                PortraitsCache.SetDirty(pawn);
            }
            colorsHeight += Text.LineHeight * 2f;
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
            pawn.GetComp<AlienPartGenerator.AlienComp>()?.OverwriteColorChannel("skin", initialColor);
            pawn.story.SkinColorBase = initialColor;
            targetColor = initialColor;
            pawn.Drawer.renderer.graphics.SetAllGraphicsDirty();
            PortraitsCache.SetDirty(pawn);
        }
    }
}