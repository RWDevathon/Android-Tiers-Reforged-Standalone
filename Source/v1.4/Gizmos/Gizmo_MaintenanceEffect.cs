using RimWorld;
using UnityEngine;
using Verse;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    public class Gizmo_MaintenanceEffect : Gizmo
    {
        public CompMaintenanceNeed maintenanceNeed;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        public Gizmo_MaintenanceEffect()
        {
            Order = -95f;
        }

        public override float GetWidth(float maxWidth)
        {
            return 140f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect overRect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Find.WindowStack.ImmediateWindow(137486536, overRect, WindowLayer.GameUI, delegate
            {
                Rect firstRect = overRect.AtZero().ContractedBy(6f);
                firstRect.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(firstRect, "ATR_MaintenanceEffectGizmoLabel".Translate(maintenanceNeed.maintenanceEffectTicks < 0 ? "ATR_MaintenanceEffectGizmoLabelNegative".Translate() : "ATR_MaintenanceEffectGizmoLabelPositive".Translate(), Mathf.RoundToInt(Mathf.Abs(maintenanceNeed.maintenanceEffectTicks / 60000))));
                Rect secondRect = overRect.AtZero().ContractedBy(6f);
                secondRect.yMin = overRect.height / 2f;
                float fillPercent = Mathf.Clamp(Mathf.Abs(maintenanceNeed.maintenanceEffectTicks / 60000) / 15f, 0, 1);
                Widgets.FillableBar(secondRect, fillPercent, FullBarTex, EmptyBarTex, doBorder: true);
                TooltipHandler.TipRegion(secondRect, () => "ATR_MaintenanceEffectDesc".Translate(), Gen.HashCombineInt(maintenanceNeed.GetHashCode(), 283641));
            });
            return new GizmoResult(GizmoState.Clear);
        }
    }
}