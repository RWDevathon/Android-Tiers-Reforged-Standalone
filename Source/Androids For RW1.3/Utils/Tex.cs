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
        public static readonly Texture2D Battery = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ChargeIcon", true);
        public static readonly Texture2D DrawPocket = ContentFinder<Texture2D>.Get("UI/Icons/Settings/DrawPocket", true);

        public static readonly Texture2D SkillWorkshopHeader = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/SkillWorkshopHeader", true);
        public static readonly Texture2D NoCare = ContentFinder<Texture2D>.Get("UI/Icons/Tabs/NoMechanicCare", true);
        public static readonly Texture2D NoMed = ContentFinder<Texture2D>.Get("UI/Icons/Tabs/NoRepairStims", true);
        public static readonly Texture2D RepairStimSimple = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/ATR_RepairStimSimple/ATR_RepairStimSimple_a", true);
        public static readonly Texture2D RepairStimIntermediate = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/ATR_RepairStimIntermediate/ATR_RepairStimIntermediate_a", true);
        public static readonly Texture2D RepairStimAdvanced = ContentFinder<Texture2D>.Get("Things/Items/Manufactured/ATR_RepairStimAdvanced/ATR_RepairStimAdvanced_a", true);

        public static readonly Texture2D Permute = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/PermuteIcon", true);
        public static readonly Texture2D Duplicate = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/DuplicateIcon", true);
        public static readonly Texture2D SkyMindUpload = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkyMindUploadIcon", true);
        public static readonly Texture2D DownloadFromSkyCloud = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkyMindDownloadIcon", true);

        public static readonly Texture2D MindAbsorption = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/MindAbsorptionIcon", true);

        public static readonly Texture2D SkillIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkillIcon", true);
        public static readonly Texture2D SecurityIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SecurityIcon", true);
        public static readonly Texture2D HackingIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/HackingIcon", true);

        public static readonly Texture2D PassionDisabled = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/PassionDisabledIcon", true);
        public static readonly Texture2D NoPassion = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/NoPassionIcon", true);
        public static readonly Texture2D MinorPassion = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MinorPassionIcon", true);
        public static readonly Texture2D MajorPassion = ContentFinder<Texture2D>.Get("UI/Icons/Dialogs/MajorPassionIcon", true);

        public static readonly Texture2D CloseDoorIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CloseDoorIcon", true);
        public static readonly Texture2D OpenDoorIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/OpenDoorIcon", true);

        public static readonly Texture2D processInfo = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreInfoIcon", true);
        public static readonly Texture2D processRemove = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreRemoveIcon", true);
        public static readonly Texture2D processReplicate = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreDuplicateIcon", true);
        public static readonly Texture2D processSkillUp = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/CoreSkillWorkshopIcon", true);

        // SkyMind
        public static readonly Texture2D ControlModeIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ControlModeIcon", true);
        public static readonly Texture2D ConnectSkyMindIcon = ContentFinder<Texture2D>.Get("UI/Avatars/SkyMindConnection", true);
        public static readonly Texture2D SkillWorkshopIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/SkillWorkshopIcon", true);

        // Surrogates
        public static readonly Texture2D ConnectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/ConnectIcon", true);
        public static readonly Texture2D RecoveryIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/RecoveryIcon", true);
        public static readonly Texture2D DisconnectIcon = ContentFinder<Texture2D>.Get("UI/Icons/Gizmos/DisconnectIcon", true);

        public static readonly Material MindOperation = MaterialPool.MatFrom("UI/Avatars/MindOperation", ShaderDatabase.MetaOverlay);

        public static readonly Material WarningHeat =  MaterialPool.MatFrom("UI/Icons/Temperature/WarningHeat", ShaderDatabase.MetaOverlay);
        public static readonly Material DangerHeat = MaterialPool.MatFrom("UI/Icons/Temperature/DangerHeat", ShaderDatabase.MetaOverlay);
        public static readonly Material CriticalHeat = MaterialPool.MatFrom("UI/Icons/Temperature/CriticalHeat", ShaderDatabase.MetaOverlay);

        public static readonly Material RemotelyControlledNode = MaterialPool.MatFrom("UI/Avatars/SkyMindNode", ShaderDatabase.MetaOverlay);
    }
}