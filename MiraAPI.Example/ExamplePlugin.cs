using HarmonyLib;
using System;

namespace MiraAPI.Example
{
    [HarmonyPatch]
    public class ExamplePlugin : global::MiraAPI.MiraPlugin
    {
        public override string OptionsTitleText => "Wheezy SNS Mode";

        public override void Load()
        {
            var options = global::MiraAPI.Options.GameOptionsManager.Instance.currentVars;
            if (options != null)
            {
                options.SabotageCooldown = 20f; 
                options.ReportDistance = 0f;
                options.NumEmergencyMeetings = 0;
                options.NumShapeshifters = 3;
                options.ShapeshifterChance = 100;
            }
            HarmonyLib.Harmony.PatchAll();
        }
    }
}
