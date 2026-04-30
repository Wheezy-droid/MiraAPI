#nullable disable
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
            if (UnityEngine.Time.time - lastUsed < cooldown)
                return;

            lastUsed = UnityEngine.Time.time;

            __instance.AddChat(
                PlayerControl.LocalPlayer,
                SNSConfig.ModeMessage
            );
        }
    }
}
