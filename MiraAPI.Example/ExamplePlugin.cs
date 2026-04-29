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
            options.SabotageCooldown = 20f; 
            options.ReportDistance = 0f;
            options.NumEmergencyMeetings = 0;
            options.NumShapeshifters = 3;
            options.ShapeshifterChance = 100;
            Harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (__instance.TextArea.text.ToLower().Contains("/r"))
            {
                __instance.AddChat(PlayerControl.LocalPlayer, "SNS: Shift-kill only. Comms only. No reports.");
            }
        }
    }
}
