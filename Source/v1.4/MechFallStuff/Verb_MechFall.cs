using Verse;

namespace ATReforged
{
    public class Verb_MechFall : Verb
    {
        protected override bool TryCastShot()
        {
            if (currentTarget.HasThing && currentTarget.Thing.Map != caster.Map)
            {
                return false;
            }
            MechFall mechfall = (MechFall)GenSpawn.Spawn(ThingDefOf.MechFallBeam, currentTarget.Cell, caster.Map);
            mechfall.duration = DurationTicks;
            mechfall.instigator = caster;
            mechfall.weaponDef = EquipmentSource?.def;
            mechfall.StartStrike();
            if (EquipmentSource != null && !EquipmentSource.Destroyed)
            {
                EquipmentSource.Destroy(DestroyMode.Vanish);
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
