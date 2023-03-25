using System;
using System.Collections.Generic;
using RimWorld;
using Verse;
using Verse.AI;
using HarmonyLib;
using RimWorld.Planet;
using System.Linq;
using AlienRace;

namespace ATReforged
{
    public static class Utils
    {
        // GENERAL UTILITIES
        // Return a new Gender for a mechanical pawn, based on settings and on the pawn kind.
        public static Gender GenerateGender(PawnKindDef pawnKind)
        {
            // Only mechanical androids have proper genders. Mechanical drones and animals never have gender.
            if (!IsConsideredMechanicalAndroid(pawnKind.race))
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
            return ATReforged_Settings.isConsideredMechanical.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanical(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanical.Contains(thingDef.defName);
        }

        public static bool IsConsideredMechanicalAnimal(Pawn pawn)
        {
            return ATReforged_Settings.isConsideredMechanicalAnimal.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanicalAnimal(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanicalAnimal.Contains(thingDef.defName);
        }

        public static bool IsConsideredMechanicalAndroid(Pawn pawn)
        {
            return ATReforged_Settings.isConsideredMechanicalAndroid.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanicalAndroid(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanicalAndroid.Contains(thingDef.defName);
        }

        public static bool IsConsideredMechanicalDrone(Pawn pawn)
        {
            return ATReforged_Settings.isConsideredMechanicalDrone.Contains(pawn.def.defName);
        }

        public static bool IsConsideredMechanicalDrone(ThingDef thingDef)
        {
            return ATReforged_Settings.isConsideredMechanicalDrone.Contains(thingDef.defName);
        }

        public static PawnType GetPawnType(Pawn pawn)
        {
            if (IsConsideredMechanicalAndroid(pawn))
            {
                return PawnType.Android;
            }
            else if (IsConsideredMechanicalDrone(pawn))
            {
                return PawnType.Drone;
            }
            else if (IsConsideredMechanicalAnimal(pawn))
            {
                return PawnType.Mechanical;
            }
            else
            {
                return PawnType.Organic;
            }
        }

        public static bool IsConsideredMassive(Pawn pawn)
        {
            return pawn.BodySize > 4.0f;
        }

        public static bool HasSpecialStatus(Pawn pawn)
        {
            return ATReforged_Settings.hasSpecialStatus.Contains(pawn.def.defName);
        }

        public static bool HasSpecialStatus(ThingDef thingDef)
        {
            return ATReforged_Settings.hasSpecialStatus.Contains(thingDef.defName);
        }

        public static bool IsSolarFlarePresent()
        {
            return Find.World.gameConditionManager.ConditionIsActive(GameConditionDefOf.SolarFlare);
        }

        /* === POWER UTILITIES === */
        public static bool CanUseBattery(Pawn pawn)
        {
            return ATReforged_Settings.canUseBattery.Contains(pawn.def.defName) || pawn.health.hediffSet.hediffs.Any(hediff => hediff.TryGetComp<HediffComp_ChargeCapable>() != null);
        }

        public static bool CanUseBattery(ThingDef thingDef)
        {
            return ATReforged_Settings.canUseBattery.Contains(thingDef.defName);
        }

        // Locate the nearest available charging bed for the given pawn user, as carried by the given pawn carrier. Pawns may carry themselves here, if they are not downed.
        public static Building_Bed GetChargingBed(Pawn user, Pawn carrier)
        {
            if (user.Map == null)
                return null;

            return (Building_Bed)GenClosest.ClosestThingReachable(user.PositionHeld, user.MapHeld, ThingRequest.ForGroup(ThingRequestGroup.Bed), PathEndMode.OnCell, TraverseParms.For(carrier), 9999f, (Thing b) => b.def.IsBed && (int)b.Position.GetDangerFor(user, user.Map) <= (int)Danger.Deadly && RestUtility.IsValidBedFor(b, user, carrier, true) && ((b.TryGetComp<CompPawnCharger>() != null && b.TryGetComp<CompPowerTrader>()?.PowerOn == true) || b.TryGetComp<CompAffectedByFacilities>()?.LinkedFacilitiesListForReading.Any(thing => thing.TryGetComp<CompPawnCharger>() != null && thing.TryGetComp<CompPowerTrader>()?.PowerOn == true) == true));
        }

        /* === HEALTH UTILITIES === */

        // Returns true if the provided thing is in the reserved list of repair stims or is recognized in the settings.
        public static bool IsMechanicalRepairStim(ThingDef thing)
        {
            return ReservedRepairStims.Contains(thing.defName) || ATReforged_Settings.thingsAllowedAsRepairStims.Contains(thing.defName);
        }

        /* === CONNECTIVITY UTILITIES === */

        // If a pawn's Def Extension allows it to use the SkyMind network or it has a hediff that allows it (via a comp bool), return true.
        public static bool HasCloudCapableImplant(Pawn pawn)
        {
            if (pawn.def.GetModExtension<ATR_MechTweaker>()?.canInherentlyUseSkyMind == true)
            {
                return true;
            }

            List<Hediff> pawnHediffs = pawn.health.hediffSet.hediffs;
            for (int i = pawnHediffs.Count - 1; i >= 0; i--)
            {
                if (pawnHediffs[i].TryGetComp<HediffComp_SkyMindEffecter>()?.AllowsSkyMindConnection == true)
                    return true;
            }
            return false;
        }

        // Returns true if the pawn has a Hediff that acts as a receiver via the SkyMindEffecter comp marking it as one.
        public static bool IsSurrogate(Pawn pawn)
        {
            List<Hediff> pawnHediffs = pawn.health.hediffSet.hediffs;
            for (int i = pawnHediffs.Count - 1; i >= 0; i--)
            {
                if (pawnHediffs[i].TryGetComp<HediffComp_SkyMindEffecter>()?.IsReceiver == true)
                    return true;
            }
            return false;
        }

        public static bool FactionCanUseSkyMind(FactionDef factionDef)
        {
            return ATReforged_Settings.factionsUsingSkyMind.Contains(factionDef.defName);
        }


        // Misc
        // When a SkyMind is breached, all users of the SkyMind receive a mood debuff. It is especially bad for direct victims.
        public static void ApplySkyMindAttack(IEnumerable<Pawn> victims = null, ThoughtDef forVictim = null, ThoughtDef forWitness = null)
        {
            try
            {
                // Victims were directly attacked by a hack and get a worse mood debuff
                if (victims != null && victims.Count() > 0)
                {
                    foreach (Pawn pawn in victims)
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemoryFast(forVictim ?? ATR_ThoughtDefOf.ATR_AttackedViaSkyMind);
                    }
                }

                // Witnesses (connected to SkyMind but not targetted directly) get a minor mood debuff
                foreach (Thing thing in gameComp.GetSkyMindDevices())
                {
                    if (thing is Pawn pawn && (victims == null || !victims.Contains(pawn)))
                    {
                        pawn.needs.mood.thoughts.memories.TryGainMemoryFast(forWitness ?? ATR_ThoughtDefOf.ATR_AttackedViaSkyMind);
                    }
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
            if (gameComp.blankPawn != null)
            {
                return gameComp.blankPawn;
            }

            // Create the Blank pawn that will be used for all non-controlled surrogates, blank androids, etc.
            PawnGenerationRequest request = new PawnGenerationRequest(Faction.OfPlayer.def.basicMemberKind, null, PawnGenerationContext.PlayerStarter, canGeneratePawnRelations: false, forceBaselinerChance: 1, colonistRelationChanceFactor: 0f, forceGenerateNewPawn: true, fixedGender: Gender.None);
            Pawn blankMechanical = PawnGenerator.GeneratePawn(request);
            blankMechanical.story.Childhood = ATR_BackstoryDefOf.ATR_MechChildhoodFreshBlank;
            blankMechanical.story.Adulthood = ATR_BackstoryDefOf.ATR_MechAdulthoodBlank;
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
            if (ModsConfig.BiotechActive)
                for (int i = blankMechanical.genes.GenesListForReading.Count - 1; i >= 0; i--)
                {
                    blankMechanical.genes.RemoveGene(blankMechanical.genes.GenesListForReading[i]);
                }
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
            gameComp.blankPawn = blankMechanical;
            return gameComp.blankPawn;
        }

        // RESERVED UTILITIES, INTERNAL USE ONLY
        public static HashSet<string> ReservedBlacklistedDiseases = new HashSet<string> { "WoundInfection" };

        public static HashSet<string> ReservedAndroidFactions = new HashSet<string> { "ATR_PlayerAndroidFaction", "ATR_AndroidUnion", "ATR_MechanicalMarauders" };
        public static HashSet<string> ReservedRepairStims = new HashSet<string> { "ATR_RepairStimSimple", "ATR_RepairStimIntermediate", "ATR_RepairStimAdvanced" };

        // Utilities not available for direct player editing but not reserved by this mod
        public static List<PawnKindDef> ValidSurrogatePawnKindDefs = new List<PawnKindDef>();
        public static List<ThingDef> ValidServerDefs = new List<ThingDef>();

        public static ATR_GameComponent gameComp;

        // Generate a surrogate and properly apply a blank personality and the appropriate receiver implant to it.
        public static Pawn GenerateSurrogate(PawnKindDef kindDef, Gender gender = Gender.None)
        {
            PawnGenerationRequest request = new PawnGenerationRequest(kindDef, forceGenerateNewPawn: true, fixedGender : gender);
            Pawn surrogate = PawnGenerator.GeneratePawn(request);
            if (IsConsideredMechanicalAndroid(surrogate))
            {
                // Remove any isolated or autonomous core hediffs before applying ReceiverCore to the brain.
                Hediff target = surrogate.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_AutonomousCore);
                if (target != null)
                    surrogate.health.RemoveHediff(target);
                target = surrogate.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_IsolatedCore);
                if (target != null)
                    surrogate.health.RemoveHediff(target);
                surrogate.health.AddHediff(ATR_HediffDefOf.ATR_ReceiverCore, surrogate.health.hediffSet.GetBrain());
            }
            else
                surrogate.health.AddHediff(ATR_HediffDefOf.ATR_SkyMindReceiver, surrogate.health.hediffSet.GetBrain());
            Duplicate(GetBlank(), surrogate, false, false);
            return surrogate;
        }

        // Duplicate the source pawn into the destination pawn. If overwriteAsDeath is true, then it is considered murdering the destination pawn.
        // if isTethered is true, then the duplicated pawn will actually share the class with the source so changing one will affect the other automatically.
        public static void Duplicate(Pawn source, Pawn dest, bool overwriteAsDeath=true, bool isTethered = true)
        {
            try
            {
                DuplicateStory(ref source, ref dest);

                // If Ideology dlc is active, duplicate pawn ideology into destination.
                if (ModsConfig.IdeologyActive)
                {
                    DuplicateIdeology(source, dest, isTethered);
                }

                // If Royalty dlc is active, then handle it. Royalty is non-transferable, but it should be checked for the other details that have been duplicated.
                if (ModsConfig.RoyaltyActive)
                {
                    DuplicateRoyalty(source, dest, isTethered);
                }

                DuplicateSkills(source, dest, isTethered);

                // If this duplication is considered to be killing a sapient individual, then handle some relations before they're duplicated.
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
                    dest.health.NotifyPlayerOfKilled(null, null, null);
                    dest.relations.ClearAllRelations();
                }

                // Duplicate relations.
                DuplicateRelations(source, dest, isTethered);

                // Duplicate faction. No difference if tethered or not.
                if (source.Faction != dest.Faction)
                    dest.SetFaction(source.Faction);

                // Duplicate source needs into destination. This is not tetherable.
                DuplicateNeeds(source, dest);

                // Only duplicate source settings for player pawns as foreign pawns don't need them. Can not be tethered as otherwise pawns would be forced to have same work/time/role settings.
                if (source.Faction != null && dest.Faction != null && source.Faction.IsPlayer && dest.Faction.IsPlayer)
                {
                    DuplicatePlayerSettings(source, dest);
                }

                // Duplicate source name into destination.
                NameTriple sourceName = (NameTriple)source.Name;
                dest.Name = new NameTriple(sourceName.First, sourceName.Nick, sourceName.Last);

                dest.Drawer.renderer.graphics.ResolveAllGraphics();
            }
            catch(Exception e)
            {
                Log.Error("[ATR] Utils.Duplicate: Error occurred duplicating " + source + " into " + dest + ". This will have severe consequences. " + e.Message + e.StackTrace);
            }
        }

        // Duplicate all appropriate details from the StoryTracker of the source into the destination.
        public static void DuplicateStory(ref Pawn source, ref Pawn dest)
        {
            if (source.story == null || dest.story == null)
            {
                Log.Warning("[ATR] A Storytracker for a duplicate operation was null. Destination story unchanged. This will have no further effects.");
                return;
            }

            try
            {
                // Clear all destination traits first to avoid issues. Only remove traits that are unspecific to genes.
                foreach (Trait trait in dest.story.traits.allTraits.ToList().Where(trait => trait.sourceGene == null))
                {
                    dest.story.traits.RemoveTrait(trait);
                }

                // Add all source traits to the destination. Only add traits that are unspecific to genes.
                foreach (Trait trait in source.story.traits?.allTraits.Where(trait => trait.sourceGene == null))
                {
                    dest.story.traits.GainTrait(new Trait(trait.def, trait.Degree, true));
                }

                // Copy some backstory related details, and double check work types and skill modifiers.
                dest.story.Childhood = source.story.Childhood;
                dest.story.Adulthood = source.story.Adulthood;
                dest.story.title = source.story.title;
                dest.story.favoriteColor = source.story.favoriteColor;
                dest.Notify_DisabledWorkTypesChanged();
                dest.skills.Notify_SkillDisablesChanged();
            }
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during story duplication between " + source + " " + dest + ". The destination StoryTracker may be left unstable!" + exception.Message + exception.StackTrace);
            }
        }

