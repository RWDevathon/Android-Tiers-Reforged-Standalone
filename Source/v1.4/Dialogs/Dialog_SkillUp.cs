using Verse;
using RimWorld;
using UnityEngine;
using System.Collections.Generic;
using static ATReforged.ListingExtensions;

namespace ATReforged
{
    public class Dialog_SkillUp : Window
    {
        Pawn pawn;
        List<string> skillDefTranslationList;
        List<SkillDef> skillDefList;
        List<int> skillDefPointList;
        List<int> skillDefPassionList;
        
        int curSumPassions = 0;

        public static Vector2 scrollPosition = Vector2.zero;

        public override Vector2 InitialSize
        {
            get
            {
                return new Vector2(580f, 725f);
            }
        }

        public Dialog_SkillUp(Pawn pawn, int freeSkills = 0, int freePassions = 0)
        {
            this.pawn = pawn;
            forcePause = true;
            doCloseX = true;
            absorbInputAroundWindow = true;
            closeOnAccept = false;
            closeOnClickedOutside = true;

            skillDefTranslationList = new List<string> { };
            skillDefList = new List<SkillDef>();
            skillDefPointList = new List<int>();
            skillDefPassionList = new List<int>();

            // Generate a list of all skill defs dynamically from all loaded skill defs (mods included!) with translations, def, the pawn's value and passions.
            foreach (SkillDef def in DefDatabase<SkillDef>.AllDefsListForReading)
            {
                skillDefTranslationList.Add(def.defName);
                skillDefList.Add(def);
                SkillRecord skillRecord = pawn.skills.GetSkill(def);

                if (skillRecord != null && !skillRecord.TotallyDisabled)
                { // Has stats in this skill, add point level and sum passion level and add it to the appropriate list.
                    skillDefPointList.Add(skillRecord.Level);
                    if ((int)skillRecord.passion <= 2)
                    { // This is a simple check for vanilla passion values. 0 = none, 1 = minor, 2 = major.
                        curSumPassions += (int)skillRecord.passion;
                    }
                    else
                    { // If it's outside this range, treat the non-vanilla passion as a minor passion.
                        curSumPassions += 1;
                    }
                    skillDefPassionList.Add((int)skillRecord.passion);
                }
                else
                { // Add a -1 to the list to represent a disabled skill, no passion possible. 
                    skillDefPointList.Add(-1);
                    skillDefPassionList.Add(-1);
                }
            }
        }

