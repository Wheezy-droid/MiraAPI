using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.Hud;
using MiraAPI.Keybinds;
using MiraAPI.LocalSettings;
using MiraAPI.Utilities;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using Rewired;
using TMPro;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches;

/// <summary>
/// General patches for the HudManager class.
/// </summary>
[HarmonyPatch(typeof(HudManager))]
public static class HudManagerPatches
{
    // Custom buttons parent.
    public static GameObject? BottomLeft { get; private set; }
    public static Transform? BottomRight { get; private set; }
    public static Transform? Buttons { get; private set; }

    private static Dictionary<TextMeshPro, int> vanillaKeybindIcons = new();

    internal static List<TextMeshPro> ModdedKeybindIcons = new();

    public static IEnumerator CoResizeUI()
    {
        while (!HudManager.Instance)
        {
            yield return null;
        }

        yield return new WaitForSeconds(0.01f);
        ResizeUI(LocalSettingsTabSingleton<MiraApiSettings>.Instance.ButtonUIFactorSlider.Value);
    }

    public static void ResizeUI(float scaleFactor)
    {
        var baseButtons = HudManager.Instance.transform.FindChild("Buttons");
        if (baseButtons != null)
        {
            foreach (var aspect in baseButtons.GetComponentsInChildren<AspectPosition>(true))
            {
                if (aspect.gameObject == null)
                {
                    continue;
                }

                if (aspect.gameObject.transform.parent.name == "TopRight")
                {
                    continue;
                }

                if (aspect.gameObject.transform.parent.transform.parent.name == "TopRight")
                {
                    continue;
                }

                aspect.gameObject.SetActive(!aspect.isActiveAndEnabled);
                aspect.DistanceFromEdge *= new Vector2(scaleFactor, scaleFactor);
                aspect.gameObject.SetActive(!aspect.isActiveAndEnabled);
            }
        }

        foreach (var button in HudManager.Instance.GetComponentsInChildren<ActionButton>(true))
        {
            if (button.gameObject == null)
            {
                continue;
            }

            button.gameObject.SetActive(!button.isActiveAndEnabled);
            button.gameObject.transform.localScale *= scaleFactor;
            button.gameObject.SetActive(!button.isActiveAndEnabled);
        }

        if (baseButtons != null)
        {
            foreach (var arrange in baseButtons.GetComponentsInChildren<GridArrange>(true))
            {
                if (!arrange.gameObject || !arrange.transform)
                {
                    continue;
                }

                arrange.gameObject.SetActive(!arrange.isActiveAndEnabled);
                arrange.CellSize = new Vector2(scaleFactor, scaleFactor);
                arrange.gameObject.SetActive(!arrange.isActiveAndEnabled);
                if (arrange.isActiveAndEnabled && arrange.gameObject.transform.childCount != 0)
                {
                    try
                    {
                        arrange.ArrangeChilds();
                    }
                    catch
                    {
                        // Error($"Error arranging child objects in GridArrange: {e}");
                    }
                }
            }
        }
    }

    /*
    /// <summary>
    /// Trigger hudstart on current custom gamemode
    /// </summary>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.OnGameStart))]
    public static void GameStartPatch(HudManager __instance)
    {
        CustomGameModeManager.ActiveMode?.HudStart(__instance);
    }*/

    /// <summary>
    /// Create custom buttons and arrange them on the hud.
    /// </summary>
    /// <param name="__instance">The HudManager instance.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.Start))]
    public static void StartPostfix(HudManager __instance)
    {
        if (Buttons == null)
        {
            Buttons = __instance.transform.Find("Buttons");
        }

        if (BottomRight == null)
        {
            BottomRight = Buttons.Find("BottomRight");
        }

        if (BottomLeft == null)
        {
            BottomLeft = Object.Instantiate(BottomRight.gameObject, Buttons);
        }

        foreach (var t in BottomLeft.GetComponentsInChildren<ActionButton>(true))
        {
            t.gameObject.Destroy();
        }

        var gridArrange = BottomLeft.GetComponent<GridArrange>();
        var aspectPosition = BottomLeft.GetComponent<AspectPosition>();

        BottomLeft.name = "BottomLeft";
        gridArrange.Alignment = GridArrange.StartAlign.Right;
        aspectPosition.Alignment = AspectPosition.EdgeAlignments.LeftBottom;

        if (Constants.GetPlatformType() is Platforms.Android or Platforms.IPhone)
        {
            var fakeButton = Object.Instantiate(__instance.AbilityButton, BottomLeft.transform);
            foreach (var renderer in fakeButton.GetComponentsInChildren<Renderer>())
            {
                renderer.enabled = false;
            }

            fakeButton.buttonLabelText.Destroy();
            fakeButton.cooldownTimerText.Destroy();
            fakeButton.usesRemainingText.Destroy();
            fakeButton.GetComponent<PassiveButton>().Destroy();
            fakeButton.ToggleVisible(true);
            fakeButton.Destroy();
        }
        ModdedKeybindIcons = [];

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            var location = button.Location switch
            {
                ButtonLocation.BottomLeft => BottomLeft.transform,
                ButtonLocation.BottomRight => BottomRight,
                _ => null,
            };

            if (location is null)
            {
                continue;
            }

            try
            {
                button.CreateButton(location);
            }
            catch (System.Exception e)
            {
                Error($"Failed to create custom button {button.GetType().Name}: {e}");
            }
        }
        __instance.ImpostorVentButton.transform.SetParent(null);
        __instance.ImpostorVentButton.transform.SetParent(BottomRight.transform);

