using HarmonyLib;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Patches.Menu;

/// <summary>
/// General MainMenuManage patches.
/// </summary>
[HarmonyPatch(typeof(MainMenuManager))]
public static class MainMenuManagerPatches
{
    /// <summary>
    /// A postifix on Awake to load all the addressables registered.
    /// </summary>
    [HarmonyPatch(nameof(MainMenuManager.Awake))]
    [HarmonyPostfix]
    public static void AwakePostfix()
    {
        AddressablesLoader.LoadAll();
        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
    }
}
