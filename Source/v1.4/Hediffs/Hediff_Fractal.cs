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

            Scribe_Values.Look(ref isTerminal, "ATR_FractalTerminalBool", false);
            Scribe_Values.Look(ref nextMutationTick, "ATR_nextMutationTick", 0);
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
                { // True Transcendance threshold reached;
                    isTerminal = true;
                    Severity = 0.01f;
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

        public static void DoMutation(Pawn pawn)
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
        }

        bool isTerminal;
        int nextMutationTick;
    }
}
