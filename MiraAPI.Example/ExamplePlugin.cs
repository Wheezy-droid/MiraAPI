using HarmonyLib;
using MiraAPI;
using MiraAPI.Options;
using MiraAPI.Roles;
using System;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin : MiraPlugin
    {
        public override string OptionsTitleText => "Wheezy SNS Mode";

        public override void Load()
        {
            // Use the full path to make it easier for the compiler
            var options = MiraAPI.Options.GameOptionsManager.Instance.currentVars;
            
            if (options != null)
            {
                options.SabotageCooldown = 20f; 
                options.ReportDistance = 0f;
                options.NumEmergencyMeetings = 0;
                options.NumShapeshifters = 3;
                options.ShapeshifterChance = 100;
            }
            HarmonyLib.Harmony.PatchAll();
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
                __instance.AddChat(PlayerControl.LocalPlayer, "SNS: Shift-kill only. Comms only. No reports.");
            }
        }
    }
}
