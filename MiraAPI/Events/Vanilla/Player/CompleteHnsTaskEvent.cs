namespace MiraAPI.Events.Vanilla.Player;

/// <summary>
/// The event that is invoked when a player completes a task within Hide and Seek. Non-cancelable.
/// </summary>
public class CompleteHnsTaskEvent : MiraEvent
{
    /// <summary>
    /// Gets the instance of the <see cref="PlayerControl"/>.
    /// </summary>
    public PlayerControl Player { get; }

    /// <summary>
    /// Gets the instance of the <see cref="PlayerTask"/> that the player completed.
    /// </summary>
    public PlayerTask Task { get; }

    /// <summary>
    /// Gets the instance of the <see cref="HnsManager"/> attached to the game.
    /// </summary>
    public HideAndSeekManager HnsManager { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CompleteHnsTaskEvent"/> class.
    /// </summary>
    /// <param name="hnsManager">The Hide and Seek Manager currently active.</param>
    /// <param name="player">The player who completed the task.</param>
    /// <param name="task">The task that the player completed.</param>
    public CompleteHnsTaskEvent(HideAndSeekManager hnsManager, PlayerControl player, PlayerTask task)
    {
        HnsManager = hnsManager;
        Player = player;
        Task = task;
    }
}
