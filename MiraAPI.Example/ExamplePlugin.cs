#nullable disable
using HarmonyLib;
using UnityEngine;
using TMPro;

namespace MiraAPI.Example
{
    public static class SNSConfig
    {
        public static bool Enabled = true;
        public static float Speed = 1.5f;
        public static string Command = "/sns";
        public static bool SNSActive = false;
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

            __instance.MyPhysics.Speed = SNSConfig.SNSActive ? SNSConfig.Speed : 1f;
        }
    }

    [HarmonyPatch(typeof(ChatController), "Update")]
    public static class SNSChatInputPatch
    {
        public static void Postfix(ChatController __instance)
        {
            if (!SNSConfig.Enabled) return;
            if (PlayerControl.LocalPlayer == null) return;
            if (__instance == null) return;

            TMP_InputField input = __instance?.chatText; // most builds use this
            if (input == null) return;

            if (!Input.GetKeyDown(KeyCode.Return)) return;

            string msg = input.text?.Trim().ToLower();
            if (string.IsNullOrEmpty(msg)) return;

            if (msg == SNSConfig.Command)
            {
                SNSConfig.SNSActive = !SNSConfig.SNSActive;

                input.text = "";

                __instance.AddChat(
                    PlayerControl.LocalPlayer,
                    "SNS Mode: " + (SNSConfig.SNSActive ? "ON" : "OFF")
                );
            }
        }
    }
}
