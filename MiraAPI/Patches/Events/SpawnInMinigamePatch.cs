/*
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(SpawnInMinigame), nameof(SpawnInMinigame.Close))]
internal static class SpawnInMinigamePatch
{
    public static void Postfix()
    {
        var @event = new BeforeRoundStartEvent(false);
        MiraEventManager.InvokeEvent(@event);

        if (@event.IsCancelled) return;

        var @event2 = new RoundStartEvent(false);
        MiraEventManager.InvokeEvent(@event2);
    }
}
*/
