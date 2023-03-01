using RimWorld;
using Verse;

namespace ATReforged
{
    public class Hediff_Fractal : HediffWithComps
    {
        public override void PostMake()
        {
            base.PostMake();
            // Initialize mutation ticks so that the fractal warping will mutate every 2 - 4 days.
            nextMutationTick = ageTicks + Rand.RangeInclusive(120000, 240000);
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref nextMutationTick, "ATR_nextMutationTick", 0);
        }

        private void ChangeState()
        {
            // Terminate if isTerminal
            if (Severity == 0.001f)
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
                    return;
                }
                Severity += 0.1f;
            }
            // Decrease mutation
            else
            {
                // True Transcendance threshold reached;
                if (Severity <= 0.1f)
                {
                    // Only humanlikes may fully transcend.
                    if (pawn.RaceProps.Humanlike)
                    {
                        DoTranscendance(pawn);
                    }
                    return;
                }
                severityInt -= .1f;
            }
            // Pick some time in the next 2 - 4 days for mutation.
            nextMutationTick = ageTicks + Rand.RangeInclusive(120000, 240000);
        }

        public override bool TryMergeWith(Hediff other)
        {
            if (other.def == ATR_HediffDefOf.ATR_FractalPillOrganic)
            {
                ChangeState();
                return true;
            }
            return false;
        }

        public override void PostTick()
        {
            base.PostTick();
            if (Severity == 0.001f)
            {
                return;
            }
            ageTicks++;

            if (ageTicks >= nextMutationTick)
            {
                ChangeState();
            }
        }

        public void DoMutation(Pawn pawn)
        {
            string label = "ATR_FractalCorruption".Translate();
            label = label.AdjustedFor(pawn);
            string text = "ATR_FractalCorruptionDesc".Translate(pawn.Name.ToStringShort);
            Find.LetterStack.ReceiveLetter(label, text, LetterDefOf.NegativeEvent, pawn);

            PawnGenerationRequest request = new PawnGenerationRequest(ATR_PawnKindDefOf.ATR_FractalAbomination, Faction.OfAncientsHostile, PawnGenerationContext.NonPlayer, fixedGender: Gender.None);
            Pawn abomination = PawnGenerator.GeneratePawn(request);
            FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_AmnioticFluid, pawn.LabelIndefinite(), 10);
            FilthMaker.TryMakeFilth(pawn.Position, pawn.Map, ThingDefOf.Filth_Blood, pawn.LabelIndefinite(), 10);

            GenSpawn.Spawn(abomination, pawn.Position, pawn.Map);
            abomination.mindState.mentalStateHandler.TryStartMentalState(ATR_MentalStateDefOf.ATR_MentalState_Exterminator, transitionSilently: true);

            pawn.Destroy();
        }

        public void DoTranscendance(Pawn pawn)
        {
            Severity = 0.001f;
        }

        int nextMutationTick;
    }
}
