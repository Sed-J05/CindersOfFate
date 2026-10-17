namespace Shatranj.Level
{
    /// <summary>
    /// Objects whose logic only runs while their room (or an adjacent one) is active (Docs/02 §12).
    /// Enemies implement this; <see cref="RoomManager"/> drives it.
    /// </summary>
    public interface IRoomActivatable
    {
        string RoomId { get; }
        void SetRoomActive(bool active);
    }

    /// <summary>Spawned enemies receive their spawner so they can report their kill with a stable id.</summary>
    public interface ISpawnedEnemy
    {
        void OnSpawnedBy(EnemySpawner spawner);
    }
}
