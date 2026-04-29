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

    [HarmonyPatch(typeof(AmongUsClient), nameof(AmongUsClient.OnPlayerJoined))]
    public static class JoinPatch
    {
        [HarmonyPostfix]
        public static void Postfix()
        {
            if (PlayerControl.LocalPlayer != null)
            {
                ChatMessageManager.Instance.SendChat("WELCOME! SNS MODE: Shift-kill only. Comms only. No reports! Type /r for rules.");
            }
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
                __instance.AddChat(PlayerControl.LocalPlayer, "RULES: 1. Shift-kill only 2. Comms only 3. No reports/meetings.");
            }
        }
    }
}
