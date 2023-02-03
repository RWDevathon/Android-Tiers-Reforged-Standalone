using Verse;
using RimWorld;
using System.Collections.Generic;
using System.Text;

namespace ATReforged
{
    public class CompSkyMind : ThingComp
    {
        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look(ref integrityBreach, "ATR_integrityBreach", -1);
            Scribe_Values.Look(ref connected, "ATR_connected", false);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);

            Utils.gameComp.PopVirusedThing(parent);
            Utils.gameComp.DisconnectFromSkyMind(parent);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            if (connected)
            {
                if (!Utils.gameComp.AttemptSkyMindConnection(parent))
                {
                    connected = false;
                }
            }
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            // Infected don't get buttons to interact with the SkyMind. 
            if (integrityBreach != -1)
                yield break;

            // If this pawn can't use the SkyMind or is not a colonist or prisoner, then it doesn't get any buttons to interact with it.
            if (parent is Pawn pawn)
            {
                if (!Utils.HasCloudCapableImplant(pawn) || (pawn.Faction != Faction.OfPlayer && !pawn.IsPrisonerOfColony))
                {
                    yield break;
                }
            }
            // Non-pawns (buildings) must belong to the player to connect to the SkyMind network.
            else
            {
                if (parent.Faction != Faction.OfPlayer)
                {
                    yield break;
                }
            }

            // If there is no SkyMind capacity, then it doesn't get any buttons to interact with it.
            if (Utils.gameComp.GetSkyMindNetworkSlots() <= 0)
            {
                yield break;
            }

            // Connect/Disconnect to SkyMind
            yield return new Command_Toggle
            { 
                icon = Tex.ConnectSkyMindIcon,
                defaultLabel = "ATR_ConnectSkyMind".Translate(),
                defaultDesc = "ATR_ConnectSkyMindDesc".Translate(),
                isActive = () => connected,
                toggleAction = delegate ()
                {
                    if (!connected)
                    { // Attempt to connect to SkyMind
                        if (!Utils.gameComp.AttemptSkyMindConnection(parent))
                        { // If trying to connect but it is unable to, inform the player. 
                            if (Utils.gameComp.GetSkyMindNetworkSlots() == 0)
                                Messages.Message("ATR_SkyMindConnectionFailedNoNetwork".Translate(), parent, MessageTypeDefOf.NegativeEvent);
                            else
                                Messages.Message("ATR_SkyMindConnectionFailed".Translate(), parent, MessageTypeDefOf.NegativeEvent);
                            return;
                        }
                    }
                    else
                    { // Disconnect from SkyMind
                        Utils.gameComp.DisconnectFromSkyMind(parent);
                    }
                }
            };
        }

        public override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);

            switch (signal)
            {
                case "SkyMindNetworkUserConnected":
                    connected = true;
                    break;
                case "SkyMindNetworkUserDisconnected":
                    connected = false;
                    break;
            }
        }

        // Controller for the state of viruses in the parent. -1 = clean, 1 = sleeper, 2 = cryptolocker, 3 = breaker. Ticker handled by the GC to avoid calculating when clean.
        public int Breached
        {
            get
            {
                return integrityBreach;
            }

            set
            {
                int status = integrityBreach;
                integrityBreach = value;
                // Device is no longer breached. Release restrictions and remove from the virus list.
                if (integrityBreach == -1 && status != -1)
                {
                    // Release hacked pawn. Remove Mind Operation hediff and reboot.
                    if (parent is Pawn pawn)
                    { 
                        Hediff hediff = HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_LongReboot, pawn, null);
                        hediff.Severity = 0.75f;
                        pawn.health.AddHediff(hediff, null, null);
                        hediff = pawn.health.hediffSet.GetFirstHediffOfDef(ATR_HediffDefOf.ATR_MindOperation);
                        if (hediff != null)
                        {
                            pawn.health.RemoveHediff(hediff);
                        }
                    }
                    // Handle buildings that lost power.
                    else
                    { 
                        CompFlickable cf = parent.TryGetComp<CompFlickable>();
                        if (cf != null)
                        {
                            cf.SwitchIsOn = true;
                            parent.SetFaction(Faction.OfPlayer);
                        }
                    }
                    Utils.gameComp.PopVirusedThing(parent);
                }
                else
                {
                    // Breached building. Hacking effect is that it gets turned off and is applied to a hostile faction until released.
                    if (parent is Building)
                    {
                        CompFlickable cf = parent.TryGetComp<CompFlickable>();
                        if (cf != null)
                        {
                            cf.SwitchIsOn = false;
                            parent.SetFaction(Faction.OfAncientsHostile);
                        }
                    }
                    // Breached pawn (surrogates). Hacking effect is that it is put offline via a forced Mind Operation hediff.
                    else if (parent is Pawn pawn)
                    {
                        pawn.health.AddHediff(HediffMaker.MakeHediff(ATR_HediffDefOf.ATR_MindOperation, pawn));
                    }
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder ret = new StringBuilder();

            if (parent.Map == null)
                return base.CompInspectStringExtra();

            // Add a special line for devices hacked into a shut-down state.
            if ((integrityBreach == 1 || integrityBreach == 3) && Utils.gameComp.GetAllVirusedDevices().ContainsKey(parent))
            {
                ret.Append("ATR_HackedWithTimer".Translate((Utils.gameComp.GetVirusedDevice(parent) - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose()));
            }
            // Add a special line for cryptolocked devices.
            else if (integrityBreach == 2)
            {
                ret.Append("ATR_CryptoLocked".Translate());
            }
            else if (connected)
            {
                ret.Append("ATR_SkyMindDetected".Translate());
            }

            return ret.Append(base.CompInspectStringExtra()).ToString();
        }

        private int integrityBreach = -1; // -1 : Not integrityBreach. 1: Sleeper Virus. 2: Cryptolocked. 3: Breaker Virus.
        public bool connected;
    }
}