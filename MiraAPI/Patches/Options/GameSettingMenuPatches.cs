using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using MiraAPI.GameOptions;
using MiraAPI.Modifiers;
using MiraAPI.PluginLoading;
using MiraAPI.Presets;
using MiraAPI.Utilities.Assets;
using Reactor.Utilities.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using static UnityEngine.UI.Button;
using Object = UnityEngine.Object;

namespace MiraAPI.Patches.Options;

[HarmonyPatch(typeof(GameSettingMenu))]
internal static class GameSettingMenuPatches
{
    public static int SelectedModIdx { get; private set; }

    public static MiraPluginInfo? SelectedMod { get; private set; }

    private static TextMeshPro? _text;

    private static Vector3 _roleBtnOgPos;
    private static Vector3 _smallRoleBtnOgPos;
    private static Vector3 _modifierBtnOgPos;
    private static Vector3 _customOneBtnOgPos;
    private static Vector3 _customTwoBtnOgPos;

    private static GameOptionsMenu? _modifiersTab;
    private static PassiveButton? _modifiersButton;
    private static PassiveButton? _smallRolesButton;
    private static GameOptionsMenu? _customOneTab;
    private static PassiveButton? _customOneButton;
    private static GameOptionsMenu? _customTwoTab;
    private static PassiveButton? _customTwoButton;
    private static GameObject? _nextModButton;
    private static GameObject? _previousModButton;

    private static Dictionary<int, Vector3> OptionsPositions { get; } = [];
    private static Dictionary<int, Vector3> ModifiersPositions { get; } = [];
    private static Dictionary<int, Vector3> CustomOnePositions { get; } = [];
    private static Dictionary<int, Vector3> CustomTwoPositions { get; } = [];

