using System.Collections.Generic;
using System.Linq;
using AmongUs.GameOptions;
using HarmonyLib;
using MiraAPI.Roles;
using UnityEngine;

namespace MiraAPI.Patches.Roles;

[HarmonyPatch(typeof(HowToPlayScene), nameof(HowToPlayScene.OpenRolesSelectionMenu))]
internal static class HowToPlayPatch
{
    // yes i patched the entire method
    private static void Prefix(HowToPlayScene __instance)
    {
        if (RoleManager.Instance.AllRoles.ToArray().All(x => !x.IsCustomRole()))
        {
            return;
        }
        __instance.sceneIndex = 0;
        __instance.category = HowToPlayScene.HowToPlayCategory.RolesSelection;
        __instance.startPage.SetActive(false);
        if (__instance.roleButtonsParent.childCount == 0)
        {
            using (IEnumerator<RoleBehaviour> enumerator = RoleManager.Instance.AllRoles.ToArray().Where(x => !x.IsCustomRole()).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    RoleBehaviour role = enumerator.Current;
                    if (!role.IsSimpleRole && role.Role != RoleTypes.CrewmateGhost && role.Role != RoleTypes.ImpostorGhost)
                    {
                        HowToPlayRoleButton component = Object.Instantiate(__instance.roleButtonPrefab, __instance.roleButtonsParent).GetComponent<HowToPlayRoleButton>();
                        Sprite roleIcon = __instance.rolesScenes.ToArray().First(r => r.role == role.Role).roleIcon;
                        component.SetRoleInfo(role, roleIcon);
                        component.SetButtonAction((Il2CppSystem.Action)(() =>
                        {
                            __instance.OpenRolePage(role.Role);
                        }));
                        __instance.controllerSelectables.Add(component.GetComponent<PassiveButton>());
                    }
                }
            }
            foreach (UiElement uiElement in __instance.controllerSelectables)
            {
                uiElement.ReceiveMouseOut();
            }
            ControllerManager.Instance.NewScene(__instance.name, __instance.closeButton, __instance.defaultButtonSelected, __instance.controllerSelectables, false);
        }
        __instance.DisableAllScenes();
        __instance.roleSelectionScene.SetActive(true);
        ControllerManager.Instance.SetDefaultSelection(__instance.defaultButtonSelected, null);
    }
}
