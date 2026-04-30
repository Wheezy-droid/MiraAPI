using HarmonyLib;
using System;
using System.Linq;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin
    {
        // This is the "brain" that tells the game which mod is active
        public static string ActiveMode = "Normal"; 

        [HarmonyPatch(typeof(global::MiraAPI.Options.TargetOptions), nameof(global::MiraAPI.Options.TargetOptions.Initialize))]
        [HarmonyPostfix]
        public static void SetupMenu()
        {
            // Creates a poll/selection in your menu
            global::MiraAPI.Options.TargetOptions.AddString("Game Mode", new[] { "Normal", "SNS", "Freeze Tag" }, "Normal");
        }

        [HarmonyPatch(typeof(global::MiraAPI.Options.GameOptionsManager), nameof(global::MiraAPI.Options.GameOptionsManager.Load))]
        [HarmonyPostfix]
        public static void ApplySelectedMode()
        {
            var options = global::MiraAPI.Options.GameOptionsManager.Instance.currentVars;
            if (options == null) return;

            // Logic for SNS
            if (ActiveMode == "SNS") {
                options.NumShapeshifters = 3;
                options.ShapeshifterChance = 100;
                options.ReportDistance = 0f;
                options.NumEmergencyMeetings = 0;
            }

            // Logic for Freeze Tag (from your TikTok rules)
            if (ActiveMode == "Freeze Tag") {
                options.NumImpostors = 2; // Taggers
                options.KillDistance = 0; // Must "touch" them
                options.ReportDistance = 0f; // No reporting bodies
                options.CanVent = false; // Impostors can't vent
            }
        }
    }

    // This makes the "Freezing" actually happen
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class FreezeLogic
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            if (ExamplePlugin.ActiveMode == "Freeze Tag")
            {
                // If a crewmate is 'killed' (tagged), we freeze their physics
                if (__instance.Data.IsDead && !__instance.Data.IsImpostor)
                {
                    __instance.MyPhysics.Speed = 0; 
                }
            }
        }
    }
}
