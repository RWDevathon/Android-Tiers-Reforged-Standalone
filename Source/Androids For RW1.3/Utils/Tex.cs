using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using UnityEngine;

namespace ATReforged
{
    [StaticConstructorOnStartup]
    static class Tex
    {
        static Tex()
        {
        }
        public static readonly Texture2D Battery = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ChargeIcon");
        public static readonly Texture2D DrawPocket = ContentFinder<Texture2D>.Get("UI/Icons/Settings/DrawPocket");
        // public static readonly Texture2D DefaultHumanlike = ContentFinder<Texture2D>.Get("UI/Icons/Settings/defaultHumanlikeIcon");

        public static readonly Texture2D SkillWorkshopHeader = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/SkillWorkshopHeader");
        public static readonly Texture2D NoCare = ContentFinder<Texture2D>.Get("UI/Icons/Tabs/NoMechanicCare");
        public static readonly Texture2D NoMed = ContentFinder<Texture2D>.Get("UI/Icons/Tabs/NoRepairStims");
        public static readonly Texture2D RepairStimSimple = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/ATR_RepairStimSimple/ATR_RepairStimSimple_a");
        public static readonly Texture2D RepairStimIntermediate = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/ATR_RepairStimIntermediate/ATR_RepairStimIntermediate_a");
        public static readonly Texture2D RepairStimAdvanced = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/ATR_RepairStimAdvanced/ATR_RepairStimAdvanced_a");

        public static readonly Texture2D Permute = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/PermuteIcon");
        public static readonly Texture2D Duplicate = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/DuplicateIcon");
        public static readonly Texture2D SkyMindUpload = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkyMindUploadIcon");
        public static readonly Texture2D DownloadFromSkyCloud = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkyMindDownloadIcon");

        public static readonly Texture2D MindAbsorption = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/MindAbsorptionIcon");

        public static readonly Texture2D SkillIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkillIcon");
        public static readonly Texture2D SecurityIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SecurityIcon");
        public static readonly Texture2D HackingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/HackingIcon");

        public static readonly Texture2D PassionDisabled = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/PassionDisabledIcon");
        public static readonly Texture2D NoPassion = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/NoPassionIcon");
        public static readonly Texture2D MinorPassion = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MinorPassionIcon");
        public static readonly Texture2D MajorPassion = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MajorPassionIcon");

        public static readonly Texture2D CloseDoorIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CloseDoorIcon");
        public static readonly Texture2D OpenDoorIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/OpenDoorIcon");

        public static readonly Texture2D processInfo = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreInfoIcon");
        public static readonly Texture2D processRemove = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreRemoveIcon");
        public static readonly Texture2D processReplicate = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreDuplicateIcon");
        public static readonly Texture2D processSkillUp = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreSkillWorkshopIcon");

        // SkyMind
        public static readonly Texture2D ControlModeIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ControlModeIcon");
        public static readonly Texture2D ConnectSkyMindIcon = ContentFinder<Texture2D>.Get("UI/Avatars/SkyMindConnection");
        public static readonly Texture2D SkillWorkshopIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkillWorkshopIcon");

        // Surrogates
        public static readonly Texture2D ConnectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ConnectIcon");
        public static readonly Texture2D RecoveryIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/RecoveryIcon");
        public static readonly Texture2D DisconnectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/DisconnectIcon");

        public static readonly Material MindOperation = MaterialPool.MatFrom("UI/Avatars/MindOperation", ShaderDatabase.MetaOverlay);

        public static readonly Material WarningHeat =  MaterialPool.MatFrom("UI/Icons/Temperature/WarningHeat", ShaderDatabase.MetaOverlay);
        public static readonly Material DangerHeat = MaterialPool.MatFrom("UI/Icons/Temperature/DangerHeat", ShaderDatabase.MetaOverlay);
        public static readonly Material CriticalHeat = MaterialPool.MatFrom("UI/Icons/Temperature/CriticalHeat", ShaderDatabase.MetaOverlay);

        public static readonly Material RemotelyControlledNode = MaterialPool.MatFrom("UI/Avatars/SkyMindNode", ShaderDatabase.MetaOverlay);
    }
}