    private static void SaveScrollPositions(GameSettingMenu gameSettingMenu)
    {
        if (_modifiersTab)
        {
            ModifiersPositions[SelectedModIdx] = _modifiersTab!.scrollBar.Inner.localPosition;
        }
        else if (_customOneTab)
        {
            CustomOnePositions[SelectedModIdx] = _customOneTab!.scrollBar.Inner.localPosition;
        }
        else if (_customTwoTab)
        {
            CustomTwoPositions[SelectedModIdx] = _customTwoTab!.scrollBar.Inner.localPosition;
        }

        RoleSettingMenuPatches.RolePositions[SelectedModIdx] = gameSettingMenu.RoleSettingsTab.scrollBar.Inner.localPosition;
        OptionsPositions[SelectedModIdx] = gameSettingMenu.GameSettingsTab.scrollBar.Inner.localPosition;
    }

    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]

    public static void ChangeTabPrefix(GameSettingMenu __instance)
    {
        SaveScrollPositions(__instance);
    }

    [HarmonyPostfix]
    [HarmonyPatch(nameof(GameSettingMenu.ChangeTab))]
    public static void ChangeTabPostfix(GameSettingMenu __instance, int tabNum, bool previewOnly)
    {
        if ((previewOnly && Controller.currentTouchType == Controller.TouchType.Joystick) || !previewOnly)
        {
            if (_modifiersTab)
            {
                _modifiersTab!.gameObject.SetActive(tabNum == 3);
            }

            if (_customOneTab)
            {
                _customOneTab!.gameObject.SetActive(tabNum == 4);
            }

            if (_customTwoTab)
            {
                _customTwoTab!.gameObject.SetActive(tabNum == 5);
            }

            _modifiersButton?.SelectButton(tabNum == 3);
            _smallRolesButton?.SelectButton(tabNum == 2);
            _customOneButton?.SelectButton(tabNum == 4);
            _customTwoButton?.SelectButton(tabNum == 5);
            if (tabNum >= 3 && tabNum <= 5)
            {
                if (__instance.RoleSettingsButton.gameObject.active)
                {
                    __instance.RoleSettingsButton.SelectButton(true);
                }
                else
                {
                    switch (tabNum)
                    {
                        case 3:
                            _modifiersButton?.SelectButton(true);
                            __instance.MenuDescriptionText.text = SelectedMod != null ? SelectedMod.MiraPlugin.ModifierMenuDescription : "Configure modifiers and their settings here!";
                            break;
                        case 4:
                            _customOneButton?.SelectButton(true);
                            __instance.MenuDescriptionText.text = SelectedMod != null ? SelectedMod.MiraPlugin.CustomOptionMenuOneDescription : "Apply game settings for this mod!";
                            break;
                        case 5:
                            _customTwoButton?.SelectButton(true);
                            __instance.MenuDescriptionText.text = SelectedMod != null ? SelectedMod.MiraPlugin.CustomOptionMenuTwoDescription : "Apply game settings for this mod!";
                            break;
                    }
                }
            }
        }

        if (previewOnly)
        {
            return;
        }

        _modifiersButton?.SelectButton(tabNum == 3);
        _smallRolesButton?.SelectButton(tabNum == 2);
        _customOneButton?.SelectButton(tabNum == 4);
        _customTwoButton?.SelectButton(tabNum == 5);

        if (tabNum >= 3 && tabNum <= 5)
        {
            if (__instance.RoleSettingsButton.gameObject.active)
            {
                __instance.RoleSettingsButton.SelectButton(true);
            }
            else
            {
                switch (tabNum)
                {
                    case 3:
                        _modifiersButton?.SelectButton(true);
                        break;
                    case 4:
                        _customOneButton?.SelectButton(true);
                        break;
                    case 5:
                        _customTwoButton?.SelectButton(true);
                        break;
                }
            }
        }
    }

    /// <summary>
    /// Prefix for the <see cref="GameSettingMenu.Start"/> method. Sets up the custom options.
    /// </summary>
    /// <param name="__instance">The GameSettingMenu instance.</param>
    [HarmonyPrefix]
    [HarmonyPatch(nameof(GameSettingMenu.Start))]
    public static void StartPrefix(GameSettingMenu __instance)
    {
        _roleBtnOgPos = __instance.RoleSettingsButton.transform.localPosition;
        __instance.transform.FindChild("GameSettingsLabel").gameObject.SetActive(false);

        var helpThing = __instance.transform.FindChild("What Is This?");
        var tmpText = Object.Instantiate(helpThing.transform.FindChild("InfoText"), helpThing.parent).gameObject;

        tmpText.GetComponent<TextTranslatorTMP>().Destroy();
        tmpText.name = "SelectedMod";
        tmpText.transform.localPosition = new Vector3(-3.3382f, 1.5399f, -2);

        _text = tmpText.GetComponent<TextMeshPro>();
        _text.fontSizeMax = 3.2f;
        _text.overflowMode = TextOverflowModes.Overflow;

        _text.alignment = TextAlignmentOptions.Center;

        _nextModButton = Object.Instantiate(__instance.BackButton, __instance.BackButton.transform.parent).gameObject;
        _nextModButton.transform.localPosition = new Vector3(-2.2663f, 1.5272f, -25f);
        _nextModButton.name = "RightArrowButton";
        _nextModButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButton.LoadAsset();
        _nextModButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().sprite =
            MiraAssets.NextButtonActive.LoadAsset();
        _nextModButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().DestroyImmediate();

        var passiveButton = _nextModButton.gameObject.GetComponent<PassiveButton>();
        passiveButton.OnClick = new ButtonClickedEvent();
        passiveButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                SaveScrollPositions(__instance);
                SelectedModIdx += 1;
                if (SelectedModIdx > MiraPluginManager.Instance.RegisteredPlugins.Length)
                {
                    SelectedModIdx = 0;
                }

                UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
            }));

        _previousModButton = Object.Instantiate(_nextModButton, __instance.BackButton.transform.parent).gameObject;
        _previousModButton.transform.localPosition = new Vector3(-4.4209f, 1.5272f, -25f);
        _previousModButton.name = "LeftArrowButton";
        _previousModButton.gameObject.GetComponent<CloseButtonConsoleBehaviour>().Destroy();
        _previousModButton.transform.FindChild("Active").gameObject.GetComponent<SpriteRenderer>().flipX =
            _previousModButton.transform.FindChild("Inactive").gameObject.GetComponent<SpriteRenderer>().flipX = true;
        _previousModButton.gameObject.GetComponent<PassiveButton>().OnClick.AddListener(
            (UnityAction)(() =>
            {
                SaveScrollPositions(__instance);
                SelectedModIdx -= 1;
                if (SelectedModIdx < 0)
                {
                    SelectedModIdx = MiraPluginManager.Instance.RegisteredPlugins.Length;
                }

                UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
            }));

        // clone game settings tab for modifiers
        _modifiersTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        _modifiersTab.name = "MODIFIERS TAB";

        // clone game settings tab for both custom tabs
        _customOneTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        _customOneTab.name = "CUSTOM TAB 1";
        _customTwoTab = Object.Instantiate(__instance.GameSettingsTab, __instance.GameSettingsTab.transform.parent);
        _customTwoTab.name = "CUSTOM TAB 2";

        // create button for the first custom category
        var customPos = new Vector3(
            __instance.RoleSettingsButton.transform.localPosition.x,
            __instance.RoleSettingsButton.transform.localPosition.y,
            __instance.RoleSettingsButton.transform.localPosition.z);
        _customOneButton = Object.Instantiate(__instance.RoleSettingsButton, __instance.RoleSettingsButton.transform.parent);
        _customOneButton.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        _customOneButton.OnClick = new ButtonClickedEvent();
        _customOneButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(4, false);
            }));
        _customOneButton.OnMouseOver = new UnityEvent();
        _customOneButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(4, true);
            }));

        _customOneButton.buttonText.text = SelectedMod != null ? SelectedMod.MiraPlugin.CustomOptionMenuNameOne : "Custom Category 1";
        customPos.y -= 0.637f;
        _customOneButton.transform.localPosition = customPos;
        _customOneButton.name = "CustomOneButton";

        _customOneBtnOgPos = _customOneButton.transform.localPosition;

        // create button for the first custom category
        _customTwoButton = Object.Instantiate(_customOneButton, _customOneButton.transform.parent);
        _customTwoButton.OnClick = new ButtonClickedEvent();
        _customTwoButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(5, false);
            }));
        _customTwoButton.OnMouseOver = new UnityEvent();
        _customTwoButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(5, true);
            }));

        _customTwoButton.buttonText.text = SelectedMod != null ? SelectedMod.MiraPlugin.CustomOptionMenuNameTwo : "Custom Category 2";
        customPos.y -= 0.637f;
        _customTwoButton.transform.localPosition = customPos;
        _customTwoButton.name = "CustomTwoButton";

        _customTwoBtnOgPos = _customTwoButton.transform.localPosition;

        // create button for modifiers
        __instance.RoleSettingsButton.buttonText.gameObject.GetComponent<TextTranslatorTMP>().Destroy();
        __instance.RoleSettingsButton.gameObject.SetActive(false);
        _smallRolesButton = Object.Instantiate(
            __instance.RoleSettingsButton,
            __instance.RoleSettingsButton.transform.parent);
        var pos = new Vector3(
            -3.65f,
            _smallRolesButton.transform.localPosition.y,
            _smallRolesButton.transform.localPosition.z);
        _smallRolesButton.transform.localPosition = pos;
        _smallRolesButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(2, false);
            }));
        _smallRolesButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(2, true);
            }));

        _smallRoleBtnOgPos = _smallRolesButton.transform.localPosition;

        var roleText = _smallRolesButton.buttonText;
        roleText.text = "Roles";
        roleText.GetComponent<TextTranslatorTMP>().Destroy();
        roleText.alignment = TextAlignmentOptions.Center;
        roleText.transform.parent.localPosition = new Vector3(
            -.525f,
            roleText.transform.parent.localPosition.y,
            roleText.transform.parent.localPosition.z);

        foreach (var collider in _smallRolesButton.Colliders)
        {
            if (collider.TryCast<BoxCollider2D>() is { } col)
            {
                col.size = new Vector2(col.size.x / 2, col.size.y);
            }
        }

        foreach (var rend in _smallRolesButton.GetComponentsInChildren<SpriteRenderer>(true))
        {
            rend.size = new Vector2(rend.size.x / 2, rend.size.y);
        }

        _modifiersButton = Object.Instantiate(_smallRolesButton, _smallRolesButton.transform.parent);
        _modifiersButton.OnClick = new ButtonClickedEvent();
        _modifiersButton.OnClick.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(3, false);
            }));
        _modifiersButton.OnMouseOver = new UnityEvent();
        _modifiersButton.OnMouseOver.AddListener(
            (UnityAction)(() =>
            {
                __instance.ChangeTab(3, true);
            }));

        _modifiersButton.buttonText.text = "Modifiers";
        pos.x = -2.27f;
        _modifiersButton.transform.localPosition = pos;
        _modifiersButton.name = "ModifiersButton";

        _modifierBtnOgPos = _modifiersButton.transform.localPosition;

        foreach (var plugin in MiraPluginManager.Instance.RegisteredPlugins)
        {
            PresetManager.LoadPresets(plugin);
        }

        UpdateText(__instance, __instance.GameSettingsTab, __instance.RoleSettingsTab);
    }

    private static void ChangeRoleSettingButton(bool replace, GameSettingMenu menu)
    {
        menu.RoleSettingsButton.OnClick = new ButtonClickedEvent();
        menu.RoleSettingsButton.OnMouseOver = new UnityEvent();

        if (replace)
        {
            menu.RoleSettingsButton.buttonText.text = "Modifiers Settings";
            menu.RoleSettingsButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    menu.ChangeTab(3, false);
                }));
            menu.RoleSettingsButton.OnMouseOver.AddListener(
                (UnityAction)(() =>
                {
                    menu.ChangeTab(3, true);
                }));
        }
        else
        {
            menu.RoleSettingsButton.buttonText.text = "Roles Settings";
            menu.RoleSettingsButton.OnClick.AddListener(
                (UnityAction)(() =>
                {
                    menu.ChangeTab(2, false);
                }));
            menu.RoleSettingsButton.OnMouseOver.AddListener(
                (UnityAction)(() =>
                {
                    menu.ChangeTab(2, true);
                }));
        }
    }

    private static void UpdateText(GameSettingMenu menu, GameOptionsMenu settings, RolesSettingsMenu roles)
    {
        if (_text is not null && SelectedModIdx == 0)
        {
            _text.text = $"<size=40%>(Page 0/{MiraPluginManager.Instance.RegisteredPlugins.Length})</size>\nDefault";
            _text.fontSizeMax = 3.2f;
            SelectedMod = null;
        }
        else if (_text is not null)
        {
            _text.fontSizeMax = 2.3f;
            SelectedMod = MiraPluginManager.Instance.RegisteredPlugins[SelectedModIdx - 1];

            var name = SelectedMod.MiraPlugin.OptionsTitleText;
            _text.text = $"<size=50%>(Page {SelectedModIdx}/{MiraPluginManager.Instance.RegisteredPlugins.Length})</size>\n" + name[..Math.Min(name.Length, 25)];
        }

        bool replaceWithModifiers = true;
        _smallRolesButton!.transform.localPosition = _smallRoleBtnOgPos;
        _modifiersButton!.transform.localPosition = _modifierBtnOgPos;
        _customOneButton!.transform.localPosition = _customOneBtnOgPos;
        _customTwoButton!.transform.localPosition = _customTwoBtnOgPos;
        menu.RoleSettingsButton.transform.localPosition = _roleBtnOgPos;

        if (SelectedModIdx != 0)
        {
            _customOneButton.buttonText.text = SelectedMod!.MiraPlugin.CustomOptionMenuNameOne;
            _customTwoButton.buttonText.text = SelectedMod!.MiraPlugin.CustomOptionMenuNameTwo;
            var modHasRoles = SelectedMod!.InternalRoles.Count != 0;
            var modHasCustomOne = SelectedMod.InternalOptionGroups.Exists(
                x => x.ParentMenu == MenuCategory.CustomOne);
            var modHasCustomTwo = SelectedMod.InternalOptionGroups.Exists(
                x => x.ParentMenu == MenuCategory.CustomTwo);
            var modHasModifiers = SelectedMod.InternalOptionGroups.Exists(
                x => x.ShowInModifiersMenu || x.ParentMenu == MenuCategory.Modifiers || x.OptionableType?.IsAssignableTo(typeof(BaseModifier)) == true);
            var modHasOptions =
                SelectedMod.InternalOptionGroups.Exists(x => x.OptionableType == null && (!x.ShowInModifiersMenu ||
                    (x.ParentMenu != MenuCategory.Modifiers && x.ParentMenu != MenuCategory.Roles)));

            _modifiersButton.gameObject.SetActive(true);
            _smallRolesButton.gameObject.SetActive(true);
            _customOneButton.gameObject.SetActive(true);
            _customTwoButton.gameObject.SetActive(true);
            menu.GameSettingsButton.gameObject.SetActive(true);
            menu.RoleSettingsButton.gameObject.SetActive(false);
            var defaultButton = menu.RoleSettingsButton;

            // If there are no registered custom roles in the selected mod, hide the button.
            if (!modHasRoles)
            {
                _smallRolesButton.gameObject.SetActive(false);

                if (roles.gameObject.active)
                {
                    menu.ChangeTab(0, false);
                }

                // If the mod has modifiers, we can disable the smaller modifier button and enable the bigger one.
                if (modHasModifiers)
                {
                    _modifiersButton.gameObject.SetActive(false);
                    menu.RoleSettingsButton.gameObject.SetActive(true);
                    defaultButton = _modifiersButton;

                    if (_modifiersTab!.gameObject.active)
                    {
                        menu.RoleSettingsButton.SelectButton(true);
                    }
                }
            }

            // If there are no modifiers in the selected mod, hide the modifiers button.
            if (!modHasModifiers)
            {
                replaceWithModifiers = false;
                _modifiersButton.gameObject.SetActive(false);

                if (_modifiersTab!.gameObject.active)
                {
                    menu.ChangeTab(0, false);
                }

                // If the mod has roles, we can enable the bigger role button and disable the small one.
                if (modHasRoles)
                {
                    defaultButton = menu.RoleSettingsButton;
                    menu.RoleSettingsButton.gameObject.SetActive(true);
                    _smallRolesButton.gameObject.SetActive(false);
                }
            }

            // If there are no custom game options registered (that aren't modifier options), hide game settings button.
            if (!modHasOptions)
            {
                menu.GameSettingsButton.gameObject.SetActive(false);

                if (settings.gameObject.active)
                {
                    menu.ChangeTab(0, false);
                }
                defaultButton = menu.RoleSettingsButton;

                // If the mod has roles and modifiers, we can move their buttons to the game settings button position, since nothing is there.
                if (menu.RoleSettingsButton.gameObject.active)
                {
                    menu.RoleSettingsButton.transform.localPosition = menu.GameSettingsButton.transform.localPosition;
                }
                else if (_modifiersButton.gameObject.active && _smallRolesButton.gameObject.active)
                {
                    _modifiersButton.transform.localPosition = new Vector3(
                        _modifiersButton.transform.localPosition.x,
                        menu.GameSettingsButton.transform.localPosition.y,
                        _modifiersButton.transform.localPosition.z);

                    _smallRolesButton.transform.localPosition = new Vector3(
                        _smallRolesButton.transform.localPosition.x,
                        menu.GameSettingsButton.transform.localPosition.y,
                        _smallRolesButton.transform.localPosition.z);
                }
            }

            // If the mod has no primary custom tab, we can go ahead and hide the button.
            if (!modHasCustomOne)
            {
                _customOneButton.gameObject.SetActive(false);
                if (_customOneTab!.gameObject.active)
                {
                    defaultButton.SelectButton(true);
                }
            }
            // If the mod has no secondary custom tab, we can go ahead and hide the button.
            if (!modHasCustomTwo)
            {
                _customTwoButton.gameObject.SetActive(false);
                if (_customTwoTab!.gameObject.active)
                {
                    defaultButton.SelectButton(true);
                }
            }
        }
        else
        {
            _modifiersButton.gameObject.SetActive(false);
            _smallRolesButton.gameObject.SetActive(false);
            _customOneButton.gameObject.SetActive(false);
            _customTwoButton.gameObject.SetActive(false);
            menu.RoleSettingsButton.gameObject.SetActive(true);
            menu.GameSettingsButton.gameObject.SetActive(true);
            replaceWithModifiers = false;

            if (_modifiersTab!.gameObject.active || _customOneTab!.gameObject.active || _customTwoTab!.gameObject.active)
            {
                menu.ChangeTab(0, false);
            }
        }

        if (menu.RoleSettingsButton.gameObject.active)
        {
            ChangeRoleSettingButton(replaceWithModifiers, menu);
        }

        if (menu.PresetsTab.gameObject.active)
        {
            menu.ChangeTab(0, false);
        }

        CleanupTab(settings, roles);
    }

    private static void ClearOptions(Il2CppSystem.Collections.Generic.List<OptionBehaviour> options)
    {
        foreach (var child in options)
        {
            if (child.TryCast<GameOptionsMapPicker>() || !child.gameObject)
            {
                continue;
            }

            child.gameObject.DestroyImmediate();
        }

        options.Clear();
    }

    private static void CleanupTab(GameOptionsMenu settings, RolesSettingsMenu roles)
    {
        if (roles.roleChances != null)
        {
            CleanupRoleSettings(roles);
        }

        if (settings.Children != null)
        {
            CleanupSettings(settings);
        }

        if (_modifiersTab?.Children?.Count > 0)
        {
            CleanupSettings(_modifiersTab, MenuCategory.Modifiers);
        }

        if (_customOneTab?.Children?.Count > 0)
        {
            CleanupSettings(_customOneTab, MenuCategory.CustomOne);
        }

        if (_customTwoTab?.Children?.Count > 0)
        {
            CleanupSettings(_customTwoTab, MenuCategory.CustomTwo);
        }

        void CleanupRoleSettings(RolesSettingsMenu rolesMenu)
        {
            if (rolesMenu.advancedSettingChildren != null)
            {
                ClearOptions(rolesMenu.advancedSettingChildren);
                rolesMenu.advancedSettingChildren = null;
            }

            rolesMenu.RoleChancesSettings
                .transform
                .GetComponentsInChildren<CategoryHeaderEditRole>()
                .ToList()
                .ForEach(header => header.gameObject.DestroyImmediate());

            foreach (var role in rolesMenu.roleChances)
            {
                role.gameObject.DestroyImmediate();
            }

            rolesMenu.roleChances?.Clear();

            rolesMenu.AdvancedRolesSettings.gameObject.SetActive(false);
            rolesMenu.RoleChancesSettings.gameObject.SetActive(true);
            rolesMenu.SetQuotaTab();
            if (RoleSettingMenuPatches.RolePositions.TryGetValue(SelectedModIdx, out var pos))
            {
                rolesMenu.scrollBar.Inner.localPosition = pos;
                rolesMenu.scrollBar.UpdateScrollBars();
            }
            else
            {
                rolesMenu.scrollBar.ScrollToTop();
            }
        }

        void CleanupSettings(GameOptionsMenu gameOptMenu, MenuCategory menuCategory = MenuCategory.Roles)
        {
            ClearOptions(gameOptMenu.Children);
            gameOptMenu.Children = null;

            gameOptMenu.settingsContainer
                .GetComponentsInChildren<CategoryHeaderMasked>()
                .ToList()
                .ForEach(header => header.gameObject.DestroyImmediate());

            gameOptMenu.Initialize();
            var positions = OptionsPositions;
            switch (menuCategory)
            {
                case MenuCategory.Modifiers:
                    positions = ModifiersPositions;
                    break;
                case MenuCategory.CustomOne:
                    positions = CustomOnePositions;
                    break;
                case MenuCategory.CustomTwo:
                    positions = CustomTwoPositions;
                    break;
            }
            if (positions.TryGetValue(SelectedModIdx, out var pos))
            {
                gameOptMenu.scrollBar.Inner.localPosition = pos;
                gameOptMenu.scrollBar.UpdateScrollBars();
            }
            else
            {
                gameOptMenu.scrollBar.ScrollToTop();
            }
        }
    }
}
