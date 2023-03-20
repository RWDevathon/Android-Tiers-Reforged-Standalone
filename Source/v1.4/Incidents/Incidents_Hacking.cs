using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Linq;

namespace ATReforged
{
    public class Incidents_Hacking : IncidentWorker
    {
        private LetterDef letter;
        private string letterLabel;
        private string letterText;
        protected override bool CanFireNowSub(IncidentParms parms)
        {
            return ATReforged_Settings.enemyHacksOccur && !Utils.IsSolarFlarePresent() && HasIncidentToFire();
        }

        protected override bool TryExecuteWorker(IncidentParms parms)
        { // Generate and execute a hacking attempt on the player. Return true if one was carried out. Return false if it aborted for any reason.
            if (!HasIncidentToFire())
            { // No hacks should execute if there can be no hacking attempt made. It also should not have been firable in the first place.
                return false;
            }
            int incidentToFire = PickHackType();
            switch (incidentToFire)
            {
                case 1:
                    if (!TryExecuteGridVirus(parms))
                        return false;
                    return true;
                case 2:
                    if (!TryExecuteDDOS(parms))
                        return false;
                    return true;
                case 3:
                    if (!TryExecuteTroll(parms))
                        return false;
                    return true;
                case 4:
                    if (!TryExecuteDiplohack(parms))
                        return false;
                    return true;
                case 5:
                    if (!TryExecuteProvokerhack(parms))
                        return false;
                    return true;
                case 6:
                    if (!TryExecuteCounterhack(parms))
                        return false;
                    return true;
            }

            return true;
        }

        // Grid Virus for structures and surrogates (Hack Mode 1)

        public bool CanFireGridVirus()
        {
            return Utils.gameComp.GetSkyMindDevices().Where(thing => !(thing is Pawn pawn) || Utils.IsSurrogate(pawn)).Count() > 0;
        }

        public bool TryExecuteGridVirus(IncidentParms parms)
        {
            // Generate attack strength and defense strength
            float attackStrength = parms.points * ATReforged_Settings.enemyHackAttackStrengthModifier;
            float defenseStrength = Utils.gameComp.GetPoints(ServerType.SecurityServer);

            if (HandleDefenseSuccessful(defenseStrength, attackStrength, 1, "ATR_IncidentGridVirusDefeated", "ATR_IncidentGridVirusAllyIntercept"))
                return true; // Function handles everything if it returns true.

            // Enemy attack succeeds

            // Select the attack type. 1 == crash, 2 == cryptolocked, 3 == breakdown. Can only cryptolock if there's at least enough money to cover the base demand.
            int attackType = Rand.RangeInclusive(1, 3);
            int fee = 100;
            if (attackType == 2 && !TradeUtility.ColonyHasEnoughSilver(Find.CurrentMap, fee))
            { // If the player can't afford even the base fee, switch the attack to a simple crash.
                attackType = 1;
            }


            HashSet<Thing> cryptolockedThings = new HashSet<Thing>();
            HashSet<Thing> potentialVictims = new HashSet<Thing>(Utils.gameComp.GetSkyMindDevices().Where(thing => !(thing is Pawn pawn) || Utils.IsSurrogate(pawn)));
            int targetDeviceCount = Rand.RangeInclusive(1, potentialVictims.Count);

            // Generate message for the hack and adding extra information depending on the attack type.
            letter = LetterDefOf.ThreatSmall;
            letterText = "ATR_IncidentGenericAttackDesc".Translate(attackStrength, targetDeviceCount) + "\n\n";

            switch (attackType)
            {
                case 1: // Grid-sleeper message
                    letterLabel = "ATR_IncidentGridsleeperAttack".Translate();
                    letterText += "ATR_IncidentGridsleeperAttackDesc".Translate();
                    break;
                case 2: // Grid-locker message
                    letterLabel = "ATR_IncidentGridlockerAttack".Translate();
                    letterText += "ATR_IncidentGridlockerAttackDesc".Translate();
                    break;
                case 3: // Grid-breaker message
                    letterLabel = "ATR_IncidentGridbreakerAttack".Translate();
                    letterText += "ATR_IncidentGridbreakerAttackDesc".Translate();
                    break;
            }

            // Identify victim devices and apply appropriate grid virus to them.
            HashSet<Thing> victims = new HashSet<Thing>();
            for (int i = 0; i < targetDeviceCount; i++)
            {
                Thing victim = potentialVictims.RandomElement();
                victims.Add(victim);
                potentialVictims.Remove(victim);
            }

            foreach (Thing victim in victims)
            {
                Utils.gameComp.DisconnectFromSkyMind(victim);
                victim.TryGetComp<CompSkyMind>().Breached = attackType;

                // Grid-sleeper and Grid-breaker time to repair
                if (attackType != 2)
                {
                    Utils.gameComp.PushVirusedThing(victim, Find.TickManager.TicksGame + Rand.RangeInclusive(30000, 120000));
                }

                // Grid-locker Encryption fee increases per victim.
                if (attackType == 2)
                {
                    Utils.gameComp.PushVirusedThing(victim, -1);
                    cryptolockedThings.Add(victim);
                    fee += (int)(victim.def.BaseMarketValue * ATReforged_Settings.percentageOfValueUsedForRansoms);
                }

                // Grid-breaker forcing explosion/breakdown of victim
                if (attackType == 3)
                {
                    CompBreakdownable breakComp = victim.TryGetComp<CompBreakdownable>();
                    if (breakComp != null)
                    {
                        breakComp.DoBreakdown();
                    }
                    GenExplosion.DoExplosion(victim.Position, victim.Map, 1, DamageDefOf.Flame, null);
                    GenExplosion.DoExplosion(victim.Position, victim.Map, 4, DamageDefOf.EMP, null);
                }
            }

            // Apply a mood debuff to all current SkyMind users as they clearly can't trust the safety of the SkyMind network. There are no direct victims in this attack however.
            Utils.ApplySkyMindAttack();
            SendLetter(letter, letterLabel, letterText, new LookTargets(victims));

            if (attackType == 2)
            { // If the attack was a grid-locker, create a ransom demand for removing the effect and send it.
                ChoiceLetter_RansomDemand ransom = (ChoiceLetter_RansomDemand)LetterMaker.MakeLetter(DefDatabase<LetterDef>.GetNamed("ATR_RansomChoiceLetter"));
                ransom.title = "ATR_CryptolockerNeedRansom".Translate();
                ransom.Text = "ATR_CryptolockerNeedRansomDesc".Translate(victims.Count, fee.ToString());
                ransom.radioMode = true;
                ransom.fee = fee;
                ransom.cryptolockedThings = cryptolockedThings;
                ransom.deviceType = true;
                ransom.StartTimeout(60000);
                Find.LetterStack.ReceiveLetter(ransom, null);
            }

            return true;
        }

