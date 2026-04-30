#nullable disable
using HarmonyLib;
using UnityEngine;

namespace MiraAPI.Example
{
    public static class SNSConfig
    {
        public static bool Enabled = true;
        public static float Speed = 1.5f;
        public static string Command = "/sns";
    }

    [HarmonyPatch(typeof(PlayerControl), "FixedUpdate")]
    public static class SNSPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            if (!SNSConfig.Enabled) return;
            if (__instance == null) return;
            if (__instance.Data == null) return;

            if (AmongUsClient.Instance == null) return;
            if (!AmongUsClient.Instance.AmHost) return;

            __instance.MyPhysics.Speed = SNSConfig.Speed;
        }
    }

    [HarmonyPatch(typeof(ChatController), "SendChat")]
    public static class SNSChat
    {
        public static void Postfix(ChatController __instance)
        {
            if (!SNSConfig.Enabled) return;
            if (__instance == null) return;
            if (PlayerControl.LocalPlayer == null) return;

            if (__instance.TextArea == null) return;

            string msg = __instance.TextArea.text;
            if (msg == null) return;

            msg = msg.Trim().ToLower();

            if (msg == SNSConfig.Command)
            {
                __instance.AddChat(
                    PlayerControl.LocalPlayer,
                    "SNS Mode is ON"
                );
            }
        }
    }
}
