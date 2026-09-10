namespace YesChef.Core
{
    /// <summary>
    /// Lifecycle of a single play session. Systems react to state transitions
    /// through <see cref="GameManager.OnStateChanged"/> rather than polling.
    /// </summary>
    public enum GameState
    {
        WaitingToStart,
        Playing,
        Paused,
        GameOver
    }
}