        // Duplicate ideology details from the source to the destination.
        public static void DuplicateIdeology(Pawn source, Pawn dest, bool isTethered)
        {
            try
            {
                // If source ideology is null, then destination's ideology should also be null. Vanilla handles null ideologies relatively gracefully.
                if (source.ideo == null)
                {
                    dest.ideo = null;
                }
                // If untethered, copy the details of the ideology over, as a separate copy.
                else if (!isTethered)
                {
                    dest.ideo = new Pawn_IdeoTracker(dest);
                    dest.ideo.SetIdeo(source.Ideo);
                    dest.ideo.OffsetCertainty(source.ideo.Certainty - dest.ideo.Certainty);
                    dest.ideo.joinTick = source.ideo.joinTick;
                }
                // If tethered, the destination and source will share a single IdeologyTracker.
                else
                {
                    dest.ideo = source.ideo;
                }
            }
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during ideology duplication between " + source + " " + dest + ". The destination IdeoTracker may be left unstable!" + exception.Message + exception.StackTrace);
            }
        }

        // Royalty status can not actually be duplicated, but duplicating a pawn should still handle cases around royal abilities/details.
        public static void DuplicateRoyalty(Pawn source, Pawn dest, bool isTethered)
        {
            try
            {
                if (source.royalty != null)
                {
                    source.royalty.UpdateAvailableAbilities();
                    if (source.needs != null)
                        source.needs.AddOrRemoveNeedsAsAppropriate();
                    source.abilities.Notify_TemporaryAbilitiesChanged();
                }
                if (dest.royalty != null)
                {
                    dest.royalty.UpdateAvailableAbilities();
                    if (dest.needs != null)
                        dest.needs.AddOrRemoveNeedsAsAppropriate();
                    dest.abilities.Notify_TemporaryAbilitiesChanged();
                }
            }
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during royalty duplication between " + source + " " + dest + ". No further issues are anticipated." + exception.Message + exception.StackTrace);
            }
        }

        // Duplicate all skill levels, xp gains, and passions into the destination.
        public static void DuplicateSkills(Pawn source, Pawn dest, bool isTethered)
        {
            try
            {
                // If untethered, create a copy of the source SkillTracker for the destination to use.
                // Explicitly create a new skill tracker to avoid any issues with tethered skill trackers.
                if (!isTethered)
                {
                    Pawn_SkillTracker destSkills = new Pawn_SkillTracker(dest);
                    foreach (SkillDef skillDef in DefDatabase<SkillDef>.AllDefsListForReading)
                    {
                        SkillRecord newSkill = destSkills.GetSkill(skillDef);
                        SkillRecord sourceSkill = source.skills.GetSkill(skillDef);
                        newSkill.Level = sourceSkill.Level;
                        newSkill.passion = sourceSkill.passion;
                        newSkill.xpSinceLastLevel = sourceSkill.xpSinceLastLevel;
                        newSkill.xpSinceMidnight = sourceSkill.xpSinceMidnight;
                    }
                    dest.skills = destSkills;
                }
                // If tethered, the destination and source will share their skill tracker directly.
                else
                {
                    dest.skills = source.skills;
                }
            }
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during skill duplication between " + source + " " + dest + ". The destination SkillTracker may be left unstable!" + exception.Message + exception.StackTrace);
            }
        }

        // Duplicate relations from the source to the destination. This should also affect other pawn relations, and any animals involved.
        public static void DuplicateRelations(Pawn source, Pawn dest, bool isTethered)
        {
            try
            {
                // If untethered, copy all relations that involve the source pawn and apply them to the destination. As animals may have only one master, assign it to the destination.
                if (!isTethered)
                {
                    Pawn_RelationsTracker destRelations = dest.relations;
                    destRelations.ClearAllRelations();

                    List<Pawn> checkedOtherPawns = new List<Pawn>();
                    // Duplicate all of the source's relations. Ensure that other pawns with relations to the source also have them to the destination.
                    foreach (DirectPawnRelation pawnRelation in source.relations?.DirectRelations?.ToList())
                    {
                        // Ensure that we check the pawn relations for the opposite side only once to avoid doing duplicate relations.
                        if (!checkedOtherPawns.Contains(pawnRelation.otherPawn))
                        {
                            // Ensure the other pawn has all the same relations to the destination as it does to the source.
                            foreach (DirectPawnRelation otherPawnRelation in pawnRelation.otherPawn.relations?.DirectRelations.ToList())
                            {
                                if (otherPawnRelation.otherPawn == source)
                                {
                                    otherPawnRelation.otherPawn = dest;
                                }
                            }
                            checkedOtherPawns.Add(pawnRelation.otherPawn);
                        }
                        destRelations.AddDirectRelation(pawnRelation.def, pawnRelation.otherPawn);
                    }

                    destRelations.everSeenByPlayer = true;

                    // Transfer animal master status to destination
                    foreach (Map map in Find.Maps)
                    {
                        foreach (Pawn animal in map.mapPawns.SpawnedColonyAnimals)
                        {
                            if (animal.playerSettings == null)
                                continue;

                            if (animal.playerSettings.Master != null && animal.playerSettings.Master == source)
                                animal.playerSettings.Master = dest;
                        }
                    }
                }
                // Tether destination relations to the source.
                else
                {
                    dest.relations = source.relations;
                }
            }
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during relation duplication between " + source + " " + dest + ". The destination RelationTracker may be left unstable!" + exception.Message + exception.StackTrace);
            }
        }

        // Duplicate applicable needs from the source to the destination. This includes mood thoughts, memories, and ensuring it updates its needs as appropriate.
        public static void DuplicateNeeds(Pawn source, Pawn dest)
        {
            try
            {
                Pawn_NeedsTracker newNeeds = new Pawn_NeedsTracker(dest);
                if (source.needs?.mood != null)
                {
                    foreach (Thought_Memory memory in source.needs.mood.thoughts.memories.Memories)
                    {
                        newNeeds.mood.thoughts.memories.TryGainMemory(memory, memory.otherPawn);
                    }
                }
                dest.needs = newNeeds;
                dest.needs?.AddOrRemoveNeedsAsAppropriate();
                dest.needs?.mood?.thoughts?.situational?.Notify_SituationalThoughtsDirty();
            }
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during need duplication between " + source + " " + dest + ". The destination NeedTracker may be left unstable!" + exception.Message + exception.StackTrace);
            }
        }

        public static void DuplicatePlayerSettings(Pawn source, Pawn dest)
        {
            try
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
            catch (Exception exception)
            {
                Log.Warning("[ATR] An unexpected error occurred during player setting duplication between " + source + " " + dest + ". The destination PlayerSettings may be left unstable!" + exception.Message + exception.StackTrace);
            }
        }

        public static void PermutePawn(Pawn firstPawn, Pawn secondPawn)
        {
            try
            {
                if (firstPawn == null || secondPawn == null)
                    return;

                // Permute all major mind-related components to each other via a temp copy.
                PawnGenerationRequest request = new PawnGenerationRequest(PawnKindDefOf.Colonist, null, PawnGenerationContext.PlayerStarter, forceGenerateNewPawn: true);
                Pawn tempCopy = PawnGenerator.GeneratePawn(request);
                Duplicate(firstPawn, tempCopy, false, false);
                Duplicate(secondPawn, firstPawn, false, false);
                Duplicate(tempCopy, secondPawn, false, false);


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
            }
            catch(Exception e)
            {
                Log.Message("[ATR] Utils.PermutePawn : " + e.Message + " - " + e.StackTrace);
            }
        }

        // Check if the targetted pawn is a valid target for receiving mind transfer operations.
        public static bool IsValidMindTransferTarget(Pawn pawn)
        {
            // Only player pawns that are connected to the SkyMind, not suffering from a security breach, and not currently in a SkyMind operation are legal targets.
            if ((pawn.Faction != null && pawn.Faction != Faction.OfPlayer && pawn.HostFaction != Faction.OfPlayer) || !gameComp.HasSkyMindConnection(pawn) || pawn.GetComp<CompSkyMind>().Breached != -1 || pawn.GetComp<CompSkyMindLink>().Linked > -1)
            {
                return false;
            }

            // Pawns afflicted with a Hediff that prevents SkyMind connections, or who are already subjects of mind operations, are not permissible targets for mind operations.
            List<Hediff> targetHediffs = pawn.health.hediffSet.hediffs;
            for (int i = targetHediffs.Count - 1; i >= 0; i--)
            {
                Hediff hediff = targetHediffs[i];
                if (hediff.TryGetComp<HediffComp_SkyMindEffecter>()?.BlocksSkyMindConnection == true)
                {
                    return false;
                }
            }

            // If the pawn has a cloud capable implant or is in the SkyMind network already, then it is valid.
            return HasCloudCapableImplant(pawn);
        }

        // Returns a list of all surrogates without hosts in caravans. Return null if there are none.
        public static IEnumerable<Pawn> GetHostlessCaravanSurrogates()
        {
            // If surrogates aren't allowed, there can be no hostless surrogates.
            if (!ATReforged_Settings.surrogatesAllowed)
                return null;

            HashSet<Pawn> hostlessSurrogates = new HashSet<Pawn>();
            foreach (Caravan caravan in Find.World.worldObjects.Caravans)
            {
                foreach (Pawn pawn in caravan.pawns)
                {
                    if (IsSurrogate(pawn) && !pawn.GetComp<CompSkyMindLink>().HasSurrogate())
                    {
                        hostlessSurrogates.AddItem(pawn);
                    }
                }
            }
            return hostlessSurrogates.Count == 0 ? null : hostlessSurrogates;
        }

        // Create as close to a perfect copy of the provided pawn as possible. If kill is true, then we're trying to make a corpse copy of it.
        public static Pawn SpawnCopy(Pawn pawn, bool kill=true)
        {
            // Generate a new pawn.
            PawnGenerationRequest request = new PawnGenerationRequest(pawn.kindDef, faction: null, context: PawnGenerationContext.NonPlayer, canGeneratePawnRelations: false, fixedBiologicalAge: pawn.ageTracker.AgeBiologicalYearsFloat, fixedChronologicalAge: pawn.ageTracker.AgeChronologicalYearsFloat, fixedGender: pawn.gender);
            Pawn copy = PawnGenerator.GeneratePawn(request);

            // Gene generation is a bit strange, so we manually handle it ourselves.
            copy.genes = new Pawn_GeneTracker(copy);
            copy.genes.SetXenotypeDirect(pawn.genes?.Xenotype);
            foreach (Gene gene in pawn.genes?.Xenogenes)
            {
                copy.genes.AddGene(gene.def, true);
            }
            foreach (Gene gene in pawn.genes?.Endogenes)
            {
                copy.genes.AddGene(gene.def, false);
            }
            // Melanin is controlled via genes. If the pawn has one, use it. Otherwise just take whatever skinColorBase the pawn has.
            if (copy.genes?.GetMelaninGene() != null && pawn.genes?.GetMelaninGene() != null)
            {
                copy.genes.GetMelaninGene().skinColorBase = pawn.genes.GetMelaninGene().skinColorBase;
            }
            copy.story.skinColorOverride = pawn.story?.skinColorOverride;
            pawn.GetComp<AlienPartGenerator.AlienComp>()?.OverwriteColorChannel("skin", pawn.story.SkinColorBase);
            copy.story.SkinColorBase = pawn.story.SkinColorBase;

            // Get rid of any items it may have spawned with.
            copy.equipment?.DestroyAllEquipment();
            copy.apparel?.DestroyAll();
            copy.inventory?.DestroyAll();

            // Copy the pawn's physical attributes.
            copy.Rotation = pawn.Rotation;
            copy.story.bodyType = pawn.story.bodyType;
            copy.story.HairColor = pawn.story.HairColor;
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

            // Copy all hediffs from the pawn to the copy. Remove the hediff from the host to ensure it isn't saved across both pawns.
            copy.health.RemoveAllHediffs();
            foreach (Hediff hediff in pawn.health.hediffSet.hediffs.ToList())
            {
                try
                {
                    if (hediff.def != HediffDefOf.MissingBodyPart && hediff.def != ATR_HediffDefOf.ATR_MindOperation)
                    {
                        hediff.pawn = copy;
                        copy.health.AddHediff(hediff, hediff.Part);
                        pawn.health.RemoveHediff(hediff);
                    }
                }
                catch(Exception ex)
                {
                    Log.Error("[ATR] Utils.SpawnCopy.TransferHediffs " + ex.Message + " " + ex.StackTrace);
                }
            }

            // If we are "killing" the pawn, that means the body is now a blank. Properly duplicate those features.
            if (kill)
            {
                KillPawnIntelligence(copy);
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
            // Assign base cost based on settings. Default is 5000.
            float result = ATReforged_Settings.basePointsNeededForPassion;

            // Multiply result by the pawn's global learning factor (inverse relation, as higher learning factor should reduce cost).
            result *= 1 / pawn.GetStatValue(StatDef.Named("GlobalLearningFactor"));

            if (passionCount > ATReforged_Settings.passionSoftCap)
            { // If over the soft cap for number of passions, each additional passion adds 25% cost to buying another passion.
                result *= (float) Math.Pow(1.25, passionCount - ATReforged_Settings.passionSoftCap);
            }

            // Return the end result as an integer for nice display numbers and costs.
            return (int) result;
        }

        // Handle the "killing" of a pawn's intelligence by duplicating in a blank and handling organic vs. mechanical cases.
        public static void KillPawnIntelligence(Pawn pawn)
        {
            Duplicate(GetBlank(), pawn, false, false);

            // Killed SkyMind intelligences cease to exist.
            if (gameComp.GetCloudPawns().Contains(pawn))
            {
                gameComp.PopCloudPawn(pawn);
                pawn.Destroy();
            }

            // Androids that become blanks should also lose their interface (if they have them) so that they're ready for a new intelligence.
            if (IsConsideredMechanicalAndroid(pawn) && pawn.def.GetModExtension<ATR_MechTweaker>()?.needsCoreAsAndroid == true)
            {
                pawn.health.AddHediff(ATR_HediffDefOf.ATR_IsolatedCore, pawn.health.hediffSet.GetBrain());
                Hediff target = pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_AutonomousCore);
                if (target != null)
                {
                    pawn.health.RemoveHediff(target);
                }
                target = pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_ReceiverCore);
                if (target != null)
                {
                    pawn.health.RemoveHediff(target);
                }
                pawn.guest?.SetGuestStatus(Faction.OfPlayer);
                if (pawn.playerSettings != null)
                    pawn.playerSettings.medCare = MedicalCareCategory.Best;
            }
            // Pawns that aren't core-using androids can not truly become blanks as they have no Core Hediff to affect. Instead, make them into a simple new pawn.
            else
            {
                // Ensure the pawn has a proper name.
                pawn.Name = PawnBioAndNameGenerator.GeneratePawnName(pawn);

                // Ensure the pawn has no SkyMind capable implants any more.
                List<Hediff> pawnHediffs = pawn.health.hediffSet.hediffs;
                for (int i = pawnHediffs.Count - 1; i >= 0; i--)
                {
                    if (pawnHediffs[i].TryGetComp<HediffComp_SkyMindEffecter>()?.AllowsSkyMindConnection == true)
                    {
                        pawnHediffs.RemoveAt(i);
                    }
                }
            }
        }

        // Handle various parts of resetting drones to default status.
        public static void ReconfigureDrone(Pawn pawn)
        {
            ATR_MechTweaker pawnExtension = pawn.def.GetModExtension<ATR_MechTweaker>();
            if (pawnExtension?.dronesCanHaveTraits == false)
            {
                foreach (Trait trait in pawn.story.traits.allTraits.ToList())
                {
                    pawn.story.traits.RemoveTrait(trait);
                }
            }

            // Drones don't have ideos.
            pawn.ideo = null;

            // Drones always take their last name (ID #) as their nickname.
            if (pawn.Name is NameTriple name)
            {
                pawn.Name = new NameTriple(name.First, name.Last, name.Last);
            }

            // Drones have a set skill, which is taken from their mod extension if it exists. If not, it defaults to 8 (which is the default value for the extension).
            // Since drones are incapable of learning, their passions and xp does not matter. Set it to 0 for consistency's sake.
            int skillLevel = pawnExtension?.droneSkillLevel ?? 8;
            foreach (SkillRecord skillRecord in pawn.skills.skills)
            {
                skillRecord.passion = 0;
                skillRecord.Level = skillLevel;
                skillRecord.xpSinceLastLevel = 0;
            }

            // If pawn kind backstories should be overwritten, then try to take it from the mod extension. If it does not exist, it defaults to ATR_MechChildhoodDrone (which is default for the extension).
            if (pawnExtension?.letPawnKindHandleDroneBackstories == false)
            {
                pawn.story.Childhood = pawnExtension?.droneChildhoodBackstoryDef ?? ATR_BackstoryDefOf.ATR_MechChildhoodDrone;
                pawn.story.Adulthood = pawnExtension?.droneAdulthoodBackstoryDef ?? ATR_BackstoryDefOf.ATR_MechAdulthoodDrone;
                pawn.workSettings.Notify_DisabledWorkTypesChanged();
                pawn.skills.Notify_SkillDisablesChanged();
            }
        }

        // Caches and returns the HediffDefs contained in the HediffGiver of a given RaceProperties.
        public static HashSet<HediffDef> GetTemperatureHediffDefsForRace(RaceProperties raceProperties)
        {
            if (raceProperties == null)
            {
                Log.Error("[ATR] A pawn with null raceProperties tried to have those properties checked!");
                return new HashSet<HediffDef>();
            }

            try
            {
                if (!cachedTemperatureHediffs.ContainsKey(raceProperties))
                {
                    List<HediffGiverSetDef> hediffGiverSetDefs = raceProperties.hediffGiverSets;
                    HashSet<HediffDef> targetHediffs = new HashSet<HediffDef>();

                    if (hediffGiverSetDefs == null)
                    {
                        cachedTemperatureHediffs[raceProperties] = targetHediffs;
                        return targetHediffs;
                    }

                    foreach (HediffGiverSetDef hediffGiverSetDef in hediffGiverSetDefs)
                    {
                        foreach (HediffGiver hediffGiver in hediffGiverSetDef.hediffGivers)
                        {
                            if (typeof(HediffGiver_Heat).IsAssignableFrom(hediffGiver.GetType()) || typeof(HediffGiver_Hypothermia).IsAssignableFrom(hediffGiver.GetType()))
                            {
                                targetHediffs.Add(hediffGiver.hediff);
                            }
                        }
                    }
                    cachedTemperatureHediffs[raceProperties] = targetHediffs;
                }
                return cachedTemperatureHediffs[raceProperties];
            }
            catch (Exception ex)
            {
                Log.Warning("[ATR] Encountered an error while trying to get temperature HediffDefs. Using default behavior." + ex.Message + ex.StackTrace);
                return new HashSet<HediffDef>();
            }
        }

        // Cached Hediffs for a particular pawn's race that count as temperature hediffs to avoid recalculation, cached when needed.
        private static Dictionary<RaceProperties, HashSet<HediffDef>> cachedTemperatureHediffs = new Dictionary<RaceProperties, HashSet<HediffDef>>();
    }
}
