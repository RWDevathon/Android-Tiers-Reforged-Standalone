using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;
using HarmonyLib;
using Verse.AI;
using RimWorld.Planet;

namespace ATReforged
{
    public static class Utils
    {
        // GENERAL UTILITIES
        public static Gender GenerateGender(PawnKindDef pawnKind)
        { // Return a Gender for a new pawn, based on settings and on the pawn being spawned.
            // Only mechanical androids have proper genders. Mechanical drones and animals never have gender.
            if (!ATReforged_Settings.isConsideredMechanicalAndroid.Contains(pawnKind.race))
                return Gender.None;

            // If androids are not allowed to have genders by setting, then set to none.
            if (!ATReforged_Settings.androidsHaveGenders)
                return Gender.None;

            // If androids don't pick their own gender by setting, then set it to the one players selected in settings.
            if (!ATReforged_Settings.androidsPickGenders)
                return ATReforged_Settings.androidsFixedGender;

            // If androids pick their gender, then randomly select a gender based on the matching setting.
            if (Rand.Chance(ATReforged_Settings.androidsGenderRatio))
                return Gender.Male;
            // If it did not randomly select male, then it randomly selected female.
            else
                return Gender.Female;
        }

        public static bool IsConsideredMechanical(Pawn pawn)
        {
            return ATReforged_Settings.isConsideredMechanical.Contains(pawn.def);
        }

        public static bool IsConsideredMechanical(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanical.Contains(thingDef);
        }

        public static bool IsConsideredMechanicalAnimal(Pawn pawn)
        {
            return ATReforged_Settings.isConsideredMechanicalAnimal.Contains(pawn.def);
        }

        public static bool IsConsideredMechanicalAndroid(Pawn pawn)
        {
            return ATReforged_Settings.isConsideredMechanicalAndroid.Contains(pawn.def);
        }

        public static bool IsConsideredMechanicalAndroid(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanicalAndroid.Contains(thingDef);
        }

        // If the race is considered drone by nature in the settings or if the unit has no core intelligence, return true.
        public static bool IsConsideredMechanicalDrone(Pawn pawn)
        { 
            return ATReforged_Settings.isConsideredMechanicalDrone.Contains(pawn.def) || pawn.health.hediffSet.hediffs.Any(testHediff => testHediff.def == HediffDefOf.ATR_IsolatedCore);
        }

        public static bool IsConsideredMechanicalDrone(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanicalDrone.Contains(thingDef);
        }

        public static bool IsConsideredMassive(Pawn pawn)
        {
            return pawn.BodySize > 4.0f;
        }

        public static bool HasSpecialStatus(Pawn pawn)
        {
            return ATReforged_Settings.hasSpecialStatus.Contains(pawn.def) || ReservedSpecialPawns.Contains(pawn.def.defName);
        }

        public static bool HasSpecialStatus(ThingDef thingDef)
        {
            return ATReforged_Settings.hasSpecialStatus.Contains(thingDef) || ReservedSpecialPawns.Contains(thingDef.defName);
        }

        public static bool IsSolarFlarePresent()
        {
            return Find.World.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare);
        }


        /* === POWER UTILITIES === */
        public static bool CanUseBattery(Pawn pawn) // TODO: Add hediff check for organics to be able to charge
        {
            return ATReforged_Settings.canUseBattery.Contains(pawn.def);
        }

        public static bool CanUseBattery(ThingDef thingDef)
        {
            return ATReforged_Settings.canUseBattery.Contains(thingDef);
        }

        /* === HEALTH UTILITIES === */

        // Returns true if the provided thing is in the reserved list of repair stims or is recognized in the settings.
        public static bool IsMechanicalRepairStim(ThingDef thing)
        {
            return ReservedRepairStims.Contains(thing.defName) || ATReforged_Settings.thingsAllowedAsRepairStims.Contains(thing.defName);
        }

        /* === CONNECTIVITY UTILITIES === */

        // There are four hediffs that grant SkyMind connectivity. If any are present, this pawn has a cloud capable implant. If settings allow the pawn's race to use it innately, return true as well.
        public static bool HasCloudCapableImplant(Pawn pawn)
        { 
            return pawn.health.hediffSet.hediffs.Any(testHediff => testHediff.def == HediffDefOf.ATR_AutonomousCore || testHediff.def == HediffDefOf.ATR_ReceiverCore || testHediff.def == HediffDefOf.ATR_SkyMindReceiver || testHediff.def == HediffDefOf.ATR_SkyMindTransceiver);
        }

