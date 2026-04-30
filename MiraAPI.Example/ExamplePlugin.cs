using HarmonyLib;
using UnityEngine;

namespace MiraAPI.Example
{
    public static class SNSConfig
    {
        public static bool Enabled = true;

        // Customizable settings
        public static float PlayerSpeed = 1.75f;
        public static bool AllowGhostMovement = false;
        public static string ModeMessage = "SNS Mode Active!";
        public static string Command = "/sns";
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class SNSPatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            if (!SNSConfig.Enabled)
                return;

            if (__instance == null || __instance.Data == null)
                return;

            // Only host controls game logic (prevents desync mess)
            if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
                return;

            var data = __instance.Data;

            // Apply custom speed
            __instance.MyPhysics.Speed = SNSConfig.PlayerSpeed;

            // Optional: disable ghost movement
            if (data.IsDead && !SNSConfig.AllowGhostMovement)
            {
                __instance.moveable = false;
                __instance.MyPhysics?.body?.velocity = Vector2.zero;
            }
            else
            {
                __instance.moveable = true;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class SNSChatPatch
    {
        private static float lastUsed = 0f;
        private const float cooldown = 2f;

        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (!SNSConfig.Enabled)
                return;

            if (__instance == null || PlayerControl.LocalPlayer == null)
                return;

            string msg = __instance.TextArea?.text?.Trim().ToLower();
            if (string.IsNullOrEmpty(msg))
                return;

            if (msg == SNSConfig.Command)
            {
                if (Time.time - lastUsed < cooldown)
                    return;

                lastUsed = Time.time;

                __instance