        // DDOS Attack (Hack Mode 2)

        public bool CanFireDDOS()
        {
            return Utils.gameComp.GetSkyMindDevices().Any();
        }

        public bool TryExecuteDDOS(IncidentParms parms)
        {
            // Generate an attack strength that is 1.0 - 4.0 times normal attack strength. DDOS attacks are very dangerous to security points.
            float attackStrength = GenerateAttackStrength(parms.points, 1, 4);
            float defenseStrength = Utils.gameComp.GetPoints(ServerType.SecurityServer);

            // Handle point loss, attack success checking, and ally intervention.
            if (HandleDefenseSuccessful(defenseStrength, attackStrength, 1, "ATR_IncidentDDOSDefeated", "ATR_IncidentDDOSAllyIntercept"))
                return true; // Function handles everything if it returns true.

            // Enemy attack succeeds
            int targetPawnCount = Utils.gameComp.GetSkyMindDevices().Where(thing => thing is Pawn).Count();

            // Remaining points goes to damaging Skill/Hacking, half each.
            float leftOverPoints = attackStrength - defenseStrength;
            Utils.gameComp.ChangeServerPoints(-leftOverPoints / 2, ServerType.SkillServer);
            Utils.gameComp.ChangeServerPoints(-leftOverPoints / 2, ServerType.HackingServer);

            // Initialize hack letter. Message starts with generic and then will append an additional string.
            letter = LetterDefOf.ThreatSmall;
            letterLabel = "ATR_IncidentDDOSAttack".Translate();
            letterText = "ATR_IncidentGenericAttackDesc".Translate(attackStrength, targetPawnCount) + "\n\n";

            // DDOS attacks only affect individual units if the attack strength is at least 1.5x the defense strength.
            if (attackStrength >= defenseStrength * 1.5)
            {
                letterText += "ATR_IncidentDDOSOverwhelmingAttackDesc".Translate();
                HashSet<Pawn> victims = new HashSet<Pawn>();

                // All pawns get the recovery hediff. Everyone is a victim of DDOS attacks.
                foreach (Pawn user in Utils.gameComp.GetSkyMindDevices().Where(thing => thing is Pawn).Cast<Pawn>())
                {
                    user.health.AddHediff(ATR_HediffDefOf.ATR_RecoveringFromDDOS, user.health.hediffSet.GetBrain());
                    victims.Add(user);
                }
                foreach (Pawn surrogate in Utils.gameComp.GetSkyMindDevices().Where(thing => thing is Pawn pawn && Utils.IsSurrogate(pawn)).Cast<Pawn>())
                {
                    surrogate.health.AddHediff(ATR_HediffDefOf.ATR_RecoveringFromDDOS, surrogate.health.hediffSet.GetBrain());
                    victims.Add(surrogate);
                }
                // Apply a SkyMind mood debuff to all victims of the DDOS attack. Everyone is a victim of DDOS attacks.
                Utils.ApplySkyMindAttack(victims);
            }
            else
            {
                letterText += "ATR_IncidentDDOSNormalAttackDesc".Translate();
                // Apply a minor mood debuff to all witnesses of the DDOS attack. Anyone can be a victim of a sufficiently strong DDOS attack.
                Utils.ApplySkyMindAttack();
            }

            SendLetter(letter, letterLabel, letterText);
            return true;
        }

