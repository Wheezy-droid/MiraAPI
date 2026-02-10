using System;
using System.Collections;
using System.Linq;
using AmongUs.Data;
using AmongUs.GameOptions;
using Assets.CoreScripts;
using BepInEx.Unity.IL2CPP.Utils;
using MiraAPI.Events;
using MiraAPI.Events.Vanilla.Gameplay;
using Reactor.Networking.Attributes;
using Reactor.Networking.Rpc;
using Reactor.Utilities;
using Reactor.Utilities.Extensions;
using UnityEngine;
using Object = UnityEngine.Object;

namespace MiraAPI.Networking;

/// <summary>
/// Custom murder RPCs to fix issues with default ones.
/// </summary>
public static class CustomMurderRpc
{
    /// <summary>
    /// Networked Custom Murder method.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    public static void RpcCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        source.RpcCustomMurder(
            target,
            MeetingCheck.Ignore,
            didSucceed,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Networked Custom Murder method, which checks for meetings as well.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="inMeeting">Whether the murder is intended to be triggered in a meeting.</param>
    /// <param name="didSucceed">Whether the murder was successful or not.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    [MethodRpc((uint)MiraRpc.CustomMurder, LocalHandling = RpcLocalHandling.Before)]
    public static void RpcCustomMurder(
        this PlayerControl source,
        PlayerControl target,
        MeetingCheck inMeeting,
        bool didSucceed = true,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        if (LobbyBehaviour.Instance)
        {
            return;
        }
        var murderResultFlags = didSucceed ? MurderResultFlags.Succeeded : MurderResultFlags.FailedError;

        var beforeMurderEvent = new BeforeMurderEvent(source, target, inMeeting);
        MiraEventManager.InvokeEvent(beforeMurderEvent);
        var isMeetingActive = MeetingHud.Instance != null || ExileController.Instance != null;
        if ((inMeeting is MeetingCheck.ForMeeting && !isMeetingActive) || (inMeeting is MeetingCheck.OutsideMeeting && isMeetingActive))
        {
            beforeMurderEvent.Cancel();
        }

        if (beforeMurderEvent.IsCancelled)
        {
            murderResultFlags = MurderResultFlags.FailedError;
        }

        var murderResultFlags2 = MurderResultFlags.DecisionByHost | murderResultFlags;

        source.CustomMurder(
            target,
            murderResultFlags2,
            resetKillTimer,
            createDeadBody,
            teleportMurderer,
            showKillAnim,
            playKillSound);
    }

    /// <summary>
    /// Custom Murder method without networking. If you need a networked version, use <see cref="RpcCustomMurder"/>.
    /// </summary>
    /// <param name="source">The killer.</param>
    /// <param name="target">The player to murder.</param>
    /// <param name="resultFlags">Murder result flags.</param>
    /// <param name="resetKillTimer">Should the kill timer be reset.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the killer be snapped to the dead player.</param>
    /// <param name="showKillAnim">Should the kill animation be shown.</param>
    /// <param name="playKillSound">Should the kill sound be played.</param>
    public static void CustomMurder(
        this PlayerControl source,
        PlayerControl target,
        MurderResultFlags resultFlags,
        bool resetKillTimer = true,
        bool createDeadBody = true,
        bool teleportMurderer = true,
        bool showKillAnim = true,
        bool playKillSound = true)
    {
        source.isKilling = false;
        if (resultFlags.HasFlag(MurderResultFlags.FailedError) || LobbyBehaviour.Instance)
        {
            return;
        }

        if (resultFlags.HasFlag(MurderResultFlags.FailedProtected) ||
            (resultFlags.HasFlag(MurderResultFlags.DecisionByHost) && target.protectedByGuardianId > -1))
        {
            target.protectedByGuardianThisRound = true;
            var flag = PlayerControl.LocalPlayer.Data.Role.Role == RoleTypes.GuardianAngel;
            if (flag && PlayerControl.LocalPlayer.Data.PlayerId == target.protectedByGuardianId)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_GuardianAngel_CrewmatesProtected);
                AchievementManager.Instance.OnProtectACrewmate();
            }

