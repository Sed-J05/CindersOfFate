namespace Shatranj.Core
{
    /// <summary>Marker for boss hurtboxes: player hits on these use the longer boss hit-stop (Docs/03 §4).</summary>
    public interface IBossTarget
    {
        string BossId { get; }
    }
}
