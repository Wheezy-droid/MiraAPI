using System.Linq;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using MiraAPI.PluginLoading;
using Reactor;
using Reactor.Networking;
using Reactor.Networking.Attributes;

namespace MiraAPI.Example;

[BepInAutoPlugin("mira.example", "MiraExampleMod")]
[BepInProcess("Among Us.exe")]
[BepInDependency(ReactorPlugin.Id)]
[BepInDependency(MiraApiPlugin.Id)]
[ReactorModFlags(ModFlags.RequireOnAllClients)]
public partial class ExamplePlugin : BasePlugin, IMiraPlugin
{
    public Harmony Harmony { get; } = new(Id);
    public string OptionsTitleText => "Mira API\nExample Mod";
    public ConfigFile GetConfigFile() => Config;
    public override void Load()
    {
        ExampleEventHandlers.Initialize();
        Harmony.PatchAll();
    }
}
public override void Load() 
{
    // SNS RULE 1: No Sabotages (Cooldown is basically infinite)
    GameOptionsManager.Instance.currentVars.SabotageCooldown = 9999f;

    // SNS RULE 2: No Reporting (Distance set to 0)
    GameOptionsManager.Instance.currentVars.ReportDistance = 0f;

    // SNS RULE 3: Guaranteed Shapeshifter
    GameOptionsManager.Instance.currentVars.NumShapeshifters = 3;
    GameOptionsManager.Instance.currentVars.ShapeshifterChance = 100;
}
