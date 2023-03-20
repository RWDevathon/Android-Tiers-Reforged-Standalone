using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ATReforged
{
    public class ChoiceLetter_PersonalityShift : ChoiceLetter_GrowthMoment
    {
        protected bool isReplacingTrait;

        protected bool isReplacingPassions;

        public void ConfigureChoiceLetter(Pawn pawn, int traitChoiceCount, int passionGainsCount, bool isReplacingTrait, bool isReplacingPassions)
        {
            this.pawn = pawn;
            this.traitChoiceCount = traitChoiceCount;
            this.passionGainsCount = passionGainsCount;
            this.isReplacingTrait = isReplacingTrait;
            this.isReplacingPassions = isReplacingPassions;
            growthTier = -1;
            CacheLetterText();
        }

        public override void OpenLetter()
        {
            TrySetChoices();
            Dialog_PersonalityShiftChoices window = new Dialog_PersonalityShiftChoices(text, this);
            Find.WindowStack.Add(window);
        }

        virtual protected void TrySetChoices()
        {
            if (choiceMade || pawn.DestroyedOrNull())
            {
                return;
            }
            if (passionChoices == null)
            {
                passionChoices = PassionOptions();
            }
            if (traitChoiceCount > 0 && traitChoices == null)
            {
                traitChoices = PawnGenerator.GenerateTraitsFor(pawn, traitChoiceCount, null, growthMomentTrait: true);
            }
        }

        virtual public void ConfirmChoices(List<SkillDef> skills, Trait trait)
        {
            // If this is flagged as replacing passions, then replace passions if the pawn has more than 8. If the pawn has less, do not replace until equal or greater than 8.
            if (isReplacingPassions)
            {
                Pawn_SkillTracker skillTracker = pawn.skills;
                List<SkillRecord> potentialSkillsToReplace = skillTracker.skills.Where(record => !skills.Contains(record.def) && record.passion != Passion.None).ToList();
                int passionsAdded = skills.Count;
                int pawnPassionCountToReplace = passionsAdded - Mathf.Clamp(8 - skillTracker.PassionCount, 0, passionsAdded);
                for (int i = pawnPassionCountToReplace; i > 0; i--)
                {
                    if (potentialSkillsToReplace.Count == 0)
                    {
                        break;
                    }

                    SkillRecord replacedSkill = potentialSkillsToReplace.RandomElement();
                    replacedSkill.passion = (Passion)((int)replacedSkill.passion - 1);
                    potentialSkillsToReplace.Remove(replacedSkill);
                }
            }
            // If this is flagged as replacing traits, then replace traits if the pawn has at least one. Allow the addition of a new trait if there are none to replace.
            if (isReplacingPassions)
            {
                List<Trait> potentialTraitsToReplace = pawn.story.traits.allTraits.Where(potentialTrait => potentialTrait.sourceGene == null).ToList();
                if (potentialTraitsToReplace.Count > 0)
                {
                    pawn.story.traits.RemoveTrait(potentialTraitsToReplace.RandomElement());
                }
            }
            MakeChoices(skills, trait);
        }

        virtual protected void CacheLetterText()
        {
            text = "ATR_PersonalityShift".Translate(pawn);
            mouseoverText = text;
            if (traitChoiceCount > 0 || passionGainsCount > 0)
            {
                mouseoverText += "\n\n" + "ATR_PersonalityShiftChooseHowPawnWillChange".Translate(pawn);
            }
        }

        virtual protected List<SkillDef> PassionOptions()
        {
            return DefDatabase<SkillDef>.AllDefsListForReading.Where((SkillDef s) => IsValidPassionOption(s)).ToList();
        }

        virtual protected bool IsValidPassionOption(SkillDef skill)
        {
            SkillRecord pawnRecord = pawn.skills.GetSkill(skill);
            return !pawnRecord.PermanentlyDisabled && pawnRecord.passion != Passion.Major;
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look(ref isReplacingTrait, "ATR_isReplacingTrait", false);
            Scribe_Values.Look(ref isReplacingPassions, "ATR_isReplacingPassions", false);
        }
    }
}