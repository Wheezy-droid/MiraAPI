using HarmonyLib;
using Hazel;

namespace MiraAPI.Example
{
    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class FreezePatch
    {
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            // Only run on host (REAL host check, not your fake one)
            if (!AmongUsClient.Instance || !AmongUsClient.Instance.AmHost)
                return;

            if (__instance == null || __instance.Data == null)
                return;

            var stats = __instance.Data;

            // Freeze dead crewmates
            if (stats.IsDead && !stats.IsImpostor)
            {
                // Better freeze handling
                __instance.moveable = false;
                __instance.MyPhysics?.body?.velocity = UnityEngine.Vector2.zero;
            }
            else
            {
                // Restore movement if needed
                __instance.moveable = true;
            }
        }
    }

    [HarmonyPatch(typeof(ChatController), nameof(ChatController.SendChat))]
    public static class ChatPatch
    {
        private static float lastUsedTime = 0f;
        private const float cooldown = 2f;

        [HarmonyPostfix]
        public static void Postfix(ChatController __instance)
        {
            if (__instance == null || PlayerControl.LocalPlayer == null)
                return;

            string msg = __instance.TextArea?.text?.Trim().ToLower();
            if (string.IsNullOrEmpty(msg))
                return;

            // Exact command check (no more "bruh = /r" stupidity)
            if (msg == "/r")
            {
                // Simple cooldown to prevent spam
                if (UnityEngine.Time.time - lastUsedTime < cooldown)
                    return;

                lastUsedTime = UnityEngine.Time.time;

                __instance.AddChat(
                    PlayerControl.LocalPlayer,
                    "MODE: SNS & Freeze Tag Active!"
                );
            }
        }
    }
}
