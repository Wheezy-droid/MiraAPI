using System.Collections;
using HarmonyLib;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities;
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
        Coroutines.Start(SetFps());
    }

    private static IEnumerator SetFps()
    {
        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
        yield return new WaitForSeconds(1f);

        Application.targetFrameRate = (int)LocalSettingsTabSingleton<MiraApiSettings>.Instance.SetFpsSlider.Value;
    }
}
