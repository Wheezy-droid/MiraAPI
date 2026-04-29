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
using HarmonyLib;
using MiraAPI.Options;
using MiraAPI.Roles;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin : MiraPlugin
    {
        public override string OptionsTitleText => "Wheezy SNS Mode";

        public override void Load()
        {
            var options = GameOptionsManager.Instance.currentVars;
            
            // --- SNS RULES SETTINGS ---
            
            // 1. ALLOW COMMS, BLOCK OTHERS (SNS Rule)
            options.SabotageCooldown = 20f; 

            // 2. DISABLE REPORTS & EMERGENCY MEETINGS
            options.ReportDistance = 0f;
            options.NumEmergencyMeetings = 0;

            // 3. SHAPESHIFTER GUARANTEE
            options.NumShapeshifters = 3;
            options.ShapeshifterChance = 100;
            options.ShapeshiftCooldown = 10f;
            options.ShapeshiftDuration = 60f;

            Harmony.PatchAll();
            
        }
    }
}
    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        public static void Postfix(ChatController __instance)
        {
            // This checks if the player typed /r
            if (__instance.TextArea.text.ToLower() == "/r")
            {
                __instance.AddChat(PlayerControl.LocalPlayer, "SNS RULES: Shift-kill only. Comms only. No reports.");
            }
        }
    }

        } // End of the ChatPatch
    } // End of the whole Mod
} // End of the Namespace
 
