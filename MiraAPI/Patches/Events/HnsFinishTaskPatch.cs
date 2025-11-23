
using HarmonyLib;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Player;

namespace MiraAPI.Patches.Events;

[HarmonyPatch(typeof(HideAndSeekManager))]
public static class HnsFinishTaskPatch
{
    [HarmonyPrefix]
    [HarmonyPatch(nameof(HideAndSeekManager.FinishTask))]
    public static bool FinishTask(HideAndSeekManager __instance, PlayerTask task)
    {
        NormalPlayerTask normalPlayerTask = task as NormalPlayerTask;
        if (normalPlayerTask != null)
        {
            var completeTaskEvent = new CompleteHnsTaskEvent(task.Owner, task);
            MiraEventManager.InvokeEvent(completeTaskEvent);
            if (completeTaskEvent.IsCancelled)
            {
                return false;
            }
            switch (normalPlayerTask.Length)
            {
                case NormalPlayerTask.TaskLength.None:
                case NormalPlayerTask.TaskLength.Common:
                    __instance.LogicFlowHnS.OnTaskComplete(__instance.LogicOptionsHnS.GetCommonTaskTimeValue());
                    break;
                case NormalPlayerTask.TaskLength.Short:
                    __instance.LogicFlowHnS.OnTaskComplete(__instance.LogicOptionsHnS.GetShortTaskTimeValue());
                    break;
                case NormalPlayerTask.TaskLength.Long:
                    __instance.LogicFlowHnS.OnTaskComplete(__instance.LogicOptionsHnS.GetLongTaskTimeValue());
                    break;
            }

            SoundManager.Instance.PlaySoundImmediate(__instance.TaskFinishedSound, false, 1f, 1f, null);
        }

        return false;
    }
}
