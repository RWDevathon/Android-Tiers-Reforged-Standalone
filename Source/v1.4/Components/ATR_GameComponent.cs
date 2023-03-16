using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace ATReforged
{
    public class ATR_GameComponent : GameComponent
    {

        public ATR_GameComponent(Game game)
        {
            Utils.gameComp = this;
            AllocateIfNull();
        }

        public override void StartedNewGame()
        {
            base.StartedNewGame();
            
            AllocateIfNull();
        }

        public override void LoadedGame()
        {
            base.LoadedGame();
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
            Scribe_Values.Look(ref hackCostTimePenalty, "ATR_hackCostTimePenalty", 0);
            Scribe_Values.Look(ref cachedSkillGeneration, "ATR_cachedSkillGeneration", 0);
            Scribe_Values.Look(ref cachedSecurityGeneration, "ATR_cachedSecurityGeneration", 0);
            Scribe_Values.Look(ref cachedHackingGeneration, "ATR_cachedHackingGeneration", 0);
            Scribe_Values.Look(ref hasBuiltAndroid, "ATR_hasBuiltAndroid", false);
            Scribe_Values.Look(ref hasBuiltDrone, "ATR_hasBuiltDrone", false);
            Scribe_Values.Look(ref hasImprisonedDrone, "ATR_hasImprisonedDrone", false);
            Scribe_Values.Look(ref hasMadeSurrogate, "ATR_hasMadeSurrogate", false);

            Scribe_Deep.Look(ref blankPawn, "ATR_blankPawn");

            Scribe_Collections.Look(ref skillServers, "ATR_skillServers", LookMode.Reference);
            Scribe_Collections.Look(ref securityServers, "ATR_securityServers", LookMode.Reference);
            Scribe_Collections.Look(ref hackingServers, "ATR_hackingServers", LookMode.Reference);
            Scribe_Collections.Look(ref networkedDevices, "ATR_networkedDevices", LookMode.Reference);
            Scribe_Collections.Look(ref cloudPawns, "ATR_cloudPawns", LookMode.Deep);
            Scribe_Collections.Look(ref virusedDevices, "ATR_virusedDevices", LookMode.Reference, LookMode.Value, ref thingKeyCopy, ref thingValueCopy);
            Scribe_Collections.Look(ref networkLinkedPawns, "ATR_networkLinkedPawns", LookMode.Reference, LookMode.Value, ref pawnKeyCopy, ref pawnValueCopy);

            if (Scribe.mode == LoadSaveMode.PostLoadInit)
            {
                AllocateIfNull();
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
            if (CGT % 6000 == 0)
            {
                CheckHackTimePenalty();
                CheckRightlessFactions();
            }
        }

        // Check the hack timer penalty and reduce it if it is non-zero.
        public void CheckHackTimePenalty()
        {
            if (hackCostTimePenalty > 0)
            {
                // The penalty decays by 1% every 6000 ticks. If it is small enough, simply set the penalty to 0.
                float decayedPenalty = hackCostTimePenalty * 0.99f;
                if (decayedPenalty <= 10)
                {
                    hackCostTimePenalty = 0;
                }
                else
                {
                    hackCostTimePenalty = (int)decayedPenalty;
                }
            }
        }

        // Check the factions to see if any of them should modify their relations with the player faction.
        public void CheckRightlessFactions()
        {
            bool postLoad = false;
            if (antiMechanicalRightsFactions == null || antiOrganicRightsFactions == null)
            {
                postLoad = true;
                GenerateRightlessFactions();
            }

            if (!ATReforged_Settings.factionsWillDeclareRightsWars)
            {
                // If settings indicate that rights wars are disabled after loading the game, ensure that factions are not permanent enemies because of rights after loading a save.
                if (postLoad)
                {
                    foreach (Faction faction in antiMechanicalRightsFactions)
                    {
                        faction.def.permanentEnemy = false;
                    }
                    foreach (Faction faction in antiOrganicRightsFactions)
                    {
                        faction.def.permanentEnemy = false;
                    }
                }
                return;
            }

            bool playerFactionHasMechanicalColonists = false;
            bool playerFactionHasOrganicColonists = false;
            IEnumerable<Pawn> playerPawns = PawnsFinder.AllMapsCaravansAndTravelingTransportPods_Alive_FreeColonistsAndPrisoners;
            foreach (Pawn pawn in playerPawns)
            {
                // Drone check (only for mechanical)
                if (Utils.IsConsideredMechanicalDrone(pawn))
                {
                    if (!ATReforged_Settings.dronesTriggerRightsWars)
                    {
                        continue;
                    }

                    playerFactionHasMechanicalColonists = true;
                }
                // Prisoner check (can be either mechanical or organic)
                else if (pawn.IsPrisonerOfColony)
                {
                    if (!ATReforged_Settings.prisonersTriggerRightsWars)
                    {
                        continue;
                    }

                    if (Utils.IsConsideredMechanical(pawn))
                    {
                        playerFactionHasMechanicalColonists = true;
                    }
                    else
                    {
                        playerFactionHasOrganicColonists = true;
                    }
                }
                // Slave check (can be either mechanical androids or organic, only available with Ideology DLC)
                else if (ModsConfig.IdeologyActive && pawn.IsSlaveOfColony)
                {
                    if (!ATReforged_Settings.slavesTriggerRightsWars)
                    {
                        continue;
                    }

                    if (Utils.IsConsideredMechanicalAndroid(pawn))
                    {
                        playerFactionHasMechanicalColonists = true;
                    }
                    else
                    {
                        playerFactionHasOrganicColonists = true;
                    }
                }
                // Surrogate check (can be mechanical androids or organic)
                else if (Utils.IsSurrogate(pawn))
                {
                    if (!ATReforged_Settings.surrogatesTriggerRightsWars)
                    {
                        continue;
                    }
                    if (Utils.IsConsideredMechanicalAndroid(pawn))
                    {
                        playerFactionHasMechanicalColonists = true;
                    }
                    else
                    {
                        playerFactionHasOrganicColonists = true;
                    }
                }
                // Colonists
                else
                {
                    if (Utils.IsConsideredMechanicalAndroid(pawn))
                    {
                        playerFactionHasMechanicalColonists = true;
                    }
                    else
                    {
                        playerFactionHasOrganicColonists = true;
                    }
                }

                // If both are true, terminate early to avoid unnecessary checks.
                if (playerFactionHasOrganicColonists && playerFactionHasMechanicalColonists)
                {
                    break;
                }
            }

            // If the player has mechanical colonists, all factions which outlaw mechanical colonists become enemies.
            if (playerFactionHasMechanicalColonists)
            {
                foreach (Faction faction in antiMechanicalRightsFactions)
                {
                    if (faction.def.permanentEnemy || faction.defeated)
                    {
                        continue;
                    }

                    // If the faction had an opinion higher than -100, send a notification about the rights war to the player.
                    if (faction.GoodwillWith(Faction.OfPlayer) > -100)
                    {
                        Find.LetterStack.ReceiveLetter("ATR_DeclarationOfWarRights".Translate(), "ATR_DeclarationOfWarRightsDesc".Translate(faction.NameColored, "ATR_PawnTypeMechanical".Translate().ToLower(), faction.leader?.NameFullColored ?? faction.NameColored), LetterDefOf.NegativeEvent);
                    }

                    // Ensure the opinion is -100, and set the faction to permanent enemy.
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -500, reason: ATR_HistoryEventDefOf.ATR_PossessesMechanicalColonist);
                    faction.def.permanentEnemy = true;
                }
            }
            // If the player has no mechanical colonists, all factions which outlaw mechanical colonists are no longer enemies (but will remain hostile).
            else
            {
                foreach (Faction faction in antiMechanicalRightsFactions)
                {
                    if (!faction.def.permanentEnemy || faction.defeated)
                    {
                        continue;
                    }

                    faction.def.permanentEnemy = false;
                    Find.LetterStack.ReceiveLetter("ATR_CessationOfConflict".Translate(), "ATR_CessationOfConflictDesc".Translate(faction.NameColored, "ATR_PawnTypeMechanical".Translate().ToLower()), LetterDefOf.PositiveEvent);
                }
            }

            // If the player has organic colonists, all factions which outlaw organic colonists become enemies.
            if (playerFactionHasOrganicColonists)
            {
                foreach (Faction faction in antiOrganicRightsFactions)
                {
                    if (faction.def.permanentEnemy || faction.defeated)
                    {
                        continue;
                    }

                    // If the faction had an opinion higher than -100, send a notification about the rights war to the player.
                    if (faction.GoodwillWith(Faction.OfPlayer) > -100)
                    {
                        Find.LetterStack.ReceiveLetter("ATR_DeclarationOfWarRights".Translate(), "ATR_DeclarationOfWarRightsDesc".Translate(faction.NameColored, "ATR_PawnTypeOrganic".Translate().ToLower(), faction.leader?.NameFullColored ?? faction.NameColored), LetterDefOf.NegativeEvent);
                    }

                    // Ensure the opinion is -100, and set the faction to permanent enemy.
                    faction.TryAffectGoodwillWith(Faction.OfPlayer, -500, reason: ATR_HistoryEventDefOf.ATR_PossessesOrganicColonist);
                    faction.def.permanentEnemy = true;
                }
            }
            // If the player has no organic colonists, all factions which outlaw organic colonists are no longer enemies (but will remain hostile).
            else
            {
                foreach (Faction faction in antiOrganicRightsFactions)
                {
                    if (!faction.def.permanentEnemy || faction.defeated)
                    {
                        continue;
                    }

                    faction.def.permanentEnemy = false;
                    Find.LetterStack.ReceiveLetter("ATR_CessationOfConflict".Translate(), "ATR_CessationOfConflictDesc".Translate(faction.NameColored, "ATR_PawnTypeOrganic".Translate().ToLower()), LetterDefOf.PositiveEvent);
                }
            }
        }

        // Generate the rightless factions for storage, to be stored until the game is deloaded or another save is loaded. Individual factions may be destroyed or set to not care and therefore should be removed.
        public void GenerateRightlessFactions()
        {
            antiMechanicalRightsFactions = new List<Faction>();
            antiOrganicRightsFactions = new List<Faction>();
            foreach (Faction faction in Find.FactionManager.AllFactionsListForReading)
            {
                if (ATReforged_Settings.antiMechanicalRightsFaction.Contains(faction.def.defName) && !faction.defeated)
                {
                    antiMechanicalRightsFactions.Add(faction);
                }
                if (ATReforged_Settings.antiOrganicRightsFaction.Contains(faction.def.defName) && !faction.defeated)
                {
                    antiOrganicRightsFactions.Add(faction);
                }
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

                // Timer has expired. Inform it that it is no longer virused. The CompSkyMind will handle the rest.
                if (virusedDevice.Value != -1 && virusedDevice.Value <= GT )
                {
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
                CompSkyMindLink cso = networkLinkedPawn.Key.GetComp<CompSkyMindLink>();
                if (cso == null)
                    continue;

                // Timer has expired. Inform it that it the SkyMind operation has ended. The CompSurrogateOwner will handle the rest.
                if (networkLinkedPawn.Value != -1 && networkLinkedPawn.Value <= GT)
                {
                    cso.Linked = -1;
                    continue;
                }

                // Check to see if the operation has been interrupted for any reason.
                cso.CheckInterruptedUpload();
            }
        }

        // Add the cached point generations from active servers. When servers are added or removed, the cached amount is recalculated automatically.
        public void CheckServers()
        {
            ChangeServerPoints(cachedSkillGeneration, ServerType.SkillServer);
            ChangeServerPoints(cachedSecurityGeneration, ServerType.SecurityServer);
            ChangeServerPoints(cachedHackingGeneration, ServerType.HackingServer);
        }

        // Calculate the point capacity and point generation for the given server type from the appropriate server list. Cache results for easy usage elsewhere.
        public void ResetServers(ServerType type)
        {
            CompComputer compComputer;
            CompSuperComputer compSuperComputer;
            switch (type)
            {
                case ServerType.SkillServer:
                    skillPointCapacity = 0;
                    cachedSkillGeneration = 0;

                    // Calculate Skill server points using CompComputer or CompSuperComputer (or both if it has them). Cache results.
                    foreach (Building building in skillServers.ToList())
                    {
                        compComputer = building.GetComp<CompComputer>();
                        compSuperComputer = building.GetComp<CompSuperComputer>();
                        if (compComputer != null)
                        {
                            skillPointCapacity += compComputer.Props.pointStorage;
                            cachedSkillGeneration += compComputer.Props.passivePointGeneration;
                        }
                        if (compSuperComputer != null)
                        {
                            skillPointCapacity += compSuperComputer.Props.pointStorage;
                            cachedSkillGeneration += compSuperComputer.Props.passivePointGeneration;
                        }
                    }
                    break;
                case ServerType.SecurityServer:
                    securityPointCapacity = 0;
                    cachedSecurityGeneration = 0;

                    // Calculate Security server points using CompComputer or CompSuperComputer (or both if it has them). Cache results.
                    foreach (Building building in securityServers.ToList())
                    {
                        compComputer = building.GetComp<CompComputer>();
                        compSuperComputer = building.GetComp<CompSuperComputer>();
                        if (compComputer != null)
                        {
                            securityPointCapacity += compComputer.Props.pointStorage;
                            cachedSecurityGeneration += compComputer.Props.passivePointGeneration;
                        }
                        if (compSuperComputer != null)
                        {
                            securityPointCapacity += compSuperComputer.Props.pointStorage;
                            cachedSecurityGeneration += compSuperComputer.Props.passivePointGeneration;
                        }
                    }
                    break;
                case ServerType.HackingServer:
                    hackingPointCapacity = 0;
                    cachedHackingGeneration = 0;

                    // Calculate Hacking server points using CompComputer or CompSuperComputer (or both if it has them). Cache results.
                    foreach (Building building in hackingServers)
                    {
                        compComputer = building.GetComp<CompComputer>();
                        compSuperComputer = building.GetComp<CompSuperComputer>();
                        if (compComputer != null)
                        {
                            hackingPointCapacity += compComputer.Props.pointStorage;
                            cachedHackingGeneration += compComputer.Props.passivePointGeneration;
                        }
                        if (compSuperComputer != null)
                        {
                            hackingPointCapacity += compSuperComputer.Props.pointStorage;
                            cachedHackingGeneration += compSuperComputer.Props.passivePointGeneration;
                        }
                    }
                    break;
                default:
                    Log.Error("[ATR] ATR_GC.ResetServers: Attempted illegal server type reset. No changes made. This may generate errors.");
                    return;
            }
        }

        // This will reset all point servers of all categories when called.
        public void ResetServers()
        {
            ResetServers(ServerType.SkillServer);
            ResetServers(ServerType.SecurityServer);
            ResetServers(ServerType.HackingServer);
        }

        public float GetPointCapacity(ServerType pointMode)
        {
            switch (pointMode)
            {
                case ServerType.SkillServer:
                    return skillPointCapacity;
                case ServerType.SecurityServer:
                    return securityPointCapacity;
                case ServerType.HackingServer:
                    return hackingPointCapacity;
                default:
                    return 0;
            }
        }

        public float GetPoints(ServerType pointMode)
        {
            switch (pointMode)
            {
                case ServerType.SkillServer:
                    return skillPoints;
                case ServerType.SecurityServer:
                    return securityPoints;
                case ServerType.HackingServer:
                    return hackingPoints;
                default:
                    return 0;
            }
        }

        // Determine if the provided pawn is connected to the SkyMind Network.
        public bool HasSkyMindConnection(Pawn pawn)
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
            if (networkedDevices.Count >= SkyMindNetworkCapacity)
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
                // Inform all comps that the thing is no longer connected to the SkyMind network.
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
            while (SkyMindNetworkCapacity < networkedDevices.Count)
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
            while (SkyMindCloudCapacity < cloudPawns.Count)
            {
                // Killing the pawn will automatically handle any interrupted mind operations or surrogate connections.
                Pawn victim = cloudPawns.RandomElement();
                cloudPawns.Remove(victim);
                victim.Kill(null);
            }
        }

        // Return the maximum number of cloud pawns that may be stored in the SkyMind network currently.
        public int GetSkyMindCloudCapacity()
        {
            return SkyMindCloudCapacity;
        }

        // Add a server to the appropriate list based on serverMode
        public void AddServer(Building building, ServerType serverMode)
        { 
            switch (serverMode)
            {
                case ServerType.SkillServer:
                    skillServers.Add(building);
                    ResetServers(ServerType.SkillServer);
                    break;
                case ServerType.SecurityServer:
                    securityServers.Add(building);
                    ResetServers(ServerType.SecurityServer);
                    break;
                case ServerType.HackingServer:
                    hackingServers.Add(building);
                    ResetServers(ServerType.HackingServer);
                    break;
                default:
                    Log.Message("[ATR] Attempted to add a server of an invalid mode. No server was added. The building responsible should have a Gizmo to fix the issue.");
                    return;
            }
        }

        // If a capacity is provided instead of a serverMode, assume this capacity is to be added to all servers
        public void AddServer(Building building)
        { 
            skillServers.Add(building);
            securityServers.Add(building);
            hackingServers.Add(building);
            ResetServers();
        }

        // Remove the building from the task group it is assigned to.
        public void RemoveServer(Building building, ServerType serverMode)
        {
            switch (serverMode)
            {
                case ServerType.SkillServer: // Remove from skill servers list.
                    if (skillServers.Contains(building))
                    {
                        skillServers.Remove(building);
                        ResetServers(ServerType.SkillServer);
                    }
                    break;
                case ServerType.SecurityServer: // Remove from security servers list.
                    if (securityServers.Contains(building))
                    {
                        securityServers.Remove(building);
                        ResetServers(ServerType.SecurityServer);
                    }
                    break;
                case ServerType.HackingServer: // Remove from hacking servers list.
                    if (hackingServers.Contains(building))
                    {
                        hackingServers.Remove(building);
                        ResetServers(ServerType.HackingServer);
                    }
                    break;
                default:
                    Log.Error("[ATR] ATR_GC.RemoveServer was given an invalid serverType. All servers recached.");
                    ResetServers();
                    return;
            }
        }

        // If a capacity is provided instead of a serverMode, assume this capacity is to removed from all servers
        public void RemoveServer(Building building)
        { 
            skillServers.Remove(building);
            securityServers.Remove(building);
            hackingServers.Remove(building);
            ResetServers();
        }

        // This always adds the points to the appropriate category. It assumes negative changes are given in the parameter. It also handles illegal types (do nothing).
        // It handles numbers going out of bounds by ensuring it doesn't drop below 0 or go above the capacity.
        public void ChangeServerPoints(float toChange, ServerType serverMode)
        {
            switch (serverMode)
            {
                case ServerType.None:
                    Log.Error("[ATR] Can't add points to a None server type! No points changed.");
                    return;
                case ServerType.SkillServer:
                    skillPoints = Mathf.Clamp(skillPoints + toChange, 0, skillPointCapacity);
                    break;
                case ServerType.SecurityServer:
                    securityPoints = Mathf.Clamp(securityPoints + toChange, 0, securityPointCapacity);
                    break;
                case ServerType.HackingServer:
                    hackingPoints = Mathf.Clamp(hackingPoints + toChange, 0, hackingPointCapacity);
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
        
        private void AllocateIfNull()
        { 
            if (networkedDevices == null)
                networkedDevices = new HashSet<Thing>();
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

        // Floats for storing capacities of various comps. Servers also have current point values.
        private float skillPointCapacity = 0;
        private float skillPoints = 0;
        private float securityPointCapacity = 0;
        private float securityPoints = 0;
        private float hackingPointCapacity = 0;
        private float hackingPoints = 0;
        private int SkyMindNetworkCapacity = 0;
        private int SkyMindCloudCapacity = 0;

        // Cached point generation amounts for efficiency. Recalculated when servers are added/removed.
        private float cachedSkillGeneration = 0;
        private float cachedSecurityGeneration = 0;
        private float cachedHackingGeneration = 0;

        // Simple container for the pawn that represents blanks so it can be generated once and then saved for the whole game. It should never be altered.
        public Pawn blankPawn = null;

        // Simple tracker for the extra cost penalty for initiating player hacks after having done one recently.
        public int hackCostTimePenalty = 0;

        // Networked devices are things that are connected to the SkyMind network, including free pawns, surrogates, and buildings.
        public HashSet<Thing> networkedDevices = new HashSet<Thing>();

        // Cloud Pawns are pawns that are stored in the SkyMind Network. This is important as their gizmo's are inaccessible and can't be connected to the SkyMind (but should be considered as if they are).
        private HashSet<Pawn> cloudPawns = new HashSet<Pawn>();

        // Servers have 3 different active states they may be in, and may be saved/changed independently of each other. They must also be saved so points may be generated.
        private HashSet<Building> skillServers = new HashSet<Building>();
        private HashSet<Building> securityServers = new HashSet<Building>();
        private HashSet<Building> hackingServers = new HashSet<Building>();

        // Virused devices are things with their values being the tick at which to release them. This avoids the CompSkyMind having to store this information.
        private Dictionary<Thing, int> virusedDevices = new Dictionary<Thing, int>();

        // Network Linked devices are things (pawns) that are currently undergoing some sort of SkyMind procedure like uploading, duplicating, etc. The value stores the tick at which to release them.
        private Dictionary<Pawn, int> networkLinkedPawns = new Dictionary<Pawn, int>();

        // Local reserved storage for saving/loading virusedDevices and newtorkedLinkedPawns in the ExposeData method.
        private List<Thing> thingKeyCopy = new List<Thing>();
        private List<int> thingValueCopy = new List<int>();
        private List<Pawn> pawnKeyCopy = new List<Pawn>();
        private List<int> pawnValueCopy = new List<int>();

        // Local containers for factions for being checked against. They are not saved, and are produced when needed;
        private List<Faction> antiMechanicalRightsFactions = null;
        private List<Faction> antiOrganicRightsFactions = null;

        // Simple booleans for whether players have encountered some mechanics yet to display educational letters. Some letters are handled by researches.
        public bool hasBuiltDrone = false;
        public bool hasBuiltAndroid = false;
        public bool hasImprisonedDrone = false;
        public bool hasMadeSurrogate = false;

    }
}