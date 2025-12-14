using BepInEx.Configuration;
using MiraAPI.LocalSettings;
using MiraAPI.LocalSettings.Attributes;
using MiraAPI.Utilities.Assets;

namespace MiraAPI;

/// <summary>
/// Mira API Config File Handler
/// </summary>
public class MiraApiSettings(ConfigFile config) : LocalSettingsTab(config)
{
    /// <inheritdoc />
    public override string TabName => "Mira API";

    /// <inheritdoc />
    public override LocalSettingTabAppearance TabAppearance => new()
    {
        TabButtonHoverColor = MiraApiPlugin.MiraColor,
        TabIcon = MiraAssets.SettingsIcon,
        HideIconOnHover = false,
    };

    /// <summary>
    /// Gets whether the modifiers hud should be on the left side of the screen (under roles/task tab). Recommended for streamers.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> ModifiersHudLeftSide { get; private set; } = config.Bind("Displays", "Show Modifiers HUD on Left Side", false);

    /// <summary>
    /// Gets whether to show keybinds on buttons.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> ShowKeybinds { get; private set; } = config.Bind("Keybinds", "Show Keybinds on Buttons", true);
    // This would be placed in the keybinds menu, but it crashes for Epic Games users. - Atony

    /// <summary>
    /// Gets whether to apply cosmetic changes to the TaskAdder.
    /// </summary>
    [LocalToggleSetting]
    public ConfigEntry<bool> PrettyTaskAdder { get; private set; } = config.Bind("Freeplay", "Pretty Task Laptop", true);
}