        public static bool IsSurrogate(Pawn pawn)
        { // Returns true if the pawn has a receiver core or a receiver implant.
            return pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == HediffDefOf.ATR_ReceiverCore || hediff.def == HediffDefOf.ATR_SkyMindReceiver).Any();
        }

        // Returns true if the pawn is a surrogate and has an active controller.
        public static bool IsControlledSurrogate(Pawn pawn)
        { 
            return IsSurrogate(pawn) && pawn.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_NoController) != null && pawn.TryGetComp<CompSkyMindLink>().HasSurrogate();
        }

        public static bool FactionCanUseSkyMind(FactionDef factionDef)
        {
            return ATReforged_Settings.factionsUsingSkyMind.Contains(factionDef.defName);
        }

        // Misc

        public static void ApplySkyMindAttack(IEnumerable<Pawn> victims = null, ThoughtDef forVictim = null, ThoughtDef forWitness = null)
        { // When a SkyMind is breached, all users of the SkyMind receive a mood debuff. It is especially bad for direct victims.
            try
            {
                if (victims != null)
                { // Victims were directly attacked by a hack and get a worse mood debuff
                    foreach (Pawn pawn in victims)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemoryFast(forVictim ?? SkyMindAttackVictimDef);
                    }
                }

                // Witnesses (connected to SkyMind but not targetted directly) get a minor mood debuff
                foreach (Pawn pawn in gameComp.GetSkyMindDevices().Where(thing => thing is Pawn pawn && !victims.Contains(pawn)).Cast<Pawn>())
                {
                    pawn.needs.mood.thoughts.memories.TryGainMemoryFast(forWitness ?? SkyMindAttackVictimDef);
                }
            }
            catch (Exception ex)
            {
                Log.Error("[ATR] Error applying SkyMind attack mood thoughts. " + ex.Message + " " + ex.StackTrace);
            }
        }

        // Remove viruses from the provided things. While they are assumed to have viruses, no errors will occur if non-virused things are provided.
        public static void RemoveViruses(IEnumerable<Thing> virusedThings)
        { 
            if (virusedThings == null)
            {
                return;
            }

            // Remove the viruses from each provided thing. No errors will occur if the thing does not have a SkyMind comp or does not have a virus.
            foreach (Thing virusedThing in virusedThings)
            { 
                CompSkyMind csm = virusedThing.TryGetComp<CompSkyMind>();

                if (csm == null)
                    continue;
                csm.Breached = -1;
                gameComp.PopVirusedThing(virusedThing);
            }
        }

        // Get a cached Blank pawn (to avoid having to create a new pawn whenever a surrogate is made, disconnects, downed, etc.)
        public static Pawn GetBlank()
        {
            if (ReservedBlank != null)
            {
                return ReservedBlank;
            }
            
            // Create the Blank pawn that will be used for all non-controlled surrogates, blank androids, etc.
            PawnGenerationRequest request = new PawnGenerationRequest(RimWorld.PawnKindDefOf.Colonist, null, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true, fixedGender: Gender.None);
            Pawn blankMechanical = PawnGenerator.GeneratePawn(request);
            BackstoryDatabase.TryGetWithIdentifier("FreshBlank", out blankMechanical.story.childhood);
            BackstoryDatabase.TryGetWithIdentifier("AdultBlank", out blankMechanical.story.adulthood);
            blankMechanical.story.traits.allTraits.Clear();
            blankMechanical.skills.Notify_SkillDisablesChanged();
            blankMechanical.skills.skills.ForEach(delegate (SkillRecord record)
            {
                record.passion = 0;
                record.Level = 0;
                record.xpSinceLastLevel = 0;
                record.xpSinceMidnight = 0;
            });
            blankMechanical.workSettings.EnableAndInitializeIfNotAlreadyInitialized();
            blankMechanical.workSettings.DisableAll();
            blankMechanical.playerSettings = new Pawn_PlayerSettings(blankMechanical)
            {
                AreaRestriction = null,
                hostilityResponse = HostilityResponseMode.Flee
            };
            if (blankMechanical.timetable == null)
                blankMechanical.timetable = new Pawn_TimetableTracker(blankMechanical);
            if (blankMechanical.playerSettings == null)
                blankMechanical.playerSettings = new Pawn_PlayerSettings(blankMechanical);
            if (blankMechanical.foodRestriction == null)
                blankMechanical.foodRestriction = new Pawn_FoodRestrictionTracker(blankMechanical);
            if (blankMechanical.drugs == null)
                blankMechanical.drugs = new Pawn_DrugPolicyTracker(blankMechanical);
            if (blankMechanical.outfits == null)
                blankMechanical.outfits = new Pawn_OutfitTracker(blankMechanical);
            blankMechanical.Name = new NameTriple("Unit 404", "Blank", "Error");
            ReservedBlank = blankMechanical;
            return ReservedBlank;
        }
        
        // RESERVED UTILITIES, INTERNAL USE ONLY
        public static HashSet<string> ReservedSpecialPawns = new HashSet<string> { "Tier5Android" };

        public static HashSet<string> ReservedFactionCanUseSurrogates = new HashSet<string> { "AndroidUnion", "MechanicalMarauders" };
        public static HashSet<string> ReservedRepairStims = new HashSet<string> { "ATR_RepairStimSimple", "ATR_RepairStimIntermediate", "ATR_RepairStimAdvanced" };

        public static Pawn ReservedBlank = null; // To be applied to surrogates, new androids, etc. Generated when first needed, then stored for later.

        // Utilities not available for direct player editing but not reserved by this mod
        public static List<PawnKindDef> ValidSurrogatePawnKindDefs = new List<PawnKindDef>();
        public static List<ThingDef> ValidServerDefs = new List<ThingDef>();
        public static ThoughtDef SkyMindAttackWitnessDef = new ThoughtDef();
        public static ThoughtDef SkyMindAttackVictimDef = new ThoughtDef();
        public static ThoughtDef SkyMindTrollVictimDef = new ThoughtDef();

        // The other shit:

        public static FleckDef ATR_FullChargeFleck;
        public static FleckDef ATR_HalfChargeFleck;
        public static FleckDef ATR_EmptyChargeFleck;

        public static List<string> BlacklistedHediffsForAndroids = new List<string> { "Anxiety" };

        public static ATR_GameComponent gameComp;

        public static int GetPowerUsageByPawn(Pawn pawn)
        {
            return (int) (ATReforged_Settings.wattsConsumedPerBodySize * pawn.BodySize);
        }


        public static Building_Bed GetAvailableChargingBed(Pawn pawn)
        { // Return a viable bed for charging in if there is one. Prefer the bed the pawn already owns, if it has one.
            Map map = pawn.Map;

            // Check if the pawn owns a bed that is charge-capable, online, and accessible. If it is, then it will choose that to charge.
            if (pawn.ownership != null && pawn.ownership.OwnedBed != null)
            {
                Building_Bed pawnBed = pawn.ownership.OwnedBed;
                CompPowerTrader compPowerTrader = pawnBed.TryGetComp<CompPowerTrader>();
                if (pawnBed is Building_ChargingBed && compPowerTrader != null)
                {
                    if (!pawnBed.Destroyed && compPowerTrader.PowerOn && pawn.CanReserveAndReach(pawn.ownership.OwnedBed, PathEndMode.OnCell, Danger.Deadly) && pawn.ownership.OwnedBed.Position.InAllowedArea(pawn))
                    {
                        return pawnBed;
                    }
                }
            }

            // Locate a viable charge-capable, online, accessible bed for the pawn. Store the closest such bed - achieved by looking from closest bed to furthest bed.
            foreach(Building building in map.listerBuildings.allBuildingsColonist.Where(target => target is Building_ChargingBed || target.TryGetComp<CompAndroidPod>() != null).OrderBy(b => b.Position.DistanceToSquared(pawn.Position)))
            {
                Building_Bed bed = (Building_Bed)building;
                CompPowerTrader cpt = building.TryGetComp<CompPowerTrader>();
                if (!building.Destroyed && cpt != null)
                {
                    if (!bed.Medical
                    && (pawn.IsPrisoner == bed.ForPrisoners)
                    && !(bed.GetCurOccupant(0) != null || (bed.OwnersForReading.Count() != 0 && !bed.OwnersForReading.Contains(pawn)))
                    && cpt.PowerOn
                    && building.Position.InAllowedArea(pawn)
                    && pawn.CanReserveAndReach(building, PathEndMode.OnCell, Danger.Deadly, 1, -1, null, false))
                    {
                        // Located the closest available charging bed. Claim it if possible, then return it.
                        pawn.ownership.ClaimBedIfNonMedical(bed);
                        return bed;
                    }
                }
            }

            return null;
        }

        public static void ThrowChargingFleck(Pawn curPawn)
        {
            if (!curPawn.Map.moteCounter.Saturated)
            {
                if (curPawn.needs.food.CurLevelPercentage >= 0.80f) { ThrowFleck(ATR_FullChargeFleck, curPawn); }
                else if (curPawn.needs.food.CurLevelPercentage >= 0.40f) { ThrowFleck(ATR_HalfChargeFleck, curPawn); }
                else { ThrowFleck(ATR_EmptyChargeFleck, curPawn); }
            }
        }

        // Generate a surrogate and properly apply to it a blank personality and the appropriate receiver implant.
        public static Pawn GenerateSurrogate(Faction faction, PawnKindDef kindDef, Gender gender = Gender.None)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(kindDef, faction, PawnGenerationContext.NonPlayer, forceGenerateNewPawn: true, fixedGender : gender);
            Pawn surrogate = PawnGenerator.GeneratePawn(request);
            if (IsConsideredMechanicalAndroid(surrogate))
            {
                // Remove any isolated or autonomous core hediffs before applying ReceiverCore to the brain.
                Hediff target = surrogate.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_AutonomousCore);
                if (target != null)
                    surrogate.health.RemoveHediff(target);
                target = surrogate.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_IsolatedCore);
                if (target != null)
                    surrogate.health.RemoveHediff(target);
                surrogate.health.AddHediff(HediffDefOf.ATR_ReceiverCore, surrogate.health.hediffSet.GetBrain());
            }
            else
                surrogate.health.AddHediff(HediffDefOf.ATR_SkyMindReceiver, surrogate.health.hediffSet.GetBrain());
            Duplicate(GetBlank(), surrogate, false);
            return surrogate;
        }

        public static void ThrowFleck(FleckDef fleckDef, Pawn pawn)
        {
            FleckMaker.ThrowMetaIcon(pawn.Position, pawn.Map, fleckDef, Rand.Range(0.35f, 0.55f));
        }

        // Duplicate the source pawn into the destination pawn. If overwriteAsDeath is true, then it is considered murdering the destination pawn.
        // if isTethered is true, then the duplicated pawn will actually share the class with the source so changing one will affect the other automatically.
        public static void Duplicate(Pawn source, Pawn dest, bool overwriteAsDeath=true, bool isTethered = true)
        { 
            try
            {
                // Duplicate source story into destination.
                if (source.story != null)
                {
                    // If not tethered, simply copy data over.
                    if (!isTethered)
                    {
                        // Ensure the target has a story.
                        if (dest.story == null)
                            dest.story = new Pawn_StoryTracker(dest);

                        // Duplicate source backstory into destination.
                        if (source.story.adulthood != null)
                        {
                            BackstoryDatabase.TryGetWithIdentifier(source.story.adulthood.identifier, out dest.story.adulthood);
                        }
                        else
                            dest.story.adulthood = null;

                        BackstoryDatabase.TryGetWithIdentifier(source.story.childhood.identifier, out dest.story.childhood);

                        // Duplicate source traits into destination.
                        dest.story.traits = new TraitSet(dest);
                        foreach (Trait trait in source.story.traits.allTraits)
                        {
                            Trait newTrait = new Trait(trait.def, trait.Degree, false);
                            dest.story.traits.allTraits.Add(newTrait);
                        }
                    }
                    // Tether destination and source traits and backstory together.
                    else
                    {
                        dest.story.traits = source.story.traits;
                        dest.story.childhood = source.story.childhood;
                        dest.story.adulthood = source.story.adulthood;
                    }
                    dest.story.title = source.story.title;
                    dest.story.favoriteColor = source.story.favoriteColor;
                    dest.Notify_DisabledWorkTypesChanged();
                    dest.skills.Notify_SkillDisablesChanged();
                }

                // If Ideology dlc is active, duplicate pawn ideology into destination.
                if (ModsConfig.IdeologyActive)
                {
                    if (!isTethered)
                    {
                        dest.ideo.SetIdeo(source.Ideo);
                        dest.ideo.OffsetCertainty(source.ideo.Certainty - dest.ideo.Certainty);
                        dest.ideo.joinTick = source.ideo.joinTick;
                    }
                    else
                    {
                        dest.ideo = source.ideo;
                    }
                }

                // If Royalty dlc is active, duplicate pawn titles into destination. Warning: This can only tether, the Empire does not recognize SkyMind or consciousness transfer.
                if (ModsConfig.RoyaltyActive && isTethered)
                {
                    dest.royalty = source.royalty;
                }

                // Duplicate source skills into destination.
                if (!isTethered)
                {
                    Pawn_SkillTracker newSkills = new Pawn_SkillTracker(source);
                    foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
                    {
                        SkillRecord newSkill = newSkills.GetSkill(skillDef);
                        SkillRecord sourceSkill = source.skills.GetSkill(skillDef);
                        newSkill.Level = sourceSkill.Level;

                        if (!sourceSkill.TotallyDisabled)
                        {
                            newSkill.passion = sourceSkill.passion;
                            newSkill.xpSinceLastLevel = sourceSkill.xpSinceLastLevel;
                            newSkill.xpSinceMidnight = sourceSkill.xpSinceMidnight;
                        }
                    }
                    dest.skills = newSkills;
                }
                else
                {
                    dest.skills = source.skills;
                }

                // Duplicate source relations into destination. If this duplication is considered murder, handle destination relations first.
                if (overwriteAsDeath)
                {
                    PawnDiedOrDownedThoughtsUtility.TryGiveThoughts(dest, null, PawnDiedOrDownedThoughtsKind.Died);
                    Pawn spouse = dest.relations?.GetFirstDirectRelationPawn(PawnRelationDefOf.Spouse);
                    if (spouse != null && !spouse.Dead && spouse.needs.mood != null)
                    {
                        MemoryThoughtHandler memories = spouse.needs.mood.thoughts.memories;
                        memories.RemoveMemoriesOfDef(ThoughtDefOf.GotMarried);
                        memories.RemoveMemoriesOfDef(ThoughtDefOf.HoneymoonPhase);
                    }
                    Traverse.Create(dest.relations).Method("AffectBondedAnimalsOnMyDeath").GetValue();
                    Hediff target = dest.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_SkyMindReceiver);
                    if (target == null)
                        target = dest.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_IsolatedCore);
                    dest.health.NotifyPlayerOfKilled(null, target, null);
                }

                // Duplicate relations.
                if (!isTethered)
                {
                    Pawn_RelationsTracker destRelations = new Pawn_RelationsTracker(dest);
                    // Duplicate all world pawn relations.
                    foreach (Pawn worldPawn in Find.WorldPawns.AllPawnsAlive)
                    {
                        if (worldPawn == null || worldPawn.relations == null || worldPawn.relations.DirectRelations == null)
                            continue;

                        // For each of the world pawns relations, if it relates to the source, add it to the destination.
                        foreach (DirectPawnRelation pawnRelation in worldPawn.relations.DirectRelations.ToList())
                        {
                            if (pawnRelation.otherPawn != null && pawnRelation.otherPawn == source)
                            {
                                worldPawn.relations.AddDirectRelation(pawnRelation.def, dest);
                            }
                        }
                    }

                    // Duplicate all of the source's relations.
                    foreach (DirectPawnRelation pawnRelation in source.relations.DirectRelations.ToList())
                    {
                        destRelations.AddDirectRelation(pawnRelation.def, pawnRelation.otherPawn);
                    }

                    destRelations.everSeenByPlayer = true;

                    // Transfer animal master status to destination
                    foreach (Map map in Find.Maps)
                    {
                        foreach (Pawn pawn in map.mapPawns.AllPawns)
                        {
                            if (pawn.playerSettings == null || pawn == source || pawn == dest)
                                continue;

                            if (pawn.playerSettings.Master != null && pawn.playerSettings.Master == source)
                                pawn.playerSettings.Master = dest;
                        }
                    }
                }
                // Tether destination relations to the source.
                else
                {
                    dest.relations = source.relations;
                }

                // Duplicate faction. No difference if tethered or not.
                if (source.Faction != dest.Faction)
                    dest.SetFaction(source.Faction);

                // Duplicate source needs into destination. This is not tetherable.
                Pawn_NeedsTracker newNeeds = new Pawn_NeedsTracker(dest);
                foreach (Thought_Memory memory in source.needs.mood.thoughts.memories.Memories)
                {
                    newNeeds.mood.thoughts.memories.Memories.Add(memory);
                }
                dest.needs = newNeeds;
                dest.needs.AddOrRemoveNeedsAsAppropriate();
                dest.needs.mood.thoughts.situational.Notify_SituationalThoughtsDirty();

                // Only duplicate source settings for player pawns as foreign pawns don't need them. Can not be tethered as otherwise pawns would be forced to have same work/time/role settings.
                if (source.Faction != null && dest.Faction != null && source.Faction.IsPlayer && dest.Faction.IsPlayer)
                {
                    // Initialize source work settings if not initialized.
                    if (source.workSettings == null)
                    { 
                        source.workSettings = new Pawn_WorkSettings(source);
                    }
                    source.workSettings.EnableAndInitializeIfNotAlreadyInitialized();

                    // Initialize destination work settings if not initialized.
                    if (dest.workSettings == null)
                    { 
                        dest.workSettings = new Pawn_WorkSettings(dest);
                    }
                    dest.workSettings.EnableAndInitializeIfNotAlreadyInitialized();

                    // Apply work settings to destination from the source
                    if (source.workSettings != null && source.workSettings.EverWork)
                    { 
                        foreach (WorkTypeDef workTypeDef in DefDatabase<WorkTypeDef>.AllDefsListForReading)
                        {
                            if (!dest.WorkTypeIsDisabled(workTypeDef))
                                dest.workSettings.SetPriority(workTypeDef, source.workSettings.GetPriority(workTypeDef));
                        }
                    }

                    // Duplicate source restrictions from into destination.
                    for (int i = 0; i != 24; i++)
                    {
                        dest.timetable.SetAssignment(i, source.timetable.GetAssignment(i));
                    }

                    dest.playerSettings = new Pawn_PlayerSettings(dest);
                    dest.playerSettings.AreaRestriction = source.playerSettings.AreaRestriction;
                    dest.playerSettings.hostilityResponse = source.playerSettings.hostilityResponse;
                    dest.outfits = new Pawn_OutfitTracker(dest);
                    dest.outfits.CurrentOutfit = source.outfits.CurrentOutfit;
                }

                // Duplicate source name into destination.
                NameTriple sourceName = (NameTriple)source.Name;
                dest.Name = new NameTriple(sourceName.First, sourceName.Nick, sourceName.Last);

                dest.Drawer.renderer.graphics.ResolveAllGraphics();
            }
            catch(Exception e)
            {
                Log.Error("[ATR] Utils.Duplicate : " + e.Message + " - " + e.StackTrace);
            }
        }

        public static void PermutePawn(Pawn firstPawn, Pawn secondPawn)
        {
            try
            {
                if (firstPawn == null || secondPawn == null)
                    return;

                // Permute all major mind-related components to each other via a temp copy.
                PawnGenerationRequest request = new PawnGenerationRequest(RimWorld.PawnKindDefOf.Colonist, null, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true);
                Pawn tempCopy = PawnGenerator.GeneratePawn(request);
                Duplicate(firstPawn, tempCopy, false);
                Duplicate(secondPawn, firstPawn, false);
                Duplicate(tempCopy, secondPawn, false);

                
                // Swap all log entries between the two pawns as appropriate.
                foreach (LogEntry log in Find.PlayLog.AllEntries)
                {
                    if (log.Concerns(firstPawn) || log.Concerns(secondPawn))
                    {
                        Traverse tlog = Traverse.Create(log);
                        Pawn initiator = tlog.Field("initiator").GetValue<Pawn>();
                        Pawn recipient = tlog.Field("recipient").GetValue<Pawn>();

                        if (initiator == firstPawn)
                            initiator = secondPawn;
                        else if (initiator == secondPawn)
                            initiator = firstPawn;

                        if (recipient == secondPawn)
                            recipient = secondPawn;
                        else if (recipient == firstPawn)
                            recipient = secondPawn;

                        tlog.Field("initiator").SetValue(initiator);
                        tlog.Field("recipient").SetValue(recipient);
                    }
                }

                // Swap any applicable royal titles between the two pawns as appropriate.
                Pawn_RoyaltyTracker tmpRoyalty = firstPawn.royalty;

                if(firstPawn.royalty != null)
                    firstPawn.royalty.pawn = secondPawn;
                if(secondPawn.royalty != null)
                    secondPawn.royalty.pawn = firstPawn;

                firstPawn.royalty = secondPawn.royalty;
                secondPawn.royalty = tmpRoyalty;
                if (firstPawn.royalty != null)
                {
                    firstPawn.royalty.UpdateAvailableAbilities();
                    if (firstPawn.needs != null)
                        firstPawn.needs.AddOrRemoveNeedsAsAppropriate();
                    firstPawn.abilities.Notify_TemporaryAbilitiesChanged();
                }
                if (secondPawn.royalty != null)
                {
                    secondPawn.royalty.UpdateAvailableAbilities();
                    if (secondPawn.needs != null)
                        secondPawn.needs.AddOrRemoveNeedsAsAppropriate();
                    secondPawn.abilities.Notify_TemporaryAbilitiesChanged();
                }
            }
            catch(Exception e)
            {
                Log.Message("[ATR] Utils.PermutePawn : " + e.Message + " - " + e.StackTrace);
            }
        }


        public static void gainDirectTrait(Pawn_StoryTracker tr,  Trait trait)
        {
            if (tr.traits.HasTrait(trait.def))
                return;

            tr.traits.allTraits.Add(trait);
        }

        // Check if the targetted pawn is a valid target for receiving mind transfer operations.
        public static bool IsValidMindTransferTarget(Pawn pawn)
        {
            // Only player pawns that are connected to the SkyMind, not suffering from a security breach, and not currently in a SkyMind operation are legal targets.
            if ((pawn.Faction != null && pawn.Faction != Faction.OfPlayer) || !gameComp.HasSkyMindConnection(pawn) || pawn.TryGetComp<CompSkyMind>().Breached != -1 || pawn.TryGetComp<CompSkyMindLink>().Linked > -1)
            {
                return false;
            }

            // Pawns afflicted with dementia, memory corruption, or who are already subjects of mind operations are not permissible targets for mind operations.
            if (pawn.health.hediffSet.hediffs.Where(hediff => hediff.def == HediffDefOf.ATR_MemoryCorruption || hediff.def == RimWorld.HediffDefOf.Dementia || hediff.def == HediffDefOf.ATR_MindOperation).Any())
            {
                return false;
            }

            // If the pawn has a cloud capable implant or is in the SkyMind network already, then it is valid.
            return HasCloudCapableImplant(pawn);
        }

        // Returns a list of all surrogates without hosts in caravans. Return null if there are none.
        public static IEnumerable<Pawn> GetHostlessCaravanSurrogates()
        {
            IEnumerable<Pawn> hostlessSurrogates = new HashSet<Pawn>();
            foreach (Caravan caravan in Find.World.worldObjects.Caravans)
            {
                foreach (Pawn pawn in caravan.pawns)
                {
                    if (IsSurrogate(pawn) && !pawn.TryGetComp<CompSkyMindLink>().HasSurrogate())
                    {
                        hostlessSurrogates.AddItem(pawn);
                    }
                }
            }
            return hostlessSurrogates.Count() == 0 ? null : hostlessSurrogates;
        }
        
        // Create as close to a perfect copy of the provided pawn as possible. If kill is true, then we're trying to make a corpse copy of it.
        public static Pawn SpawnCopy(Pawn pawn, bool kill=true)
        {
            // Generate a new pawn.
            PawnGenerationRequest request = new PawnGenerationRequest(kind: pawn.kindDef, faction: null, context: PawnGenerationContext.NonPlayer, fixedBiologicalAge: pawn.ageTracker.AgeBiologicalYearsFloat, fixedChronologicalAge: pawn.ageTracker.AgeChronologicalYearsFloat, fixedGender: pawn.gender, fixedMelanin: pawn.story.melanin);
            Pawn copy = PawnGenerator.GeneratePawn(request);

            // Get rid of any items it may have spawned with.
            copy?.equipment?.DestroyAllEquipment();
            copy?.apparel?.DestroyAll();
            copy?.inventory?.DestroyAll();

            // Copy the pawn's physical attributes.
            copy.Rotation = pawn.Rotation;
            copy.story.bodyType = pawn.story.bodyType;
            Color hair = new Color
            {
                a = pawn.story.hairColor.a,
                r = pawn.story.hairColor.r,
                g = pawn.story.hairColor.g,
                b = pawn.story.hairColor.b
            };
            copy.story.hairColor = hair;
            copy.story.crownType = pawn.story.crownType;
            copy.story.hairDef = pawn.story.hairDef;

            // Attempt to transfer all items the pawn may be carrying over to its copy.
            if (pawn.inventory != null && pawn.inventory.innerContainer != null && copy.inventory != null && copy.inventory.innerContainer != null)
            {
                try
                {
                    pawn.inventory.innerContainer.TryTransferAllToContainer(copy.inventory.innerContainer);
                }
                catch (Exception ex)
                {
                    Log.Error("[ATR] Utils.SpawnCopy.TransferInventory " + ex.Message + " " + ex.StackTrace);
                }
            }

            // Attempt to transfer all equipment the pawn may have to the copy.
            if (pawn.equipment != null && copy.equipment != null)
            {
                foreach (ThingWithComps equipment in pawn.equipment.AllEquipmentListForReading.ToList())
                {
                    try
                    {
                        pawn.equipment.Remove(equipment);
                        copy.equipment.AddEquipment(equipment);
                    }
                    catch (Exception ex)
                    {
                        Log.Message("[ATR] Utils.SpawnCopy.TransferEquipment " + ex.Message + " " + ex.StackTrace);
                    }
                }
            }

            // Transfer all apparel from the pawn to the copy.
            if (pawn.apparel != null)
            {
                foreach (Apparel apparel in pawn.apparel.WornApparel.ToList())
                {
                    pawn.apparel.Remove(apparel);
                    copy.apparel.Wear(apparel);
                }
            }

            // Copy all hediffs from the pawn to the copy.
            copy.health.RemoveAllHediffs();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.ToList())
            {
                try
                {
                    hediff.pawn = copy;
                    copy.health.AddHediff(hediff, hediff.Part);
                }
                catch(Exception)
                {

                }
            }

            // If we are "killing" the android, that means the body is now a blank. Properly duplicate those features.
            if (kill)
            {
                Duplicate(GetBlank(), copy, false, false);

                // Androids that become blanks should also lose their interface so that they're ready for a new intelligence.
                if (IsConsideredMechanicalAndroid(copy))
                {
                    Hediff autoCore = copy.health.hediffSet.GetFirstHediffOfDef(HediffDefOf.ATR_AutonomousCore);
                    if (autoCore != null)
                    {
                        copy.health.RemoveHediff(autoCore);
                    }
                }
            }
            // Else, duplicate all mind-related things to the copy. This is not considered murder.
            else
            {
                Duplicate(pawn, copy, false, false);
            }

            // Spawn the copy.
            GenSpawn.Spawn(copy, pawn.Position, pawn.Map);

            // Draw the copy.
            copy.Drawer.renderer.graphics.ResolveAllGraphics();
            return copy;
        }

        // Calculate the number of skill points required in order to give a pawn a new passion.
        public static int GetSkillPointsToIncreasePassion(Pawn pawn, int passionCount)
        {
            // Assign base cost based on settings. Default is 1000.
            float result = ATReforged_Settings.basePointsNeededForPassion;

            // Multiply result by the pawn's global learning factor (inverse relation, as higher learning factor should reduce cost).
            result *= 1 / pawn.GetStatValue(StatDef.Named("GlobalLearningFactor"));

            if (passionCount > ATReforged_Settings.passionSoftCap)
            { // If over the soft cap for number of passions, each additional passion adds 25% cost to buying another passion.
                result *= (float) Math.Pow(1.25, passionCount - ATReforged_Settings.passionSoftCap);
            } 

            // Return the end result as an integer for nice display numbers and costs (servers can't give parts of a skill point).
            return (int) result;
        }

        // Remove blacklisted traits from mechanical units.
        public static void removeMindBlacklistedTrait(Pawn pawn)
        {
            // Remove blacklisted traits for androids.
            foreach (Trait trait in pawn.story.traits.allTraits.Where(trait => ATReforged_Settings.blacklistedMechanicalTraits.Contains(trait.def.defName)).ToList())
            {
                pawn.story.traits.RemoveTrait(trait);
            }
        }

        // Handle various parts of resetting drones to default status.
        public static void ReconfigureDrone(Pawn pawn)
        {
            if (IsConsideredMechanicalDrone(pawn))
            {
                // Drones don't have traits.
                foreach (Trait trait in pawn.story.traits.allTraits.ToList())
                {
                    pawn.story.traits.RemoveTrait(trait);
                }

                // Drones don't have ideos.
                pawn.ideo = null;

                // Drones have a set skill of 8 in all skills. Massive drones get 14 in all skills.
                int skillLevel = IsConsideredMassive(pawn) ? 14 : 8;
                foreach (SkillRecord skillRecord in pawn.skills.skills)
                {
                    skillRecord.passion = 0;
                    skillRecord.Level = skillLevel;
                    skillRecord.xpSinceLastLevel = 0;
                }

                // Massive drones get MSeries drone backstory, which they spawn with.
                if (!IsConsideredMassive(pawn))
                {
                    BackstoryDatabase.TryGetWithIdentifier("ATR_DroneChildhood", out pawn.story.childhood);
                    BackstoryDatabase.TryGetWithIdentifier("ATR_DroneAdulthood", out pawn.story.adulthood);
                }
            }
        }
    }
}