        float cachedScrollHeight = 0;
        public override void DoWindowContents(Rect inRect)
        {
            Color colorSave = GUI.color;
            TextAnchor anchorSave = Text.Anchor;
            Text.Anchor = TextAnchor.MiddleCenter;

            // Initialize header, footer, and content boxes.
            var headerRect = inRect.TopPartPixels(40);
            var footerRect = inRect.BottomPartPixels(90);
            var mainRect = new Rect(inRect);
            headerRect.y += 80;
            mainRect.y += 120;
            mainRect.height -= 210;

            // Display header content image and pawn information.
            Widgets.ButtonImage(new Rect(0, 0, inRect.width - 20, 80), Tex.SkillWorkshopHeader, Color.white, Color.white, false);
            Listing_Standard prelist = new Listing_Standard();
            prelist.Begin(headerRect);

            prelist.Label("ATR_PawnBeingModified".Translate() + pawn.LabelShort + ", " + pawn.story.TitleShort);
            prelist.GapLine();

            prelist.End();

            // Ensure main content has enough space to display everything using a scrollbox.
            bool needToScroll = cachedScrollHeight > mainRect.height;
            var viewRect = new Rect(mainRect);
            if (needToScroll)
            {
                viewRect.width -= 20f;
                viewRect.height = cachedScrollHeight;
                Widgets.BeginScrollView(mainRect, ref scrollPosition, viewRect);
            }

            Listing_Standard listingStandard = new Listing_Standard();
            listingStandard.maxOneColumn = true;
            listingStandard.Begin(viewRect);

            float maxWidth = listingStandard.ColumnWidth;

            SkillRecord skillRecord;
            float availableSkillPoints = Utils.gameComp.GetPoints(ServerType.SkillServer);

            // Controls for buying skill points
            for (int i = 0; i != skillDefList.Count; i++)
            {
                skillRecord = pawn.skills.GetSkill(skillDefList[i]);

                // If this skill does not exist or is disabled, move on to the next skill.
                if (skillRecord == null || skillRecord.TotallyDisabled)
                    continue;

                // Display the current skill, how many points the pawn has in it, and how much they've learned today.
                listingStandard.Label(skillDefTranslationList[i] + ": " + skillRecord.levelInt + ". " + "ATR_CurrentXP".Translate() + skillRecord.xpSinceLastLevel + "/" + skillRecord.XpRequiredForLevelUp + "ATR_TodaysXP".Translate() + skillRecord.xpSinceMidnight);
                
                // Cut the current section in half to make space for adding points and passions to be side by side under their skill.
                var subsection = listingStandard.BeginHiddenSection(out float subsectionHeight);
                subsection.ColumnWidth = (maxWidth - ColumnGap) / 2;

                // Section for purchasing raw xp, a number of skill points for that number of points * Settings modifier. Affected by vanilla (or patched by mod) learning speed effects.
                if (subsection.ButtonText("ATR_AddSkillPoints".Translate(skillDefTranslationList[i])))
                {
                    int insertionRate = ATReforged_Settings.skillPointInsertionRate;
                    if (availableSkillPoints >= insertionRate)
                    { // Use vanilla stat learning to maximize compatibility. Remove skill points on complete.
                        skillRecord.Learn(insertionRate * ATReforged_Settings.skillPointConversionRate);
                        Utils.gameComp.ChangeServerPoints(-insertionRate, ServerType.SkillServer);
                        availableSkillPoints -= insertionRate;
                    }
                    else
                        Messages.Message("ATR_InsufficientPoints".Translate("100"), MessageTypeDefOf.NeutralEvent);
                }

                // Section for passions, starting with getting the correct texture and then displaying the purchase button.
                subsection.NewHiddenColumn(ref subsectionHeight);
                Texture2D texture = null;
                switch (skillDefPassionList[i])
                {
                    case -1: // If this particular skill is disabled, show an uninteractible button with no passion.
                        subsection.ButtonImage(Tex.PassionDisabled, 24, 24);
                        break;
                    case 0: // No Passion
                        texture = Tex.NoPassion;
                        break;
                    case 1: // Minor Passion
                        texture = Tex.MinorPassion;
                        break;
                    case 2: // Major Passion (No need to have the button be interactible as it can't be upgraded/changed at this stage.)
                        subsection.ButtonImage(Tex.MajorPassion, 24, 24);
                        break;
                    default: // Display a no passion and move on as we have no texture for non-vanilla passions available.
                        subsection.ButtonImage(Tex.NoPassion, 24, 24);
                        break;
                }

                if (texture == null)
                { // If there is no need for an interactible button, skip the next step and continue - ensure the subsection has ended.
                    listingStandard.EndHiddenSection(subsection, subsectionHeight);
                    listingStandard.GapLine();
                    continue;
                }

                int pointsToIncreasePassion = Utils.GetSkillPointsToIncreasePassion(pawn, curSumPassions);

                // Display a button for upgrading a passion to the next tier.
                if (subsection.ButtonImage(texture, 24, 24))
                {
                    if (availableSkillPoints >= pointsToIncreasePassion)
                    { // Increase passion tier and take away points used.
                        skillDefPassionList[i]++;
                        skillRecord.passion = (Passion)skillDefPassionList[i];
                        Utils.gameComp.ChangeServerPoints(-pointsToIncreasePassion, ServerType.SkillServer);
                        availableSkillPoints -= pointsToIncreasePassion;
                    }
                    else
                    { // Can't afford to increase, send a message to the player.
                        Messages.Message("ATR_InsufficientPoints".Translate(pointsToIncreasePassion), MessageTypeDefOf.NeutralEvent);
                    }
                }
                listingStandard.EndHiddenSection(subsection, subsectionHeight);
                listingStandard.GapLine();
            }
            cachedScrollHeight = listingStandard.CurHeight;
            listingStandard.End();

            if (needToScroll)
            {
                Widgets.EndScrollView();
            }

            // Display the available points and their usage note.
            Listing_Standard postlist = new Listing_Standard();
            postlist.Begin(footerRect);

            postlist.Label("ATR_AvailableSkillPoints".Translate(availableSkillPoints));
            postlist.Label("ATR_AvailableSkillPointsDesc".Translate());

            postlist.End();

            GUI.color = colorSave;
            Text.Anchor = anchorSave;
        }
    }
}