        public bool CanFireTroll()
        {
            return Utils.gameComp.GetSkyMindDevices().Where(thing => thing is Pawn pawn && !Utils.IsSurrogate(pawn)).Any();
        }

        public bool TryExecuteTroll(IncidentParms parms)
        {
            // Generate attack strength and defense strength
            float attackStrength = parms.points * ATReforged_Settings.enemyHackAttackStrengthModifier;
            float defenseStrength = Utils.gameComp.GetPoints(ServerType.SecurityServer);


            // Handle point loss, attack success checking, and ally intervention.
            if (HandleDefenseSuccessful(defenseStrength, attackStrength, 1, "ATR_IncidentTrollDefeated", "ATR_IncidentTrollAllyIntercept"))
                return true; // Function handles everything if it returns true.

            // Enemy attack succeeds

            // Select victim. Troll attacks target a singular (non-surrogate) individual.
            Pawn victim = (Pawn) Utils.gameComp.GetSkyMindDevices().Where(thing => thing is Pawn pawn && !Utils.IsSurrogate(pawn)).RandomElement();
            float remainingAttackStrength = attackStrength - defenseStrength;
            float totalSkillPointsLost = 0;

            // Damage the victim's good (> 10 levels) skills and the skills they like doing (passions).
            foreach (SkillRecord skillRecord in victim.skills.skills)
            {
                if (skillRecord != null && (skillRecord.Level >= 10 || skillRecord.passion != Passion.None))
                {
                    skillRecord.Learn(-remainingAttackStrength, direct: true);
                    totalSkillPointsLost += remainingAttackStrength;
                }
            }

            // Apply the Trolled mood debuff to this victim and the witnessed breach debuff to all other pawns.
            Utils.ApplySkyMindAttack((IEnumerable<Pawn>) victim, ATR_ThoughtDefOf.ATR_TrolledViaSkyMind);

            // Create hack letter. Message starts with generic and then will append an additional string.
            letter = LetterDefOf.ThreatSmall;
            letterLabel = "ATR_IncidentTrollAttack".Translate();
            letterText = "ATR_IncidentGenericAttackDesc".Translate(attackStrength, 1) + "\n\n";
            letterText += "ATR_IncidentTrollAttackDesc".Translate(victim.LabelShortCap, totalSkillPointsLost);
            SendLetter(letter, letterLabel, letterText, new LookTargets(victim));

            return true;
        }

        public bool CanFireDiplohack()
        {
            // Diplomacy hacks can only occur if there is some way to carry out diplomacy.
            bool hasComms = false;
            foreach (Map activeMap in Find.Maps)
            {
                if (activeMap.listerBuildings.allBuildingsColonist.Any(building => building.def.IsCommsConsole))
                {
                    hasComms = true;
                    break;
                }
            }
            return Utils.gameComp.GetSkyMindDevices().Any() && hasComms;
        }