            if (source.AmOwner || flag)
            {
                target.ShowFailedMurder();

                if (resetKillTimer)
                {
                    source.SetKillTimer(
                        GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown) / 2f);
                }
            }
            else
            {
                target.RemoveProtection();
            }

            return;
        }

        if (!resultFlags.HasFlag(MurderResultFlags.Succeeded) &&
            !resultFlags.HasFlag(MurderResultFlags.DecisionByHost))
        {
            return;
        }

        DebugAnalytics.Instance.Analytics.Kill(target.Data, source.Data);
        if (source.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(
                GameManager.Instance.IsHideAndSeek()
                    ? StatID.HideAndSeek_ImpostorKills
                    : StatID.ImpostorKills);

            if (source.CurrentOutfitType == PlayerOutfitType.Shapeshifted)
            {
                DataManager.Player.Stats.IncrementStat(StatID.Role_Shapeshifter_ShiftedKills);
            }

            if (Constants.ShouldPlaySfx() && playKillSound)
            {
                SoundManager.Instance.PlaySound(source.KillSfx, false, 0.8f);
            }

            if (resetKillTimer)
            {
                source.SetKillTimer(GameOptionsManager.Instance.CurrentGameOptions.GetFloat(FloatOptionNames.KillCooldown));
            }
        }

        UnityTelemetry.Instance.WriteMurder();
        target.gameObject.layer = LayerMask.NameToLayer("Ghost");
        if (target.AmOwner)
        {
            DataManager.Player.Stats.IncrementStat(StatID.TimesMurdered);
            if (Minigame.Instance)
            {
                try
                {
                    Minigame.Instance.Close();
                    Minigame.Instance.Close();
                }
                catch
                {
                    // ignored
                }
            }

            if (showKillAnim)
            {
                try
                {
                    HudManager.Instance.KillOverlay.ShowKillAnimation(source.Data, target.Data);
                }
                catch (Exception e)
                {
                    Error($"Error with kill animation: {e.ToString()}");
                }
            }

            target.cosmetics.SetNameMask(false);
            target.RpcSetScanner(false);
        }

        AchievementManager.Instance.OnMurder(
            source.AmOwner,
            target.AmOwner,
            source.CurrentOutfitType == PlayerOutfitType.Shapeshifted,
            source.shapeshiftTargetPlayerId,
            target.PlayerId);
        source.MyPhysics.StartCoroutine(source.KillAnimations.Random()?.CoPerformCustomKill(source, target, createDeadBody, teleportMurderer));
    }

    /// <summary>
    /// Perform a custom kill animation.
    /// </summary>
    /// <param name="anim">The kill animation.</param>
    /// <param name="source">The murderer.</param>
    /// <param name="target">The murdered player.</param>
    /// <param name="createDeadBody">Should a dead body be created.</param>
    /// <param name="teleportMurderer">Should the murder be teleported.</param>
    /// <returns>Coroutine.</returns>
    public static IEnumerator CoPerformCustomKill(
        this KillAnimation anim,
        PlayerControl source,
        PlayerControl target,
        bool createDeadBody = true,
        bool teleportMurderer = true)
    {
        if (LobbyBehaviour.Instance)
        {
            yield break;
        }
        var cam = Camera.main?.GetComponent<FollowerCamera>();
        var isParticipant = source.AmOwner || target.AmOwner;
        var sourcePhys = source.MyPhysics;

        if (teleportMurderer)
        {
            KillAnimation.SetMovement(source, false);
        }

        KillAnimation.SetMovement(target, false);

        if (isParticipant)
        {
            PlayerControl.LocalPlayer.isKilling = true;
            source.isKilling = true;
        }

        DeadBody? deadBody = null;

        if (createDeadBody)
        {
            deadBody = Object.Instantiate(GameManager.Instance.GetDeadBody(source.Data.Role));
            deadBody.enabled = false;
            deadBody.ParentId = target.PlayerId;
            deadBody.bodyRenderers.ToList().ForEach(target.SetPlayerMaterialColors);

            target.SetPlayerMaterialColors(deadBody.bloodSplatter);
            var vector = target.transform.position + anim.BodyOffset;
            vector.z = vector.y / 1000f;
            deadBody.transform.position = vector;
        }

        source.Data.Role.KillAnimSpecialSetup(deadBody, source, target);
        target.Data.Role.KillAnimSpecialSetup(deadBody, source, target);

        // no idea if this causes bugs, but innersloth is brain-dead
        // I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF I HATE INNERSCUFF
        PlayerControl.LocalPlayer.Data.Role.KillAnimSpecialSetup(deadBody, source, target);

        if (isParticipant)
        {
            if (cam != null)
            {
                cam.Locked = true;
            }

            ConsoleJoystick.SetMode_Task();
            if (PlayerControl.LocalPlayer.AmOwner)
            {
                PlayerControl.LocalPlayer.MyPhysics.inputHandler.enabled = true;
            }
        }

        target.Die(DeathReason.Kill, true);

        if (teleportMurderer)
        {
            yield return source.MyPhysics.Animations.CoPlayCustomAnimation(anim.BlurAnim);
            sourcePhys.Animations.PlayIdleAnimation();
            source.NetTransform.SnapTo(target.transform.position);
            KillAnimation.SetMovement(source, true);
        }

        KillAnimation.SetMovement(target, true);

        if (deadBody != null)
        {
            deadBody.enabled = true;
        }

        var afterMurderEvent = new AfterMurderEvent(source, target, deadBody);
        MiraEventManager.InvokeEvent(afterMurderEvent);

        if (!isParticipant)
        {
            yield break;
        }

        if (cam != null)
        {
            cam.Locked = false;
        }

        PlayerControl.LocalPlayer.isKilling = false;
        source.isKilling = false;
    }
}

/// <summary>
/// Checks for custom murders.
/// </summary>
public enum MeetingCheck
{
    /// <summary>
    /// Checks for meetings.
    /// </summary>
    ForMeeting,

    /// <summary>
    /// Checks for a meeting to not be active.
    /// </summary>
    OutsideMeeting,

    /// <summary>
    /// Doesn't check for a meeting at all.
    /// </summary>
    Ignore,
}
