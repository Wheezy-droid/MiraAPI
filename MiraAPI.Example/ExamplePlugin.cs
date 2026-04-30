using HarmonyLib;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin
    {
        // 1. Adds the Mode Selector to the Lobby Menu
        [HarmonyPatch(typeof(global::MiraAPI.Options.TargetOptions), nameof(global::MiraAPI.Options.TargetOptions.Initialize))]
        [HarmonyPostfix]
        public static void SetupMenu()
        {
            global::MiraAPI.Options.TargetOptions.AddString("Game Mode", new[] { "Normal", "SNS", "Freeze Tag" }, "Normal");
        }

        // 2. Changes the game rules based on what you picked
        [HarmonyPatch(typeof(global::MiraAPI.Options.GameOptionsManager), nameof(global::MiraAPI.Options.GameOptionsManager.Load))]
        [HarmonyPostfix]
        public static void ApplySelectedMode()
        {
            var options = global::MiraAPI.Options.GameOptionsManager.Instance.currentVars;
            if (options == null) return;

            // SNS Rules
            options.NumShapeshifters = 3;
            options.ShapeshifterChance = 100;
            options.ReportDistance = 0f;
            options.NumEmergencyMeetings = 0;

            // Freeze Tag Rules (TikTok Rules)
            options.KillDistance = 0; // Touch to freeze
            options.CanVent = false;   // Taggers can't vent
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class FreezeLogic
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            // If they are 'dead' (tagged), they stop moving
            if (__instance.Data.IsDead && !__instance.Data.IsImpostor)
            {
                __instance.MyPhysics.Speed = 0; 
            }
        }
    }
}
