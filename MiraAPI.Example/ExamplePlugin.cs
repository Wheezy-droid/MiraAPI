using HarmonyLib;
using System;

namespace MiraAPI.Example
{
    // 1. Using [HarmonyPatch] without ': MiraPlugin' to avoid the CS0246 error
    [HarmonyPatch]
    public class ExamplePlugin 
    {
        [HarmonyPatch(typeof(global::MiraAPI.Options.GameOptionsManager), nameof(global::MiraAPI.Options.GameOptionsManager.Load))]
        [HarmonyPostfix]
        public static void Load()
        {
            var options = global::MiraAPI.Options.GameOptionsManager.Instance.currentVars;
            if (options != null)
            {
                // SNS Rules
                options.SabotageCooldown = 20f; 
                options.ReportDistance = 0f;
                options.NumEmergencyMeetings = 0;
                options.NumShapeshifters = 3;
                options.ShapeshifterChance = 100;
                
                // Freeze Tag (No venting)
                options.CanVent = false;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            // This is the cool part the AI wrote - type /r to show rules!
            if (__instance != null && __instance.TextArea.text.ToLower().Contains("/r"))
            {
                __instance.AddChat(PlayerControl.LocalPlayer, "MODE: SNS/Freeze Tag. No reports. No venting!");
            }
        }
    }
}
