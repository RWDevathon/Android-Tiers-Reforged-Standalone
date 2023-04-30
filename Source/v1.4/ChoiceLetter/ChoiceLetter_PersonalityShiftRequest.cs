using Verse;
using RimWorld;
using System.Collections.Generic;

namespace ATReforged
{
    public class ChoiceLetter_PersonalityShiftRequest : ChoiceLetter
    {
        public Pawn subject;

        public override bool CanDismissWithRightClick => false;

        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                    yield break;
                }
                DiaOption diaOption = new DiaOption("AcceptButton".Translate());
                DiaOption optionReject = new DiaOption("RejectLetter".Translate());
                diaOption.action = delegate
                {
                    ChoiceLetter_PersonalityShift choiceLetter = (ChoiceLetter_PersonalityShift)LetterMaker.MakeLetter(ATR_LetterDefOf.ATR_PersonalityShiftLetter);
                    choiceLetter.ConfigureChoiceLetter(subject, 3, 3, true, true);
                    choiceLetter.Label = "ATR_PersonalityShiftRequest".Translate(subject);
                    choiceLetter.StartTimeout(2500);
                    Find.LetterStack.ReceiveLetter(choiceLetter);
                    Find.LetterStack.RemoveLetter(this);
                    subject.needs.mood?.thoughts?.memories?.TryGainMemoryFast(ATR_ThoughtDefOf.ATR_PersonalityShiftAllowed);
                };
                diaOption.resolveTree = true;
                optionReject.action = delegate
                {
                    Find.LetterStack.RemoveLetter(this);
                    subject.needs.mood?.thoughts?.memories?.TryGainMemoryFast(ATR_ThoughtDefOf.ATR_PersonalityShiftDenied);
                };
                optionReject.resolveTree = true;
                yield return diaOption;
                yield return optionReject;
                if (lookTargets.IsValid())
                {
                    yield return Option_JumpToLocationAndPostpone;
                }
                yield return Option_Postpone;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_References.Look(ref subject, "ATR_shiftSubject");
        }
    }
}