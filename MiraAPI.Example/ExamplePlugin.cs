#nullable disable
using HarmonyLib;
using UnityEngine;

namespace MiraAPI.Example
{
    public static class SNSConfig
    {
        public static bool Enabled = true;
        public static float PlayerSpeed = 1.75f;
        public static bool AllowGhostMovement = false;
        public static string ModeMessage = "SNS Mode Active!";
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

            __instance.MyPhysics.Speed = SNSConfig.PlayerSpeed;

            if (__instance.Data.IsDead && !SNSConfig.AllowGhostMovement)
            {
                __instance.moveable = false;

                if (__instance.MyPhysics != null && __instance.MyPhysics.body != null)
                {
                    __instance.MyPhysics.body.velocity = Vector2.zero;
                }
            }
            else
            {
                __instance.moveable = true;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), "SendChat")]
    public static class SNSChatPatch
    {
        private static float lastUsed = 0f;
        private const float cooldown = 2f;

        public static void Postfix(ChatController __instance)
        {
            if (!SNSConfig.Enabled) return;
            if (__instance == null) return;
            if (PlayerControl.LocalPlayer == null) return;

            string msg = "";

            if (__instance.TextArea != null)
                msg = __instance.TextArea.text;

            if (msg == null) return;

            msg = msg.Trim().ToLower();

            if (msg == "") return;

            if (msg == SNSConfig.Command)
            {
                if (Time.time - lastUsed < cooldown) return;

                lastUsed = Time.time;

                __instance.AddChat(
                    PlayerControl.LocalPlayer,
                    SNSConfig.ModeMessage
                );
            }
        }
    }
}
