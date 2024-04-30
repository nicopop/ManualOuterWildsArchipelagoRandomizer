using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace ArchipelagoRandomizer;

[HarmonyPatch]
internal class Jellyfish
{
    static HashSet<InsulatingVolume> jellyfishInsulatingVolumes = new();

    public static void OnCompleteSceneLoad(OWScene _scene, OWScene _loadScene)
    {
        // we don't want to retain these references beyond a scene transition / loop reset, or else
        // they become invalid and lead to NullReferenceExceptions when we try using them later
        jellyfishInsulatingVolumes.Clear();

        var pioneerInteractables = GameObject.Find("DB_PioneerDimension_Body/Sector_PioneerDimension/Interactables_PioneerDimension");
        var vesselInteractables = GameObject.Find("DB_VesselDimension_Body/Sector_VesselDimension/Interactables_VesselDimension");
        var smallNestInteractables = GameObject.Find("DB_SmallNestDimension_Body/Sector_SmallNestDimension/Interactables_SmallNestDimension");
        var anglerNestInteractables = GameObject.Find("DB_AnglerNestDimension_Body/Sector_AnglerNestDimension/Interactables_AnglerNestDimension");
        var clusterInteractables = GameObject.Find("DB_ClusterDimension_Body/Sector_ClusterDimension/Interactables_ClusterDimension");
        var exitOnlyInteractables = GameObject.Find("DB_ExitOnlyDimension_Body/Sector_ExitOnlyDimension/Interactables_ExitOnlyDimension");
        var hubInteractables = GameObject.Find("DB_HubDimension_Body/Sector_HubDimension/Interactables_HubDimension");
        var escapePodInteractables = GameObject.Find("DB_EscapePodDimension_Body/Sector_EscapePodDimension/Interactables_EscapePodDimension");
        APRandomizer.OWMLModConsole.WriteLine($"{pioneerInteractables} - {vesselInteractables} - {smallNestInteractables} - {anglerNestInteractables} - {clusterInteractables} - {exitOnlyInteractables} - {hubInteractables} - {escapePodInteractables}");

        var pioneerOFWV = pioneerInteractables.transform.Find("OuterWarp_Pioneer").GetComponent<OuterFogWarpVolume>();
        var vesselOFWV = vesselInteractables.transform.Find("OuterWarp_Vessel").GetComponent<OuterFogWarpVolume>();
        OuterFogWarpVolume smallNestOFWV = null;// smallNestInteractables.transform.Find("OuterWarp_SmallNest").GetComponent<OuterFogWarpVolume>();
        var anglerNestOFWV = anglerNestInteractables.transform.Find("OuterWarp_AnglerNest").GetComponent<OuterFogWarpVolume>();
        var clusterOFWV = clusterInteractables.transform.Find("OuterWarp_Cluster").GetComponent<OuterFogWarpVolume>();
        var exitOnlyOFWV = exitOnlyInteractables.transform.Find("OuterWarp_ExitOnly").GetComponent<OuterFogWarpVolume>();
        var hubOFWV = hubInteractables.transform.Find("OuterWarp_Hub").GetComponent<OuterFogWarpVolume>();
        var escapePodOFWV = escapePodInteractables.transform.Find("OuterWarp_EscapePod").GetComponent<OuterFogWarpVolume>();
        APRandomizer.OWMLModConsole.WriteLine($"{pioneerOFWV} - {vesselOFWV} - {smallNestOFWV} - {anglerNestOFWV} - {clusterOFWV} - {exitOnlyOFWV} - {hubOFWV} - {escapePodOFWV}");

        var entranceIFVW = GameObject.Find("DarkBramble_Body/Sector_DB/Interactables_DB/EntranceWarp_ToHub").GetComponent<InnerFogWarpVolume>();

        var clusterIFVWs = clusterInteractables.transform.GetComponentsInChildren<InnerFogWarpVolume>();
        var clusterToPioneerIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToPioneer");
        var clusterToExitOnlyIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToExitOnly");

        var escapePodToAnglerNestIFVW = escapePodInteractables.transform.Find("InnerWarp_ToAnglerNest").GetComponent<InnerFogWarpVolume>();

        var anglerNestIFVWs = anglerNestInteractables.transform.GetComponentsInChildren<InnerFogWarpVolume>();
        var anglerNestToExitOnlyIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToExitOnly");
        var anglerNestToVesselIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToVessel");

        var hubIFVWs = hubInteractables.transform.GetComponentsInChildren<InnerFogWarpVolume>();
        var hubToClusterIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToCluster");
        var hubToAnglerNestIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToAnglerNest");
        var hubToSmallNestIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToSmallNest");
        var hubToEscapePodIFVWs = clusterIFVWs.Where(ifvw => ifvw.name == "InnerWarp_ToEscapePod");

        foreach (var ifvw in GameObject.FindObjectsOfType<InnerFogWarpVolume>())
            if (ifvw?._linkedOuterWarpVolume?._linkedInnerWarpVolume != ifvw)
                APRandomizer.OWMLModConsole.WriteLine($"mismatch: {ifvw?.transform?.parent?.name}/{ifvw?.name} -> {ifvw?._linkedOuterWarpVolume?.transform?.parent?.name}/{ifvw?._linkedOuterWarpVolume?.name} -> {ifvw?._linkedOuterWarpVolume?._linkedInnerWarpVolume?.transform?.parent?.name}/{ifvw?._linkedOuterWarpVolume?.name}");

        // actually edit some warps
        entranceIFVW._linkedOuterWarpVolume = clusterOFWV;
        foreach (var ifvw in clusterToPioneerIFVWs) ifvw._linkedOuterWarpVolume = vesselOFWV;
        foreach (var ifvw in clusterToExitOnlyIFVWs) ifvw._linkedOuterWarpVolume = escapePodOFWV;
        escapePodToAnglerNestIFVW._linkedOuterWarpVolume = pioneerOFWV; // does this "incorrectly" still glow red? yes

        var ofwvs = GameObject.FindObjectsOfType<OuterFogWarpVolume>();
        APRandomizer.OWMLModConsole.WriteLine($"ofwvs: {ofwvs.Length}\n{string.Join("\n", ofwvs.Select(ofwv => {
            return $"{ofwv.transform.parent.name}/{ofwv.name} ({ofwv._name}) - {ofwv._linkedInnerWarpVolume.transform.parent.name}/{ofwv._linkedInnerWarpVolume.name}";
        }))}");

        var ifwvs = GameObject.FindObjectsOfType<InnerFogWarpVolume>();
        APRandomizer.OWMLModConsole.WriteLine($"ifwvs: {ifwvs.Length}\n{string.Join("\n", ifwvs.Select(ifwv => {
            return $"{ifwv.transform?.parent?.name}/{ifwv.name} - {ifwv._linkedOuterWarpVolume?.transform?.parent?.name}/{ifwv._linkedOuterWarpVolume?.name} ({ifwv._linkedOuterWarpName})";
        }))}");

        var signals = GameObject.FindObjectsOfType<AudioSignal>();
        APRandomizer.OWMLModConsole.WriteLine($"signals: {signals.Length}\n{string.Join("\n", signals.Select(s => {
            return $"{s.transform?.parent?.name}/{s.name} - {s._outerFogWarpVolume?.transform?.parent?.name}/{s._outerFogWarpVolume?.name}";
        }))}");

        OWAudioSource harmonicaSource = null;
        OWAudioSource pod3Source = null;

        // prevent NREs in TravelerAudioManager by trimming its signal list first
        //var tam = Locator.GetTravelerAudioManager();
        //tam._signals = tam._signals.Where(s => s._outerFogWarpVolume == null).ToList();

        // actually delete all the vanilla signal on nodes inside DB (the signals on the DB exterior/entrance are untouched)
        var dbInteriorSignals = signals.Where(s => s._outerFogWarpVolume != null);
        foreach (var s in dbInteriorSignals)
        {
            if (s.name == "Signal_Harmonica" && harmonicaSource == null)
                harmonicaSource = s._owAudioSource;
            if (s.name == "Signal_EscapePod" && pod3Source == null)
                pod3Source = s._owAudioSource;
            s.gameObject.DestroyAllComponents<AudioSignal>();
        }

        var signal = escapePodToAnglerNestIFVW.gameObject.AddComponent<AudioSignal>();
        signal._frequency = SignalFrequency.Traveler;
        signal._name = SignalName.Traveler_Feldspar;
        signal._onlyAudibleToScope = true;
        signal._outerFogWarpVolume = escapePodOFWV;
        signal._owAudioSource = harmonicaSource;
    }

    [HarmonyPrefix, HarmonyPatch(typeof(JellyfishController), nameof(JellyfishController.OnSectorOccupantsUpdated))]
    public static void JellyfishController_OnSectorOccupantsUpdated_Prefix(ref JellyfishController __instance)
    {
        var insulators = __instance.gameObject.transform.GetComponentsInChildren<InsulatingVolume>();
        jellyfishInsulatingVolumes.UnionWith(insulators);
        ApplyHasKnowledgeFlag();
    }

    private static bool _hasJellyfishKnowledge = false;

    public static bool hasJellyfishKnowledge
    {
        get => _hasJellyfishKnowledge;
        set
        {
            if (_hasJellyfishKnowledge != value)
            {
                _hasJellyfishKnowledge = value;
                ApplyHasKnowledgeFlag();
                if (_hasJellyfishKnowledge)
                {
                    var nd = new NotificationData(NotificationTarget.All, "SPACESUIT ELECTRICAL INSULATION AUGMENTED WITH JELLYFISH MEMBRANES", 10);
                    NotificationManager.SharedInstance.PostNotification(nd, false);
                }
            }
        }
    }

    private static void ApplyHasKnowledgeFlag()
    {
        jellyfishInsulatingVolumes.Do(iv => iv.SetVolumeActivation(hasJellyfishKnowledge));
    }

    static ScreenPrompt insulationIntactPrompt = new("Jellyfish Insulation: Intact", 0);

    [HarmonyPostfix, HarmonyPatch(typeof(ToolModeUI), nameof(ToolModeUI.LateInitialize))]
    public static void ToolModeUI_LateInitialize_Postfix()
    {
        Locator.GetPromptManager().AddScreenPrompt(insulationIntactPrompt, PromptPosition.UpperRight, false);
    }
    [HarmonyPostfix, HarmonyPatch(typeof(ToolModeUI), nameof(ToolModeUI.Update))]
    public static void ToolModeUI_Update_Postfix()
    {
        insulationIntactPrompt.SetVisibility(
            hasJellyfishKnowledge &&
            OWInput.IsInputMode(InputMode.Character) &&
            Locator.GetPlayerSectorDetector().IsWithinSector(Sector.Name.GiantsDeep) &&
            PlayerState.IsCameraUnderwater()
        );
    }
}
