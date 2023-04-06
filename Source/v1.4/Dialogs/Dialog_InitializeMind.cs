using UnityEngine;
using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ATReforged
{
    public class Dialog_InitializeMind : Dialog_MessageBox
    {
        private const float TitleHeight = 42f;
        private const float DialogWidth = 500f;
        private const float DialogHeight = 300f;

        private Vector2 scrollPosition = Vector2.zero;

        private float creationRealTime = -1f;

        private float TimeUntilInteractive => interactionDelay - (Time.realtimeSinceStartup - creationRealTime);

        private bool InteractionDelayExpired => TimeUntilInteractive <= 0f;

        public override Vector2 InitialSize
        {
            get
            {
                float height = DialogHeight;
                if (title != null) height += TitleHeight;
                return new Vector2(DialogWidth, height);
            }
        }

        // Dialog options for initializing the mind of a mechanical unit that just had its autonomous core initialized.
        public Dialog_InitializeMind(Pawn newIntelligence) : base("ATR_InitializeMindDesc".Translate(), "ATR_SkyMindInitialization".Translate(), null, "ATR_AutomaticInitialization".Translate(), null, "ATR_InitializeMindTitle".Translate(), false)
        {
            // If there is any idle intelligence in the SkyMind, then the new intelligence may download it. This is a standard download action.
            IEnumerable<Pawn> cloudPawns = Utils.gameComp.GetCloudPawns().Where(pawn => pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_MindOperation) == null && !pawn.GetComp<CompSkyMindLink>().HasSurrogate());
            IEnumerable<Thing> networkedPawns = Utils.gameComp.networkedDevices.Where(thing => thing is Pawn pawn && pawn.GetComp<CompSkyMindLink>()?.HasSurrogate() == false);
            if (cloudPawns.Count() + networkedPawns.Count() > 0 && Utils.gameComp.networkedDevices.Count < Utils.gameComp.GetSkyMindNetworkSlots())
            {
                buttonAAction = delegate ()
                {
                    List<FloatMenuOption> opts = new List<FloatMenuOption>();

                    foreach (Pawn pawn in cloudPawns)
                    {
                        opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate ()
                        {
                            Utils.gameComp.AttemptSkyMindConnection(newIntelligence);
                            newIntelligence.GetComp<CompSkyMindLink>().InitiateConnection(4, pawn);
                            // Remove the short reboot Hediff now so there aren't two restarting hediffs but it isn't removed before the long reboot is added.
                            Hediff target = newIntelligence.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_ShortReboot);
                            if (target != null)
                            {
                                newIntelligence.health.RemoveHediff(target);
                            }
                            Close();
                        }));
                    }
                    foreach (Pawn pawn in networkedPawns.Cast<Pawn>())
                    {
                        opts.Add(new FloatMenuOption(pawn.LabelShortCap, delegate ()
                        {
                            Utils.gameComp.AttemptSkyMindConnection(newIntelligence);
                            newIntelligence.GetComp<CompSkyMindLink>().InitiateConnection(4, pawn);
                            // Remove the short reboot Hediff now so there aren't two restarting hediffs but it isn't removed before the long reboot is added.
                            Hediff target = newIntelligence.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_ShortReboot);
                            if (target != null)
                            {
                                newIntelligence.health.RemoveHediff(target);
                            }
                            Close();
                        }));
                    }
                    opts.SortBy((x) => x.Label);

                    if (opts.Count == 0)
                        Log.Error("[ATR] Initializing a mind via SkyMind attempted but no viable pawns were found!");
                    Find.WindowStack.Add(new FloatMenu(opts, ""));
                };
            }
            // Players may choose to allow the new pawn to initialize itself and choose its own personality and intelligence.
            buttonBAction = delegate ()
            {
                PawnGenerationRequest request = new PawnGenerationRequest(newIntelligence.kindDef, Faction.OfPlayer, forceGenerateNewPawn: true, canGeneratePawnRelations: false, allowAddictions: false, fixedBiologicalAge: 30, forceNoIdeo: true, colonistRelationChanceFactor: 0, forceBaselinerChance: 1f);
                Pawn newPawn = PawnGenerator.GeneratePawn(request);
                newPawn.story.Childhood = ATR_BackstoryDefOf.ATR_NewbootChildhood;
                Utils.Duplicate(newPawn, newIntelligence, false, false);
                Hediff rebootHediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_LongReboot, newIntelligence, null);
                rebootHediff.Severity = 1;
                newIntelligence.health.AddHediff(rebootHediff);
                // Remove the short reboot Hediff now so there aren't two restarting hediffs but it isn't removed before the long reboot is added.
                Hediff target = newIntelligence.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_ShortReboot);
                if (target != null)
                {
                    newIntelligence.health.RemoveHediff(target);
                }

                // Allow the player to pick a few passions and a trait for the new android, akin to child growth moments in Biotech.
                if (ModLister.BiotechInstalled)
                {
                    ChoiceLetter_PersonalityShift choiceLetter = (ChoiceLetter_PersonalityShift)LetterMaker.MakeLetter(ATR_LetterDefOf.ATR_PersonalityShiftLetter);
                    choiceLetter.ConfigureChoiceLetter(newIntelligence, 3, 3, false, false);
                    choiceLetter.Label = "ATR_PersonalityShiftNewboot".Translate();
                    choiceLetter.StartTimeout(120000);
                    Find.LetterStack.ReceiveLetter(choiceLetter);
                }
            };
            closeOnCancel = false;
            closeOnAccept = false;
        }

        public override void DoWindowContents(Rect inRect)
        {
            float num = inRect.y;
            if (!title.NullOrEmpty())
            {
                Text.Font = GameFont.Medium;
                Widgets.Label(new Rect(0f, num, inRect.width, 42f), title);
                num += 42f;
            }

            Text.Font = GameFont.Small;
            Rect outRect = new Rect(inRect.x, num, inRect.width, inRect.height - 35f - 5f - num);
            float width = outRect.width - 16f;
            Rect viewRect = new Rect(0f, 0f, width, Text.CalcHeight(text, width));
            Widgets.BeginScrollView(outRect, ref scrollPosition, viewRect);
            Widgets.Label(new Rect(0f, 0f, viewRect.width, viewRect.height), text);
            Widgets.EndScrollView();
            int num4 = (buttonCText.NullOrEmpty() ? 2 : 3);
            float num5 = inRect.width / (float)num4;
            float width2 = num5 - 10f;

            string label = InteractionDelayExpired ? buttonAText : (buttonAText + "(" + Mathf.Ceil(TimeUntilInteractive).ToString("F0") + ")");
            if (Widgets.ButtonText(new Rect(num5 * (float)(num4 - 1) + 10f, inRect.height - 35f, width2, 35f), label) && InteractionDelayExpired)
            {
                if (buttonAAction != null)
                {
                    buttonAAction();
                }
                else
                {
                    Messages.Message("ATR_NoAvailableTarget".Translate(), MessageTypeDefOf.NeutralEvent);
                }
            }

            GUI.color = Color.white;
            if (buttonBText != null && Widgets.ButtonText(new Rect(0f, inRect.height - 35f, width2, 35f), buttonBText))
            {
                buttonBAction?.Invoke();
                Close();
            }
        }
    }
}