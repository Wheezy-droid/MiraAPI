using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using AmongUs.GameOptions;
using MiraAPI.Utilities.Assets;
using UnityEngine;

namespace MiraAPI.Roles;

/// <summary>
/// Utilities to make handling roles in-game easier.
/// </summary>
public static class CustomRoleUtils
{
    /// <summary>
    /// Determines whether the specified role can spawn in general, accounting for gamemodes and everything else.
    /// </summary>
    /// <param name="role">The role you would like to check for.</param>
    /// <returns>True if the role is able to spawn, otherwise false.</returns>
    public static bool CanSpawnOnCurrentMode(RoleBehaviour role)
    {
        if (role is ICustomRole custom)
        {
            return custom.CanSpawnOnCurrentMode();
        }

        if (GameManager.Instance.IsHideAndSeek())
        {
            return role.Role is RoleTypes.Engineer || role.Role is RoleTypes.Impostor;
        }
        return true;
    }

    public static List<(ushort RoleType, int Chance)> GetPossibleRoles(
        List<RoleManager.RoleAssignmentData> assignmentData,
        Func<RoleManager.RoleAssignmentData, bool>? predicate = null)
    {
        var roles = new List<(ushort, int)>();

        assignmentData.Where(x => predicate == null || predicate(x)).ToList().ForEach(x =>
        {
            for (var i = 0; i < x.Count; i++)
            {
                roles.Add(((ushort)x.Role.Role, x.Chance));
            }
        });

        return roles;
    }

    public static RoleManager.RoleAssignmentData GetAssignData(RoleTypes roleType)
    {
        var currentGameOptions = GameOptionsManager.Instance.CurrentGameOptions;
        var roleOptions = currentGameOptions.RoleOptions;

        var role = GetRegisteredRole(roleType);
        var assignmentData = new RoleManager.RoleAssignmentData(role, roleOptions.GetNumPerGame(role!.Role),
            roleOptions.GetChancePerGame(role.Role));

        return assignmentData;
    }

    public static RoleBehaviour? GetRegisteredRole(RoleTypes roleType)
    {
        // we want to prioritize the custom roles because the role has the right RoleColour/TeamColor
        var role = CustomRoleManager.AllRoles.FirstOrDefault(x => x.Role == roleType);

        return role;
    }

    /// <summary>
    /// Gets all active in-game roles.
    /// </summary>
    /// <returns>A list of roles.</returns>
    // ReSharper disable once MemberCanBePrivate.Global
    public static IEnumerable<RoleBehaviour> GetActiveRoles() => PlayerControl.AllPlayerControls.ToArray().Select(x => x.Data.Role);

    /// <summary>
    /// Gets all active in-game roles in a certain team.
    /// </summary>
    /// <param name="team">The team you would like to check for.</param>
    /// <returns>A list of roles with the team.</returns>
    public static IEnumerable<RoleBehaviour> GetActiveRolesOfTeam(ModdedRoleTeams team) => GetActiveRoles().Where(x => x is ICustomRole customRole && customRole.Team == team);

    /// <summary>
    /// Gets all active in-game roles of a certain type.
    /// </summary>
    /// <typeparam name="T">The role Type you would like to check for. Must be a RoleBehaviour.</typeparam>
    /// <returns>A list of roles with that specific type.</returns>
    public static IEnumerable<T> GetActiveRolesOfType<T>() where T : RoleBehaviour => GetActiveRoles().OfType<T>();

    /// <summary>
    /// Creates a string builder for the Role Tab.
    /// </summary>
    /// <param name="role">The ICustomRole object.</param>
    /// <returns>A StringBuilder.</returns>
    public static StringBuilder CreateForRole(ICustomRole role)
    {
        var taskStringBuilder = new StringBuilder();
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleColor.ToTextColor()}Your role is <b>{role.RoleName}.</b></color>");
        taskStringBuilder.Append("<size=70%>");
        taskStringBuilder.AppendLine(CultureInfo.InvariantCulture, $"{role.RoleLongDescription}");
        return taskStringBuilder;
    }

    /// <summary>
    /// Returns an intro sound from a role.
    /// </summary>
    /// <param name="roleType">The role type.</param>
    /// <returns>The intro sound.</returns>
    public static LoadableAsset<AudioClip>? GetIntroSound(RoleTypes roleType)
    {
        var role = CustomRoleManager.AllRoles.FirstOrDefault(role => role.Role == roleType);
        if (role is ICustomRole customRole)
        {
            return customRole.Configuration.IntroSound;
        }

        return new PreloadedAsset<AudioClip>(role!.IntroSound);
    }

    /// <summary>
    /// Determines if a role is a custom role or not.
    /// </summary>
    /// <param name="role">The RoleBehaviour to check.</param>
    /// <returns>True if the role is a custom role, false otherwise.</returns>
    public static bool IsCustomRole(this RoleBehaviour role)
    {
        return CustomRoleManager.CustomRoles.ContainsKey((ushort)role.Role);
    }
}
