using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI.Group;
using Verse.AI;
using UnityEngine;
using RimWorld.Planet;
using HarmonyLib;
using System.Reflection;
using Verse.Sound;
using System.Security.Cryptography;

namespace ATReforged
{
    public class GC_ATPP : GameComponent
    {

        public GC_ATPP(Game game)
        {
            Utils.GCATPP = this;
            initNull();
            
            try
            {
                Utils.SkyMindAttackWitnessDef = DefDatabase<ThoughtDef>.GetNamed("ATR_ConnectedSkyMindAttacked");
                Utils.SkyMindAttackVictimDef = DefDatabase<ThoughtDef>.GetNamed("ATR_AttackedViaSkyMind");
                Utils.SkyMindTrollVictimDef = DefDatabase<ThoughtDef>.GetNamed("ATR_TrolledViaSkyMind");

                Utils.ATR_FullChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_FullChargeFleck");
                Utils.ATR_HalfChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_HalfChargeFleck");
                Utils.ATR_EmptyChargeFleck = DefDatabase<FleckDef>.GetNamed("ATR_EmptyChargeFleck");

                RecipeDef androidDisassembly = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseMechanoid");
                RecipeDef androidSmashing = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseMechanoid");
                androidDisassembly.fixedIngredientFilter.SetDisallowAll();
                RecipeDef butcherFlesh = DefDatabase<RecipeDef>.GetNamed("ButcherCorpseFlesh");

                // RunTime patching
                foreach (ThingDef thingDef in DefDatabase<ThingDef>.AllDefsListForReading)
                {
                    try
                    {
                        // Check race to see if the thingDef is for a Pawn.
                        if (thingDef != null && thingDef.race != null)
                        {
                            // Humanlike non-drone pawns get SkyMind comps.
                            if (thingDef.race.intelligence == Intelligence.Humanlike && !Utils.IsConsideredMechanicalDrone(thingDef) && !Utils.HasSpecialStatus(thingDef))
                            {
                                CompProperties cp;
                                cp = new CompProperties
                                {
                                    compClass = typeof(CompSkyMind)
                                };
                                thingDef.comps.Add(cp);

                                cp = new CompProperties
                                {
                                    compClass = typeof(CompSkyMindLink)
                                };
                                thingDef.comps.Add(cp);
                            }
                            
                            // Mechanical pawns do not need rest or get butchered. Patch the defs at run-time because settings may change.
                            if (Utils.IsConsideredMechanical(thingDef))
                            {
                                androidDisassembly.fixedIngredientFilter.SetAllow(thingDef.race.corpseDef, true);
                                androidSmashing.fixedIngredientFilter.SetAllow(thingDef.race.corpseDef, true);
                                butcherFlesh.fixedIngredientFilter.SetAllow(thingDef.race.corpseDef, false);
                                thingDef.race.needsRest = false;
                            }

                            // Drones do not have learning factors.
                            if (Utils.IsConsideredMechanicalDrone(thingDef))
                            {
                                StatModifier learningModifier = thingDef.statBases.Find(modifier => modifier.stat.defName == "GlobalLearningFactor");
                                if (learningModifier != null)
                                {
                                    learningModifier.value = 0;
                                }
                                else
                                {
                                    thingDef.statBases.Add(new StatModifier() { stat = RimWorld.StatDefOf.GlobalLearningFactor, value = 0 });
                                }
                            }

                            // Ensure charge-capable units have the Battery Comp.
                            if (Utils.CanUseBattery(thingDef))
                            {
                                thingDef.comps.Add(new CompProperties { compClass = typeof(CompPawnBattery) });
                            }
                            // Ensure non-mechanical units do not have the comp - they may have one if the game is reloaded after switching their status.
                            else
                            {
                                thingDef.comps.RemoveAll(compProperties => compProperties.compClass == typeof(CompPawnBattery));
                            }
                        }
                        else
                        {
                            if (thingDef.IsDoor && thingDef.HasComp(typeof(CompPowerTrader)))
                            {
                                CompProperties cp = new CompProperties
                                {
                                    compClass = typeof(CompAutoDoor)
                                };
                                thingDef.comps.Add(cp);

                                //SkyMind
                                cp = new CompProperties
                                {
                                    compClass = typeof(CompSkyMind)
                                };
                                thingDef.comps.Add(cp);
                            }
                            // Add the CompSkyMind to all powered and flickable structures that aren't SkyMind Cores (no need for one).
                            else if (thingDef.comps != null && !thingDef.HasComp(typeof(CompSkyMindCore)))
                            {
                                bool found = false;
                                bool flickable = false;

                                foreach (CompProperties compProp in thingDef.comps)
                                {
                                    if (compProp.compClass == null)
                                        continue;

                                    if (compProp.compClass == typeof(CompFlickable))
                                        flickable = true;
                                    else if (compProp.compClass == typeof(CompPowerTrader) || (compProp.compClass == typeof(CompPowerPlant) || compProp.compClass.IsSubclassOf(typeof(CompPowerPlant))))
                                    {
                                        found = true;
                                    }
                                }

                                if (found && flickable)
                                {
                                    CompProperties cp = new CompProperties
                                    {
                                        compClass = typeof(CompSkyMind)
                                    };
                                    thingDef.comps.Add(cp);
                                }
                            }
                            else if (thingDef.IsCorpse)
                            {
                                Log.Warning("[ATR DEBUG] Located thingDef " + thingDef.defName + " as a corpse! It's category is " + thingDef.category.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Log.Message("[ATR] Runtime.Patching.Comps " + e.Message + " " + e.StackTrace);
                    }
                }

                try
                {
                    // Go through all PawnKindDefs to locate if anything has "SurrogateSoldier" as a BackstoryFiltersOverride. These are our valid surrogates so add it to a list.
                    foreach (PawnKindDef entry in DefDatabase<PawnKindDef>.AllDefsListForReading)
                    {
                        if (entry.backstoryFiltersOverride != null)
                        {
                            foreach (BackstoryCategoryFilter backstoryFilter in entry.backstoryFiltersOverride)
                            {
                                if (backstoryFilter.categories != null && backstoryFilter.categories.Contains("SurrogateSoldier"))
                                {
                                    Utils.ValidSurrogatePawnKindDefs.Add(entry);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning("[ATR] Failed to generate valid Surrogates,  " + ex.Message + " " + ex.StackTrace);
                }
            }
            catch (Exception e)
            {
                Log.Error("[ATPP] GC_ATPP.CTOR(Init)  Fatal Error : " + e.Message + " - " + e.StackTrace);
            }
        }



        private void RemoveBlacklistedAndroidsHediffs()
        {
            bool BlacklistedHediff(Hediff hediff)
            {
                return Utils.BlacklistedHediffsForAndroids.Contains(hediff.def.defName);
            }

            foreach(var map in Find.Maps)
            {
                foreach(var pawn in map.mapPawns.AllPawns)
                {
                    if (Utils.IsConsideredMechanical(pawn) && pawn.health != null && pawn.health.hediffSet != null)
                    {
                        foreach (Hediff hediff in pawn.health.hediffSet.hediffs.Where(hediff => BlacklistedHediff(hediff)))
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                }
            }
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();

            initNull();
            CheckDeleteAndroidFactions();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();

            RemoveBlacklistedAndroidsHediffs();
            CheckDeleteAndroidFactions();
        }

        public override void ExposeData()
        {
            base.ExposeData();

            Scribe_Values.Look(ref skillPointCapacity, "ATR_skillPointCapacity", 0);
            Scribe_Values.Look(ref skillPoints, "ATR_skillPoints", 0);
            Scribe_Values.Look(ref securityPointCapacity, "ATR_securityPointCapacity", 0);
            Scribe_Values.Look(ref securityPoints, "ATR_securityPoints", 0);
            Scribe_Values.Look(ref hackingPointCapacity, "ATR_hackingPointCapacity", 0);
            Scribe_Values.Look(ref hackingPoints, "ATR_hackingPoints", 0);
            Scribe_Values.Look(ref SkyMindNetworkCapacity, "ATR_SkyMindNetworkCapacity", 0);
            Scribe_Values.Look(ref SkyMindCloudCapacity, "ATR_SkyMindCloudCapacity", 0);

            List<Thing> thingKeyCopy = virusedDevices.Keys.FastToList();
            List<int> thingValueCopy = virusedDevices.Values.FastToList();
            List<Pawn> pawnKeyCopy = networkLinkedPawns.Keys.FastToList();
            List<int> pawnValueCopy = networkLinkedPawns.Values.FastToList();

            Scribe_Collections.Look(ref skillServers, "ATR_skillServers", LookMode.Reference);
            Scribe_Collections.Look(ref securityServers, "ATR_securityServers", LookMode.Reference);
            Scribe_Collections.Look(ref hackingServers, "ATR_hackingServers", LookMode.Reference);
            Scribe_Collections.Look(ref networkedDevices, "ATR_networkedDevices", LookMode.Reference);
            Scribe_Collections.Look(ref cloudPawns, "ATR_cloudPawns", LookMode.Reference);
            Scribe_Collections.Look(ref chargingStations, "ATR_chargingStations", LookMode.Reference);
            Scribe_Collections.Look(ref heatSensitiveDevices, "ATR_heatSensitiveDevices", LookMode.Reference);
            Scribe_Collections.Look(ref virusedDevices, "ATR_virusedDevices", LookMode.Reference, LookMode.Value, ref thingKeyCopy, ref thingValueCopy);
            Scribe_Collections.Look(ref networkLinkedPawns, "ATR_networkLinkedPawns", LookMode.Reference, LookMode.Value, ref pawnKeyCopy, ref pawnValueCopy);


            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                initNull();
            }
        }

        public override void GameComponentTick()
        {
            base.GameComponentTick();

            int CGT = Find.TickManager.TicksGame;

            // Check virus and mind operation timers every 10 seconds.
            if(CGT % 600 == 0)
            {
                CheckVirusedThings();
                CheckNetworkLinkedPawns();
                CheckServers();
            }
        }

        // Check to see if any virused things have elapsed their infected timers. Remove viruses that have elapsed.
        public void CheckVirusedThings()
        {
            if (virusedDevices.Count == 0)
                return;

            int GT = Find.TickManager.TicksGame;

            // We make it into a list here so that it stores a copy. If we didn't, the list may change size as virusedDevices thins while it is still running, throwing an exception.
            foreach (KeyValuePair<Thing, int> virusedDevice in virusedDevices.ToList())
            {
                // If for whatever reason the device has no CompSkyMind, pass over it.
                CompSkyMind csm = virusedDevice.Key.TryGetComp<CompSkyMind>();
                if (csm == null)
                    continue;

                if (virusedDevice.Value != -1 && virusedDevice.Value <= GT )
                { // Timer has expired. Inform it that it is no longer virused. The CompSkyMind will handle the rest.
                    csm.Breached = -1;
                }
            }
        }

        // Check to see if any SkyMind operations have elapsed their timers. Remove operations that have elapsed and handle any interrupts.
        public void CheckNetworkLinkedPawns()
        {
            if (networkLinkedPawns.Count == 0)
                return;

            int GT = Find.TickManager.TicksGame;

            // We make it into a list here so that it stores a copy. If we didn't, the list may change size as networkedLinkedPawns thins while it is still running, throwing an exception.
            foreach (KeyValuePair<Pawn, int> networkLinkedPawn in networkLinkedPawns.ToList())
            {
                // If for whatever reason the device has no CompSkyMindLink, pass over it.
                CompSkyMindLink cso = networkLinkedPawn.Key.TryGetComp<CompSkyMindLink>();
                if (cso == null)
                    continue;

                if (networkLinkedPawn.Value != -1 && networkLinkedPawn.Value <= GT)
                { // Timer has expired. Inform it that it the SkyMind operation has ended. The CompSurrogateOwner will handle the rest.
                    cso.Linked = -1;
                }

                // Check to see if the operation has been interrupted for any reason.
                cso.CheckInterruptedUpload();
            }
        }

        // Tally up points from active servers. Servers add/remove themselves from the set as appropriate so there is no reason to check them here.
        public void CheckServers()
        {
            // Servers may either be computerComps or superComputerComps. If not compComputer, check compSuperComputer.
            CompComputer computerComp;

            // Skill Servers
            int changeAmount = 0;
            foreach (Building skillServer in skillServers)
            {
                computerComp = skillServer.TryGetComp<CompComputer>();
                if (computerComp != null)
                    changeAmount += computerComp.Props.passivePointGeneration;
                else
                    changeAmount += skillServer.TryGetComp<CompSuperComputer>().Props.passivePointGeneration;
            }
            ChangeServerPoints(changeAmount, ServerType.SkillServer);

            // Security Servers
            changeAmount = 0;
            foreach (Building securityServer in securityServers)
            {
                computerComp = securityServer.TryGetComp<CompComputer>();
                if (computerComp != null)
                    changeAmount += computerComp.Props.passivePointGeneration;
                else
                    changeAmount += securityServer.TryGetComp<CompSuperComputer>().Props.passivePointGeneration;
            }
            ChangeServerPoints(changeAmount, ServerType.SecurityServer);

            // Hacking Servers
            changeAmount = 0;
            foreach (Building hackingServer in hackingServers)
            {
                computerComp = hackingServer.TryGetComp<CompComputer>();
                if (computerComp != null)
                    changeAmount += computerComp.Props.passivePointGeneration;
                else
                    changeAmount += hackingServer.TryGetComp<CompSuperComputer>().Props.passivePointGeneration;
            }
            ChangeServerPoints(changeAmount, ServerType.HackingServer);
        }

        public int GetSkillPointCapacity()
        {
            return skillPointCapacity;
        }
        public int GetSecurityPointCapacity()
        {
            return securityPointCapacity;
        }
        public int GetHackingPointCapacity()
        {
            return hackingPointCapacity;
        }


        public int GetSkillPoints()
        {
            return skillPoints;
        }
        public int GetSecurityPoints()
        {
            return securityPoints;
        }
        public int GetHackingPoints()
        {
            return hackingPoints;
        }

        // Determine if the provided thing is connected to the SkyMind Network.
        public bool HasSkyMindConnection(Thing thing)
        {
            return networkedDevices.Contains(thing);
        }

        // Determine if the provided pawn is connected to the SkyMind Network.
        public bool HasSkyMindConnection (Pawn pawn)
        {
            return networkedDevices.Contains(pawn) || cloudPawns.Contains(pawn);
        }

        // Attempt to connect the provided thing to the SkyMind network.
        public bool AttemptSkyMindConnection(Thing thing)
        {
            // If the thing is already connected, return true.
            if (networkedDevices.Contains(thing))
            {
                return true;
            }

            // If there is no available space in the network, return false. No connection is made.
            if (networkedDevices.Count() >= SkyMindNetworkCapacity)
            {
                return false;
            }

            // Add the device to a list of devices in the SkyMind.
            networkedDevices.Add(thing);

            // Inform the comps of the pawn that the user successfully connected. This configures details like the SkyMind assistance hediff and a few bools.
            ((ThingWithComps)thing).BroadcastCompSignal("SkyMindNetworkUserConnected");
            return true;
        }

        // Disconnect the provided thing from the SkyMind network. This does nothing if it wasn't connected already.
        public void DisconnectFromSkyMind(Thing thing)
        {
            if (networkedDevices.Contains(thing))
            {
                // Remove the device from the network.
                networkedDevices.Remove(thing);
                // Inform all comps that the thing is no longer connected to the SkyMind network. If this was a manual disconnection, inform it appropriately.
                ((ThingWithComps)thing).BroadcastCompSignal("SkyMindNetworkUserDisconnected");
            }
        }

        public HashSet<Thing> GetSkyMindDevices()
        {
            return networkedDevices;
        }

        // Add the SkyMind Tower's capacity to the current total.
        public void AddTower(CompSkyMindTower tower)
        {
            SkyMindNetworkCapacity += tower.Props.SkyMindSlotsProvided;
        }

        // Remove the SkyMind Tower's capacity from the current total.
        public void RemoveTower(CompSkyMindTower tower)
        {
            SkyMindNetworkCapacity -= tower.Props.SkyMindSlotsProvided;

            // Removing a tower may result in being over the SkyMind network limit. Randomly disconnect some until under the limit if necessary.
            while (SkyMindNetworkCapacity < networkedDevices.Count())
            {
                Thing device = networkedDevices.RandomElement();
                DisconnectFromSkyMind(device);
            }
        }

        // Return the current total capacity of SkyMind Towers currently active.
        public int GetSkyMindNetworkSlots()
        {
            return SkyMindNetworkCapacity;
        }

        // Add a SkyMind Core that generates cloudPawn capacity to the appropriate set for later use.
        public void AddCore(CompSkyMindCore core)
        {
            SkyMindCloudCapacity += core.Props.cloudPawnCapacityProvided;
        }

        // Remove a SkyMind Core that generates cloudPawn capacity from the appropriate set.
        public void RemoveCore(CompSkyMindCore core)
        {
            SkyMindCloudCapacity -= core.Props.cloudPawnCapacityProvided;

            // Removing a core may result in being over the cloud pawn limit. Randomly murder stored intelligences until under the limit if necessary.
            while (SkyMindCloudCapacity < cloudPawns.Count())
            {
                // Killing the pawn will automatically handle any interrupted mind operations or surrogate connections. TODO: Ensure this is the case.
                cloudPawns.RandomElement().Kill(null);
            }
        }

        // Return the maximum number of cloud pawns that may be stored in the SkyMind network currently.
        public int GetSkyMindCloudCapacity()
        {
            return SkyMindCloudCapacity;
        }

        // Add a charging station to the set. No errors are thrown if it was in the set already.
        public void PushChargingStation(Building build)
        {
            chargingStations.Add(build);
        }

        // Remove a charging station from the set. No errors are thrown if it wasn't in the set.
        public void PopChargingStation(Building build)
        {
            chargingStations.Remove(build);
        }

        // Returns the closest available Charging Station on the pawn's map.
        public Building GetClosestFreeChargingStation(Map map, Pawn pawn)
        {
            // Acquire all charging stations on the pawn's map in order from closest to furthest, and iterate through them to find the first free one.
            foreach (Building station in chargingStations.Where(building => building.Map == map).OrderBy(building => building.Position.DistanceToSquared(pawn.Position)))
            {
                if (station == null || station.Destroyed || station.IsBrokenDown() || !station.TryGetComp<CompPowerTrader>().PowerOn || !station.Position.InAllowedArea(pawn))
                    continue;

                // Try to get a free spot at this station. If there is a free spot and the pawn can reserve it, then return this station.
                IntVec3 freePlace = station.TryGetComp<CompChargingStation>().GetOpenRechargeSpot(pawn);
                if (freePlace != IntVec3.Invalid && pawn.CanReach(freePlace, PathEndMode.OnCell, Danger.Deadly))
                    return station;
            }
            
            return null;
        }

        // Add a heat sensitive device into the proper set. No errors are thrown if it was already in the set.
        public void PushHeatSensitiveDevice(Building build)
        {
            heatSensitiveDevices.Add(build);
        }

        // Remove a heat sensitive device from the proper set. No errors are thrown if it was not in the set.
        public void PopHeatSensitiveDevice(Building build)
        {
            heatSensitiveDevices.Remove(build);
        }

        // Return all heat sensitive devices on a given map. If a map isn't provided, return all heat sensitive devices across all maps.
        public IEnumerable<Thing> GetHeatSensitiveDevices(Map map = null)
        {
            return heatSensitiveDevices.Where(device => map != null ? device.Map == map : device != null);
        }

        // Destroy all android factions/bases if they are disabled in settings.
        public void CheckDeleteAndroidFactions()
        {
            // If the Raider faction is blacklisted, destroy all bases relating to them.
            if (!ATReforged_Settings.androidRaidersExist)
            {
                Log.Message("[ATR] Android Raider faction blacklisted. Removing bases and faction.");
                Find.WorldObjects.SettlementBases.RemoveAll(settlement => settlement.Faction.def == FactionDefOf.MechanicalMarauders);

                foreach (Faction faction in Find.FactionManager.GetFactions(minTechLevel: TechLevel.Industrial).Where(faction => faction.def == FactionDefOf.MechanicalMarauders))
                {
                    faction.defeated = true;
                    faction.def.hidden = true;
                }
            }

            // If the Union faction is blacklisted, destroy all bases relating to them.
            if (!ATReforged_Settings.androidUnionistsExist)
            {
                Log.Message("[ATR] Android Union faction blacklisted. Removing bases and faction.");
                Find.WorldObjects.SettlementBases.RemoveAll(settlement => settlement.Faction.def == FactionDefOf.AndroidUnion);

                foreach (Faction faction in Find.FactionManager.GetFactions(minTechLevel: TechLevel.Industrial).Where(faction => faction.def == FactionDefOf.AndroidUnion))
                {
                    faction.defeated = true;
                    faction.def.hidden = true;
                }

            }
        }

        // Add a server to the appropriate list based on serverMode
        public void AddServer(Building building, ServerType serverMode, int capacity)
        { 
            switch (serverMode)
            {
                case ServerType.None:
                    return;
                case ServerType.SkillServer:
                    skillServers.Add(building);
                    skillPointCapacity += capacity;
                    break;
                case ServerType.SecurityServer:
                    securityServers.Add(building);
                    securityPointCapacity += capacity;
                    break;
                case ServerType.HackingServer:
                    hackingServers.Add(building);
                    hackingPointCapacity += capacity;
                    break;
                default:
                    Log.Error("[ATR] GC_ATPP.AddServer was given an invalid serverType. No servers added.");
                    return;
            }
        }

        // If a capacity is provided instead of a serverMode, assume this capacity is to be added to all servers
        public void AddServer(Building building, int capacity)
        { 
            skillServers.Add(building);
            skillPointCapacity += capacity;

            securityServers.Add(building);
            securityPointCapacity += capacity;

            hackingServers.Add(building);
            hackingPointCapacity += capacity;
        }

        // Remove the building from the task group it is assigned to.
        public void RemoveServer(Building building, ServerType serverMode, int capacity)
        {
            switch (serverMode)
            {
                case ServerType.None: // None Type has no server list nor a point capacity. 
                    return;
                case ServerType.SkillServer: // Remove from skill servers list.
                    skillServers.Remove(building);
                    skillPointCapacity -= capacity;
                    break;
                case ServerType.SecurityServer: // Remove from security servers list.
                    securityServers.Remove(building);
                    securityPointCapacity -= capacity;
                    break;
                case ServerType.HackingServer: // Remove from hacking servers list.
                    hackingServers.Remove(building);
                    hackingPointCapacity -= capacity;
                    break;
                default: // Illegal server type results in no changes as it doesn't know what to change.
                    Log.Error("[ATR] GC_ATTP.RemoveServer was given an invalid serverType. No servers removed.");
                    return;
            }
        }

        // If a capacity is provided instead of a serverMode, assume this capacity is to removed from all servers
        public void RemoveServer(Building building, int capacity)
        { 
            skillServers.Remove(building);
            skillPointCapacity -= capacity;

            securityServers.Remove(building);
            securityPointCapacity -= capacity;

            hackingServers.Remove(building);
            hackingPointCapacity -= capacity;
        }

        // This always adds the points to the appropriate category. It assumes negative changes are given in the parameter. It also handles illegal types (do nothing).
        // It handles numbers going out of bounds by ensuring it doesn't drop below 0 or go above the capacity.
        public void ChangeServerPoints(int toChange, ServerType serverMode)
        {
            switch (serverMode)
            {
                case ServerType.None:
                    Log.Error("[ATR] Can't add points to a None server type! No points changed.");
                    return;
                case ServerType.SkillServer:
                    skillPoints += toChange;
                    skillPoints = Mathf.Clamp(skillPoints, 0, skillPointCapacity);
                    break;
                case ServerType.SecurityServer:
                    securityPoints += toChange;
                    securityPoints = Mathf.Clamp(securityPoints, 0, securityPointCapacity);
                    break;
                case ServerType.HackingServer:
                    hackingPoints += toChange;
                    hackingPoints = Mathf.Clamp(hackingPoints, 0, hackingPointCapacity);
                    break;
            }
        }

        // Add a new virused thing to the dictionary. If it was already contained there, update the endTick. If it wasn't, add it in with the key and endTick provided.
        public void PushVirusedThing(Thing thing, int endTick)
        {
            if (virusedDevices.ContainsKey(thing))
            {
                virusedDevices[thing] = endTick;
            }
            else
            {
                virusedDevices.Add(thing, endTick);
            }
        }

        // Remove a virused thing from the dictionary. Nothing happens if it already wasn't contained.
        public void PopVirusedThing(Thing thing)
        {
            if (virusedDevices.ContainsKey(thing))
                virusedDevices.Remove(thing);
        }

        // Get the virus end timer for the given device. Return -1 if it wasn't found.
        public int GetVirusedDevice(Thing device)
        {
            if (!virusedDevices.ContainsKey(device))
                return -1;
            return virusedDevices[device];
        }

        // Return all virused devices. Keys are the virused devices themselves, the ints represent how long until they are released.
        public Dictionary<Thing, int> GetAllVirusedDevices()
        {
            return virusedDevices;
        }

        // Add a new network linked pawn to the dictionary. If it was already contained there, update the endTick. If it wasn't, add it in with the key and endTick provided.
        public void PushNetworkLinkedPawn(Pawn pawn, int endTick)
        {
            if (networkLinkedPawns.ContainsKey(pawn))
            {
                networkLinkedPawns[pawn] = endTick;
            }
            else
            {
                networkLinkedPawns.Add(pawn, endTick);
            }
        }

        // Remove a network linked pawn from the dictionary. Nothing happens if it already wasn't contained.
        public void PopNetworkLinkedPawn(Pawn pawn)
        {
            if (networkLinkedPawns.ContainsKey(pawn))
                networkLinkedPawns.Remove(pawn);
        }

        // Get the mind operation end timer for the given device. Return -2 if it wasn't found.
        public int GetLinkedPawn(Pawn device)
        {
            if (!networkLinkedPawns.ContainsKey(device))
                return -2;
            return networkLinkedPawns[device];
        }

        // Return all virused devices. Keys are the virused devices themselves, the ints represent how long until they are released.
        public Dictionary<Pawn, int> GetAllLinkedPawns()
        {
            return networkLinkedPawns;
        }

        // Add a new pawn to the cloud set. If it was already contained there, do nothing.
        public void PushCloudPawn(Pawn pawn)
        {
            if (!cloudPawns.Contains(pawn))
            {
                cloudPawns.Add(pawn);
            }
        }

        // Remove a pawn from the cloud set. Nothing happens if it already wasn't contained.
        public void PopCloudPawn(Pawn pawn)
        {
            if (cloudPawns.Contains(pawn))
                cloudPawns.Remove(pawn);
        }

        // Return all of the things in the cloud set.
        public HashSet<Pawn> GetCloudPawns()
        {
            return cloudPawns;
        }
        
        private void initNull()
        { 
            if (chargingStations == null)
                chargingStations = new HashSet<Building>();
            if (networkedDevices == null)
                networkedDevices = new HashSet<Thing>();
            if (heatSensitiveDevices == null)
                heatSensitiveDevices = new HashSet<Thing>();
            if (skillServers == null)
                skillServers = new HashSet<Building>();
            if (securityServers == null)
                securityServers = new HashSet<Building>();
            if (hackingServers == null)
                hackingServers = new HashSet<Building>();
            if (virusedDevices == null)
                virusedDevices = new Dictionary<Thing, int>();
            if (networkLinkedPawns == null)
                networkLinkedPawns = new Dictionary<Pawn, int>();
            if (cloudPawns == null)
                cloudPawns = new HashSet<Pawn>();
        }

        // Ints for storing capacities of various comps. Servers also have current point values.
        private int skillPointCapacity = 0;
        private int skillPoints = 0;
        private int securityPointCapacity = 0;
        private int securityPoints = 0;
        private int hackingPointCapacity = 0;
        private int hackingPoints = 0;
        private int SkyMindNetworkCapacity = 0;
        private int SkyMindCloudCapacity = 0;

        // Networked devices are things that are connected to the SkyMind network, including free pawns, surrogates, and buildings.
        public HashSet<Thing> networkedDevices = new HashSet<Thing>();

        // Cloud Pawns are pawns that are stored in the SkyMind Network. This is important as their gizmo's are inaccessible and can't be connected to the SkyMind (but should be considered as if they are).
        private HashSet<Pawn> cloudPawns = new HashSet<Pawn>();

        // Servers have 3 different states they may be in, and may be saved/changed independently of each other. They must also be saved so points may be generated.
        private HashSet<Building> skillServers = new HashSet<Building>();
        private HashSet<Building> securityServers = new HashSet<Building>();
        private HashSet<Building> hackingServers = new HashSet<Building>();

        // Charging Stations are buildings that have CompReloadStation. We store them so that they can be checked for pawn's needing energy in a much easier and cheaper search.
        private HashSet<Building> chargingStations = new HashSet<Building>();

        // Dictionary mapping maps to heat sensitive devices in them for the purpose of checking their heat levels for alerts.
        private HashSet<Thing> heatSensitiveDevices = new HashSet<Thing>();

        // Virused devices are things with their values being the tick at which to release them. This avoids the CompSkyMind having to store this information.
        private Dictionary<Thing, int> virusedDevices = new Dictionary<Thing, int>();

        // Network Linked devices are things (pawns) that are currently undergoing some sort of SkyMind procedure like uploading, duplicating, etc. The value stores the tick at which to release them.
        private Dictionary<Pawn, int> networkLinkedPawns = new Dictionary<Pawn, int>();
    }
}