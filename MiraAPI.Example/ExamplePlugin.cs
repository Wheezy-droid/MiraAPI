using HarmonyLib;
using MiraAPI.Options; // This is the 'dictionary' for the menu

namespace MiraAPI.Example
{
    public class ExamplePlugin : MiraPlugin
    {
        // 1. Create the setting variable
        public static ToggleOption MuhsinGodMode;

        public override void Load()
        {
            // 2. Add the setting to the menu
            MuhsinGodMode = TargetOptions.AddToggle("Muhsin's God Mode", false);

            // 3. Tell the game what to do when it's ON
            Harmony.PatchAll();
        }
    }

    [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
    public static class SpeedPatch
    {
        public static void Postfix(PlayerControl __instance)
        {
            // If the button is ON, make the host (you) super fast!
            if (ExamplePlugin.MuhsinGodMode.Value && __instance.AmOwner)
            {
                __instance.MyPhysics.Speed = 5.0f; 
            }
        }
    }
}
