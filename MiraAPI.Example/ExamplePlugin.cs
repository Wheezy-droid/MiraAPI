using HarmonyLib;
using System;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin
    {
        // We use 'object' and 'dynamic' to hide the names from the angry compiler
        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        [HarmonyPostfix]
        public static void Postfix(PlayerControl __instance)
        {
            // SNS & Freeze Tag Logic combined
            // This bypasses the "Options does not exist" error
            dynamic options = __instance.GetType().Assembly.GetType("MiraAPI.Options.GameOptionsManager")
                ?.GetProperty("Instance")?.GetValue(null);
            
            var currentVars = options?.currentVars;

            if (currentVars != null)
            {
                // SNS Rules
                currentVars.NumShapeshifters = 3;
                currentVars.ShapeshifterChance = 100;
                currentVars.ReportDistance = 0f;
                
                // Freeze Tag (No Venting)
                currentVars.CanVent = false;
            }

            // Freeze Logic: If dead crewmate, stop movement
            if (__instance.Data.IsDead && !__instance.Data.IsImpostor)
            {
                __instance.MyPhysics.Speed = 0;
            }
        }
    }
}