        public bool TryExecuteDiplohack(IncidentParms parms)
        {
            // Generate attack strength and defense strength
            float attackStrength = parms.points * ATReforged_Settings.enemyHackAttackStrengthModifier;
            float defenseStrength = Utils.gameComp.GetPoints(ServerType.SecurityServer);


            // Handle point loss, attack success checking, and ally intervention.
            if (HandleDefenseSuccessful(defenseStrength, attackStrength, 1, "ATR_IncidentDiplohackDefeated", "ATR_IncidentDiplohackAllyIntercept"))
                return true; // Function handles everything if it returns true.

            // Enemy attack succeeds

            // Victim is a random non-hostile non-hidden faction
            Faction victim = Find.FactionManager.RandomNonHostileFaction();

            victim.TryAffectGoodwillWith(Faction.OfPlayer, Rand.RangeInclusive(-5, -10));

            foreach (Faction otherFaction in Find.FactionManager.AllFactionsVisible.Where(otherFaction => otherFaction.AllyOrNeutralTo(victim) && !otherFaction.IsPlayer))
            { // The victim makes sure to insult the player faction to any other faction willing to listen.
                victim.TryAffectGoodwillWith(Faction.OfPlayer, Rand.RangeInclusive(0, -2));
            }

            // Create hack letter.
            letter = LetterDefOf.ThreatSmall;
            letterLabel = "ATR_IncidentDiplohackAttack".Translate();
            letterText = "ATR_IncidentDiplohackAttackDesc".Translate(victim.Name);
            SendLetter(letter, letterLabel, letterText);

            return true;
        }

        public bool CanFireProvokerhack()
        {
            return Utils.gameComp.GetSkyMindDevices().Any();
        }

        public bool TryExecuteProvokerhack(IncidentParms parms)
        {
            // Generate attack strength and defense strength
            float attackStrength = parms.points * ATReforged_Settings.enemyHackAttackStrengthModifier;
            float defenseStrength = Utils.gameComp.GetPoints(ServerType.SecurityServer);

            // Handle point loss, attack success checking, and ally intervention.
            if (HandleDefenseSuccessful(defenseStrength, attackStrength, 1, "ATR_IncidentProvokerhackDefeated", "ATR_IncidentProvokerhackAllyIntercept"))
                return true; // Function handles everything if it returns true.

            // Enemy attack succeeds

            // Try to generate a raid with normal raid points.
            FiringIncident incident = new FiringIncident();
            incident.def = IncidentDefOf.RaidEnemy;
            incident.parms = parms;
            // Incident was unable to fire.
            if (!Find.Storyteller.TryFire(incident))
            { 
                Log.Warning("[ATR] Attempted to fire a raid incident when it was unable to. Attempting to execute a DDOS attack instead.");
                return TryExecuteDDOS(parms);
            }

            // Create hack letter.
            letter = LetterDefOf.ThreatSmall;
            letterLabel = "ATR_IncidentProvokerhackAttack".Translate();
            letterText = "ATR_IncidentProvokerhackAttackDesc".Translate();
            SendLetter(letter, letterLabel, letterText);
            return true;
        }

        public bool CanFireCounterhack()
        {
            return Utils.gameComp.GetSkyMindDevices().Any();
        }

        public bool TryExecuteCounterhack(IncidentParms parms)
        {
            // Generate attack strength and defense strength
            float attackStrength = GenerateAttackStrength(parms.points, 1, 4);
            float defenseStrength = Utils.gameComp.GetPoints(ServerType.SecurityServer);

            // Handle point loss, attack success checking, and ally intervention.
            if (HandleDefenseSuccessful(defenseStrength, attackStrength, 0.25f, "ATR_IncidentCounterhackDefeated", "ATR_IncidentCounterhackAllyIntercept"))
                return true; // Function handles everything if it returns true.

            // Enemy attack succeeds

            // Counterhacks destroy hacking points first, then skill points.
            float remainingAttackStrengthAfterHacking = parms.points - Utils.gameComp.GetPoints(ServerType.HackingServer);
            Utils.gameComp.ChangeServerPoints(-Utils.gameComp.GetPoints(ServerType.HackingServer), ServerType.HackingServer);
            if (remainingAttackStrengthAfterHacking > 0)
                Utils.gameComp.ChangeServerPoints(-remainingAttackStrengthAfterHacking, ServerType.SkillServer);

            // Create hack letter.
            letter = LetterDefOf.ThreatSmall;
            letterLabel = "ATR_IncidentCounterhackAttack".Translate();
            letterText = "ATR_IncidentCounterhackAttackDesc".Translate(parms.points);
            SendLetter(letter, letterLabel, letterText);
            return true;
        }

