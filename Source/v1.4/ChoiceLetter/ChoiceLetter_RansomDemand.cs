using System.Collections.Generic;
using Verse;
using RimWorld;

namespace ATReforged
{
    public class ChoiceLetter_RansomDemand : ChoiceLetter
    {
        public int fee;
        public HashSet<Thing> cryptolockedThings;
        public bool deviceType = false;

        // This choice letter handles cryptolocker ransoms. Ransoms can be paid and the viruses will always be cleared if done. 
        public override IEnumerable<DiaOption> Choices
        {
            get
            {
                if (ArchivedOnly)
                {
                    yield return Option_Close;
                }
                else
                {
                    // Attempt to find a map with a sufficient amount of silver
                    Map target = null;
                    foreach (Map map in Find.Maps)
                    {
                        if (map.IsPlayerHome && TradeUtility.ColonyHasEnoughSilver(map, fee))
                        {
                            target = map;
                            break;
                        }
                    }

                    // Action taken when ransom is paid.
                    DiaOption accept = new DiaOption("RansomDemand_Accept".Translate())
                    {
                        action = delegate
                        {
                            // Remove the fee from the player's storage.
                            TradeUtility.LaunchSilver(target, fee);

                            // Remove all cryptolocker viruses from the virused list and send a letter.
                            Utils.RemoveViruses(cryptolockedThings);
                            Messages.Message("ATR_GridlockerCleared".Translate(), MessageTypeDefOf.PositiveEvent);
                            Find.LetterStack.RemoveLetter(this);
                        },
                        resolveTree = true
                    };
                    // Condition for being able to pay the ransom.
                    if (target == null)
                    {
                        accept.Disable("NeedSilverLaunchable".Translate(fee.ToString()));
                    }
                    yield return accept;
                    yield return Option_Reject;
                    yield return Option_Postpone;
                }
            }
        }

        public override bool CanShowInLetterStack
        {
            get
            {
                return base.CanShowInLetterStack;
            }
        }

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref cryptolockedThings, "ATR_cryptolockedThings", LookMode.Value);
            Scribe_Values.Look(ref fee, "ATR_cryptoFee", 0, false);
            Scribe_Values.Look(ref deviceType, "ATR_deviceType", false, false);
        }
    }
}