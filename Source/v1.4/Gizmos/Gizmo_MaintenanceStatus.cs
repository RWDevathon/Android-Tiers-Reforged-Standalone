using RimWorld;
using System;
using UnityEngine;
using Verse;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    public class Gizmo_MaintenanceStatus : Gizmo
    {
        public CompMaintenanceNeed maintenanceNeed;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.35f, 0.35f, 0.2f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(Color.black);

        private static readonly Texture2D TargetLevelArrow = ContentFinder<Texture2D>.Get("UI/Misc/BarInstantMarkerRotated");

        private const float ArrowScale = 0.5f;

        public Gizmo_MaintenanceStatus()
        {
            Order = -100f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(243114243, overRect, WindowLayer.GameUI, delegate
            {
                Rect firstRect = overRect.AtZero().ContractedBy(6f);
                firstRect.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(firstRect, "ATR_MaintenanceGizmoLabel".Translate());
                Rect secondRect = overRect.AtZero().ContractedBy(6f);
                secondRect.yMin = overRect.height / 2f;
                float fillPercent = maintenanceNeed.MaintenanceLevel;
                Widgets.FillableBar(secondRect, fillPercent, FullBarTex, EmptyBarTex, doBorder: false);

                float x = secondRect.x + maintenanceNeed.TargetMaintenanceLevel * secondRect.width - TargetLevelArrow.width * ArrowScale / 2f;
                float y = secondRect.y - TargetLevelArrow.height * ArrowScale;
                GUI.DrawTexture(new Rect(x, y, TargetLevelArrow.width * ArrowScale, TargetLevelArrow.height * ArrowScale), TargetLevelArrow);

                Text.Font = GameFont.Small;
                Text.Anchor = TextAnchor.MiddleCenter;
                Widgets.Label(secondRect, (int)(maintenanceNeed.MaintenanceLevel * 100) + "%");
                Text.Anchor = TextAnchor.UpperLeft;
            });
            return new GizmoResult(GizmoState.Clear);
        }
    }
}