        public bool HasIncidentToFire()
        {
            return CanFireGridVirus() || CanFireDDOS() || CanFireTroll() || CanFireDiplohack() || CanFireProvokerhack() || CanFireCounterhack();
        }

        public int PickHackType()
        {
            HashSet<int> validTypes = new HashSet<int>();
            if (CanFireGridVirus())
                validTypes.Add(1);
            if (CanFireDDOS())
                validTypes.Add(2);
            if (CanFireTroll())
                validTypes.Add(3);
            if (CanFireDiplohack())
                validTypes.Add(4);
            if (CanFireProvokerhack())
                validTypes.Add(5);
            if (CanFireCounterhack())
                validTypes.Add(6);
            return validTypes.RandomElementByWeight(GetHackTypeWeights);
        }

        private float GetHackTypeWeights(int target)
        {
            switch (target)
            {
                case 1:
                case 2:
                    return 1f;
                case 3:
                case 4:
                    return .25f;
                case 5:
                    return .5f;
                case 6:
                    return 1f;
                default:
                    return 1f;
            }
        }

        // Internal Functions to use in incidents

        private void SendLetter(LetterDef letter, string label, string text, LookTargets lookTargets = null)
        { // Send a letter, defaulting to defense success if no parameters are provided.
            Find.LetterStack.ReceiveLetter(label, text, letter ?? LetterDefOf.NeutralEvent, lookTargets);
        }

        private float GenerateAttackStrength(float baseStrength, float lowerBoundModifier, float upperBoundModifier)
        { // Generates the attack strength as the baseStrength times a random percentage value between the lower and upper bound modifiers. IE. 100, 1, 4 == 100 * (100% ~ 400%).
            return baseStrength * Rand.Range(lowerBoundModifier, upperBoundModifier) * ATReforged_Settings.enemyHackAttackStrengthModifier;
        }

        private bool HandleDefenseSuccessful(float defenseStrength, float attackStrength, float damageModifier, string interceptTitle = "ATR_IncidentGenericAllyIntercept", string label = null, string text = null)
        { // Check if the defense was successful. If it was, send an appropriate letter. If not, inform the caller of the failed defense.
            bool defenseSuccessful = false;

            if (defenseStrength >= attackStrength)
            { // Defense was successful. No damage is done, subtract strength that was used to defend. 
                letter = LetterDefOf.NeutralEvent;
                SendLetter(letter, label != null ? label.Translate() :"ATR_IncidentGenericDefense".Translate(), text != null ? text.Translate(attackStrength) : "ATR_IncidentGenericDefenseDesc".Translate(attackStrength));
                defenseSuccessful = true;
            }

            if (!defenseSuccessful && HandleAlliesIntercept(attackStrength, interceptTitle))
            { // Allies handled the attack - including preventing loss of points and sent the letter. Terminate early.
                return true;
            }

            // Points are subtracted from the security servers. 
            Utils.gameComp.ChangeServerPoints(-attackStrength * damageModifier, ServerType.SecurityServer);
            return defenseSuccessful;
        }

        private bool HandleAlliesIntercept(float attackStrength, string interceptTitle)
        { // Check if allies intercepted a hacking attempt. Each allied faction has a 5% of being in a position to catch and stop an attack. If it does, it gives the player 25% of the attack strength as free points.
            IEnumerable<Faction> alliedValidFactions = Find.FactionManager.GetFactions().Where(targetFaction => Utils.FactionCanUseSkyMind(targetFaction.def) && targetFaction.PlayerRelationKind == FactionRelationKind.Ally);

            foreach (Faction faction in alliedValidFactions)
            { // Allied factions each have a percentage chance (settings) of catching the attack and being in a position to prevent it. 
                if (Rand.Chance(ATReforged_Settings.chanceAlliesInterceptHack))
                {
                    // Add 25% of the attack strength to the security points. Overflows are accounted for automatically.
                    float gainedStrength = (attackStrength * ATReforged_Settings.pointsGainedOnInterceptPercentage);
                    Utils.gameComp.ChangeServerPoints(gainedStrength, ServerType.SecurityServer);

                    // Generate the message and terminate the function.
                    LetterDef letter = LetterDefOf.PositiveEvent;
                    string title = interceptTitle.Translate();
                    string msg = "ATR_IncidentGenericAllyInterceptDesc".Translate(faction.Name, attackStrength, gainedStrength);
                    SendLetter(letter, letterLabel, letterText);
                    return true;
                }
            }
            // If this is reached, then no allied factions intercepted the attack.
            return false;
        }
    }
}
