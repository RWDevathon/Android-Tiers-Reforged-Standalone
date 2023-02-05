using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ATReforged
{
    // This comp adds a gizmo to its parent allowing it to be restrictable to certain pawn types (drones, androids, organics, combinations). Certain harmony patches can check against this.
    public class CompPawnTypeRestrictable : ThingComp
    {
        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (!respawningAfterLoad)
            {
                ResetToDefault();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref assignedToType, "ATR_assignedToType", PawnType.All);
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            yield return new Command_Action
            {
                icon = Tex.RestrictionGizmoIcon,
                defaultLabel = "ATR_RestrictPawnType".Translate(),
                defaultDesc = "ATR_RestrictPawnTypeDescription".Translate(),
                action = delegate ()
                {
                    Find.WindowStack.Add(new Dialog_RestrictToPawnType());
                }
            };
        }

        // Switch to the restricted type. If it did not already have this type, ensure no restrictions are being violated after the change.
        public void SwitchToType(PawnType newType)
        {
            if (assignedToType == newType)
            {
                return;
            }

            assignedToType = newType;
            // Bed ownership can be restricted to pawn types, switching to a new type may force some pawns to lose their bed ownership.
            if (parent is Building_Bed bed)
            {
                List<Pawn> bedOwnersForRemoval = new List<Pawn>();
                foreach (Pawn bedOwner in bed.OwnersForReading)
                {
                    if ((Utils.GetPawnType(bedOwner) | assignedToType) != assignedToType)
                    {
                        bedOwnersForRemoval.Add(bedOwner);
                    }
                }
                foreach (Pawn bedOwner in bedOwnersForRemoval)
                {
                    bedOwner.ownership.UnclaimBed();
                    Messages.Message("MessageBedLostAssignment".Translate(bed.def, bedOwner), new LookTargets(bed, bedOwner), MessageTypeDefOf.CautionInput, historical: false);
                }
            }
        }

        public void ResetToDefault()
        {
            if (parent is Building_Bed)
            {
                if (ATReforged_Settings.bedRestrictionDefaultsToAll)
                {
                    assignedToType = PawnType.All;
                }
                else
                {
                    assignedToType = parent is Building_ChargingBed ? PawnType.Mechanical : PawnType.Organic;
                }
            }
            else
            {
                assignedToType = PawnType.All;
            }
        }

        public PawnType assignedToType;
    }
}