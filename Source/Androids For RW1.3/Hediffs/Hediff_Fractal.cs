using RimWorld;
using Verse;

namespace ATReforged
{
    public class Hediff_Fractal : HediffWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
            // Initialize mutation ticks so that the fractal warping will mutate every 10 - 20 days.
            nextMutationTick = ageTicks + Rand.RangeInclusive(600000, 1200000);
        }

        private void ChangeState()
        {
            // Terminate if isTerminal
            if (isTerminal)
            {
                nextMutationTick = int.MaxValue;
                return;
            }

            // Increase mutation
            if (Rand.Chance(0.8f))
            { 
                if (Severity >= 0.45f)
                { // Abomination threshold reached;
                    DoMutation(pawn);
                    pawn.Destroy();
                    return;
                }
                Severity += 0.1f;
            }
            // Decrease mutation
            else
            { 
                if (Severity <= 0.1f)
                { // True Transcendance threshold reached; TODO: replace fractal transcendance with secret sauce
                    isTerminal = true;
                    Severity = 0.01f;
                    return;
                }
                severityInt -= .1f;
            }
            // Pick some time in the next 10 - 20 days for mutation.
            nextMutationTick = ageTicks + Rand.RangeInclusive(600000, 1200000);
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (other.def == HediffDefOf.FractalPillOrganic)
            {
                ChangeState();
                return true;
            }
            return false;
        }

        public override void PostTick()
        {
            base.PostTick();
            ageTicks++;
            if (isTerminal)
            {
                return;
            }

            if (ageTicks >= nextMutationTick)
            {
                ChangeState();
            }
        }

        public static void DoMutation(Pawn premutant)
        {
            string label = "ATR_FractalCorruption".Translate();
            label = label.AdjustedFor(premutant);
            string text = "ATR_FractalCorruptionDesc".Translate(premutant.Name.ToStringShort);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, premutant);

            PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.AbominationAtlas, Faction.OfAncientsHostile, PawnGenerationContext.NonPlayer, fixedGender: Gender.None);
            Pawn pawn = PawnGenerator.GeneratePawn(request);
            FilthMaker.TryMakeFilth(premutant.Position, premutant.Map, RimWorld.ThingDefOf.Filth_AmnioticFluid, premutant.LabelIndefinite(), 10);
            FilthMaker.TryMakeFilth(premutant.Position, premutant.Map, RimWorld.ThingDefOf.Filth_Blood, premutant.LabelIndefinite(), 10);

            GenSpawn.Spawn(pawn, premutant.Position, premutant.Map);
            pawn.mindState.mentalStateHandler.TryStartMentalState(MentalStateDefOf.ATR_MentalState_Exterminator, transitionSilently: true);
        }

        bool isTerminal;
        int nextMutationTick;
    }
}
