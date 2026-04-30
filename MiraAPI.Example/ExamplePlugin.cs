using HarmonyLib;
using System;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin
    {
        // This is the "Selector" that lets you choose the game mode in the lobby
        [HarmonyPatch(typeof(global::MiraAPI.Options.TargetOptions), nameof(global::MiraAPI.Options.TargetOptions.Initialize))]
        [HarmonyPostfix]
        public static void SetupMenu()
        {
            global::MiraAPI.Options.TargetOptions.AddString("Game Mode", new[] { "Normal", "SNS", "Freeze Tag" }, "Normal");
        }

        [HarmonyPatch(typeof(global::MiraAPI.Options.GameOptionsManager), nameof(global::MiraAPI.Options.GameOptionsManager.Load))]
        [HarmonyPostfix]
        public static void ApplySelectedMode()
        {
            var options = global::MiraAPI.Options.GameOptionsManager.Instance.currentVars;
            if (options == null) return;

            // --- SNS MODE LOGIC ---
            options.NumShapeshifters = 3;
            options.ShapeshifterChance = 100;
            options.ReportDistance = 0f;
            options.NumEmergencyMeetings = 0;

            // --- FREEZE TAG LOGIC (TikTok Rules) ---
            options.KillDistance = 0; // Must touch them to 'freeze'
            options.CanVent = false;   // Impostors/Taggers can't vent
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class FreezeLogic
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            // If someone is 'killed' in Freeze Tag, they just stop moving instead of dying
            if (__instance.Data.IsDead && !__instance.Data.IsImpostor)
            {
                __instance.MyPhysics.Speed = 0; 
            }
        }
    }
}
