using BepInEx.Configuration;

namespace MiraAPI.PluginLoading;

/// <summary>
/// The interface that all Mira plugins must implement.
/// </summary>
public interface IMiraPlugin
{
    /// <summary>
    /// Gets the name to display on the options menu.
    /// </summary>
    string OptionsTitleText { get; }

    /// <summary>
    /// Gets the name for the first custom category in the game options menu, if any.
    /// </summary>
    public virtual string CustomOptionMenuNameOne => "Custom Category 1";

    /// <summary>
    /// Gets the name for the second custom category in the game options menu, if any.
    /// </summary>
    public virtual string CustomOptionMenuNameTwo => "Custom Category 2";

    /// <summary>
    /// Gets the description for the second custom category in the game options menu, if any.
    /// </summary>
    public virtual string ModifierMenuDescription => "Configure modifiers and their settings here!";

    /// <summary>
    /// Gets the description for the first custom category in the game options menu, if any.
    /// </summary>
    public virtual string CustomOptionMenuOneDescription => "Apply game settings for this mod!";

    /// <summary>
    /// Gets the description for the second custom category in the game options menu, if any.
    /// </summary>
    public virtual string CustomOptionMenuTwoDescription => "Apply game settings for this mod!";

    /// <summary>
    /// Gets the BepInEx configuration file for the plugin.
    /// </summary>
    /// <returns>The BepInEx configuration file for the plugin.</returns>
    public ConfigFile GetConfigFile();
}
