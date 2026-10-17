namespace Shatranj.Core
{
    /// <summary>Top-level game state owned by <see cref="GameManager"/>.</summary>
    public enum GameState
    {
        Menu,
        Playing,
        Cutscene,
        Paused,
        Dead,
    }
}
