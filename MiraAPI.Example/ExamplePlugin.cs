using HarmonyLib;
using System;

// This is the shortcut that fixes the "Missing Reference" error
using GameOptions = global::MiraAPI.Options.GameOptionsManager;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin
    {
        [HarmonyPatch(typeof(GameOptions), nameof(GameOptions.Load))]
        [HarmonyPostfix]
        public static void Postfix()
        {
            var options = GameOptions.Instance.currentVars;
            if (options != null)
            {
                // SNS & Freeze Tag Logic
                options.NumShapeshifters = 3;
                options.ShapeshifterChance = 100;
                options.ReportDistance = 0f;
                options.CanVent = false; // Freeze Tag Rule
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (__instance != null && __instance.TextArea.text.ToLower().Contains("/r"))
            {
                __instance.AddChat(PlayerControl.LocalPlayer, "Rules: SNS/Freeze Mode Active!");
            }
        }
    }
}
