using System;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class Verb_MechFall : Verb
    {
        protected override bool TryCastShot()
        {
            if (this.currentTarget.HasThing && this.currentTarget.Thing.Map != this.caster.Map)
            {
                return false;
            }
            MechFall mechfall = (MechFall)GenSpawn.Spawn(ThingDefOf.MechFallBeam, this.currentTarget.Cell, this.caster.Map);
            mechfall.duration = 450;
            mechfall.instigator = this.caster;
            mechfall.weaponDef = ((base.EquipmentSource == null) ? null : base.EquipmentSource.def);
            mechfall.StartStrike();
            if (base.EquipmentSource != null && !base.EquipmentSource.Destroyed)
            {
                base.EquipmentSource.Destroy(DestroyMode.Vanish);
            }
            return true;
        }
        public override float HighlightFieldRadiusAroundTarget(out bool needLOSToCenter)
        {
            needLOSToCenter = false;
            return 2f;
        }

        private const int DurationTicks = 450;
    }
}