        gridArrange.Start();
        gridArrange.ArrangeChilds();
        aspectPosition.AdjustPosition();

        vanillaKeybindIcons = [];
        var keybindIconPos = new Vector3(0.4f, 0.4f, -9.5f);
        var vanillaButtons = new Dictionary<GameObject, int>
        {
            { __instance.KillButton.gameObject, 8 },
            { __instance.UseButton.gameObject, 6 },
            { __instance.ReportButton.gameObject, 7 },
            { __instance.ImpostorVentButton.gameObject, 50 },
            { __instance.SabotageButton.gameObject, 4 },
            { __instance.AbilityButton.gameObject, 49 },
        };

        foreach (var kvp in vanillaButtons)
        {
            var buttonObj = kvp.Key;
            var actionId = kvp.Value;

            var key = KeybindUtils.GetKeycodeByActionId(actionId);
            if (key == KeyboardKeyCode.None)
            {
                continue;
            }
            var icon = Helpers.CreateKeybindIcon(buttonObj, key, keybindIconPos);
            vanillaKeybindIcons.Add(icon.transform.GetChild(0).GetComponent<TextMeshPro>(), actionId);
            if (!MiraApiPlugin.IsMobile)
            {
                var comp = buttonObj.GetComponent<ActionButton>();
                KeybindManager.VanillaKeybinds[comp.GetType()].Button = comp;
            }
        }
        MiraApiSettings.OldButtonScaleFactor =
            LocalSettingsTabSingleton<MiraApiSettings>.Instance.ButtonUIFactorSlider.Value;
        Coroutines.Start(CoResizeUI());
    }

    /// <summary>
    /// Set the custom buttons active when the hud is active.
    /// </summary>
    /// <param name="__instance">HudManager instance.</param>
    /// <param name="localPlayer">The local PlayerControl.</param>
    /// <param name="role">The player's RoleBehaviour.</param>
    /// <param name="isActive">Whether the Hud should be set active or not.</param>
    [HarmonyPostfix]
    [HarmonyPatch(nameof(HudManager.SetHudActive), typeof(PlayerControl), typeof(RoleBehaviour), typeof(bool))]
    public static void SetHudActivePostfix(HudManager __instance, PlayerControl localPlayer, RoleBehaviour role, bool isActive)
    {
        __instance.AdminButton.ToggleVisible(isActive && role.IsImpostor && GameOptionsManager.Instance.CurrentGameOptions.GameMode == AmongUs.GameOptions.GameModes.HideNSeek);
        if (localPlayer.Data == null)
        {
            return;
        }

        foreach (var button in CustomButtonManager.CustomButtons)
        {
            try
            {
                button.SetActive(isActive, role);
            }
            catch (System.Exception e)
            {
                Error($"Failed to set custom button {button.GetType().Name} active: {e}");
            }
        }
    }

    [HarmonyPatch(nameof(HudManager.Update))]
    [HarmonyPostfix]
    public static void UpdatePostfix()
    {
        var canSeeBinds = ActiveInputManager.currentControlType == ActiveInputManager.InputType.Keyboard &&
                          LocalSettingsTabSingleton<MiraApiSettings>.Instance.ShowKeybinds.Value;
        foreach (var btnIcon in vanillaKeybindIcons)
        {
            btnIcon.Key.text = KeybindUtils.GetKeycodeByActionId(btnIcon.Value).ToString();
            btnIcon.Key.transform.parent.gameObject.SetActive(canSeeBinds);
        }
        foreach (var btnIcon in ModdedKeybindIcons)
        {
            btnIcon.transform.parent.gameObject.SetActive(canSeeBinds);
        }

        var player = ReInput.players.GetPlayer(0);
        foreach (var entry in KeybindManager.Keybinds)
        {
            var keyboard = player.controllers.Keyboard;
            bool modifierKeysPressed = entry.ModifierKeys.All(k => keyboard.GetModifierKey(k)) ||
                                       entry.ModifierKeys.Length <= 0;
            if (player.GetButtonDown(entry.Id) && modifierKeysPressed)
            {
                entry.Invoke();
            }
        }

        foreach (var entry in KeybindManager.VanillaKeybinds.Values.Where(x => player.GetButtonDown(x.Id)))
        {
            entry.Invoke();
        }
    }
}
