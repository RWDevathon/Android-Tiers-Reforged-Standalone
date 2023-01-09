using RimWorld;
using UnityEngine;
using Verse;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    public class Gizmo_MaintenanceStatus : Gizmo
    {
        public CompMaintenanceNeed maintenanceNeed;

        private static readonly Texture2D FullBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.34f, 0.42f, 0.43f));

        private static readonly Texture2D FullHighlightTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.43f, 0.54f, 0.55f));

        private static readonly Texture2D EmptyBarTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.03f, 0.035f, 0.05f));

        private static readonly Texture2D TargetLevelTex = SolidColorMaterials.NewSolidColorTexture(new Color(0.74f, 0.97f, 0.8f));

        private static bool draggingBar;

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
            Find.WindowStack.ImmediateWindow(251714293, overRect, WindowLayer.GameUI, delegate
            {
                Rect firstRect = overRect.AtZero().ContractedBy(6f);
                firstRect.height = overRect.height / 2f;
                Text.Font = GameFont.Tiny;
                Widgets.Label(firstRect, "ATR_MaintenanceGizmoLabel".Translate());
                Rect secondRect = overRect.AtZero().ContractedBy(6f);
                secondRect.yMin = overRect.height / 2f;
                float fillPercent = maintenanceNeed.MaintenanceLevel;
                if (Find.Selector.SingleSelectedThing is Pawn pawn && pawn.IsColonistPlayerControlled)
                {
                    Widgets.DraggableBar(secondRect, FullBarTex, FullHighlightTex, EmptyBarTex, TargetLevelTex, ref draggingBar, fillPercent, ref maintenanceNeed.targetLevel, CompMaintenanceNeed.MaintenanceThresholdBandPercentages);
                }
                else
                {
                    Widgets.FillableBar(secondRect, fillPercent, FullBarTex, EmptyBarTex, doBorder: true);
                }
                TooltipHandler.TipRegion(secondRect, () => maintenanceNeed.MaintenanceTipString(), Gen.HashCombineInt(maintenanceNeed.GetHashCode(), 171495));
            });
            return new GizmoResult(GizmoState.Clear);
        }
    }
}