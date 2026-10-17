using System;
using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// Tracks the current room, switches the camera bounds (0.4 s blend via CameraController), and keeps
    /// only the current + adjacent rooms' enemies active (Docs/02 §12). Also serves F3 (skip room),
    /// F5 (kill all in room), F6 (teleport to boss arena) and the nearest safe-ground point for hazards.
    /// </summary>
    [DisallowMultipleComponent]
    public class RoomManager : MonoBehaviour
    {
        public static RoomManager Instance { get; private set; }

        [SerializeField] private float adjacencyMargin = 0.6f;

        private readonly List<RoomBounds> rooms = new List<RoomBounds>();
        private readonly Dictionary<string, RoomBounds> roomsById = new Dictionary<string, RoomBounds>();
        private readonly Dictionary<string, List<EnemySpawner>> spawnersByRoom = new Dictionary<string, List<EnemySpawner>>();
        private readonly Dictionary<string, List<IRoomActivatable>> activatablesByRoom = new Dictionary<string, List<IRoomActivatable>>();
        private readonly List<SafeGroundPoint> safePoints = new List<SafeGroundPoint>();
        private readonly HashSet<string> activeRoomIds = new HashSet<string>();
        private PlayerController player;

        public RoomBounds Current { get; private set; }

        /// <summary>Lowest world y of any room; anything below it has fallen out of the level.</summary>
        public float WorldBottom { get; private set; } = float.NegativeInfinity;
        public IReadOnlyList<RoomBounds> Rooms => rooms;
        /// <summary>(room, instant) — instant = teleport/respawn rather than walking through a doorway.</summary>
        public event Action<RoomBounds, bool> OnRoomChanged;

        private void Awake()
        {
            Instance = this;
            rooms.AddRange(FindObjectsByType<RoomBounds>(FindObjectsSortMode.None));
            rooms.Sort((a, b) => a.OrderIndex.CompareTo(b.OrderIndex));
            foreach (RoomBounds r in rooms) roomsById[r.RoomId] = r;
            if (rooms.Count > 0)
            {
                float bottom = float.PositiveInfinity;
                foreach (RoomBounds r in rooms) bottom = Mathf.Min(bottom, r.WorldRect.yMin);
                WorldBottom = bottom;
            }

            foreach (EnemySpawner s in FindObjectsByType<EnemySpawner>(FindObjectsSortMode.None))
            {
                if (!spawnersByRoom.TryGetValue(s.RoomId, out List<EnemySpawner> list)) spawnersByRoom[s.RoomId] = list = new List<EnemySpawner>();
                list.Add(s);
            }
            safePoints.AddRange(FindObjectsByType<SafeGroundPoint>(FindObjectsSortMode.None));
        }

        private void OnEnable()
        {
            DebugCommands.OnSkipRoomRequested += SkipRoom;
            DebugCommands.OnTeleportToBossRequested += TeleportToBoss;
            DebugCommands.OnKillAllEnemiesRequested += KillAllInCurrentRoom;
            DebugCommands.OnKillBossRequested += KillBossInCurrentRoom;
        }

        private void OnDisable()
        {
            DebugCommands.OnSkipRoomRequested -= SkipRoom;
            DebugCommands.OnTeleportToBossRequested -= TeleportToBoss;
            DebugCommands.OnKillAllEnemiesRequested -= KillAllInCurrentRoom;
            DebugCommands.OnKillBossRequested -= KillBossInCurrentRoom;
        }

        private void OnDestroy()
        {
            if (Instance == this) Instance = null;
        }

        private void Start()
        {
            player = PlayerController.Active != null ? PlayerController.Active : FindFirstObjectByType<PlayerController>();
            LocatePlayer(instant: true);
        }

        /// <summary>Finds the room containing the player and enters it (used after teleports / scene setup).</summary>
        public void LocatePlayer(bool instant)
        {
            if (player == null) player = PlayerController.Active;   // static, no per-frame scene search (Prompt 17)
            if (player == null) return;
            RoomBounds room = RoomAt(player.Position);
            if (room != null) Enter(room, instant);
            else if (Current == null && rooms.Count > 0) Enter(rooms[0], true);
        }

        public RoomBounds RoomAt(Vector2 point)
        {
            for (int i = 0; i < rooms.Count; i++) if (rooms[i].Contains(point)) return rooms[i];
            return null;
        }

        public RoomBounds Find(string roomId) => roomsById.TryGetValue(roomId, out RoomBounds r) ? r : null;

        public bool IsRoomActive(string roomId) => activeRoomIds.Contains(roomId);

        /// <summary>Called by RoomBounds triggers.</summary>
        public void NotifyPlayerEntered(RoomBounds room) => Enter(room, instant: false);

        /// <summary>True when <paramref name="point"/> lies in a room that is currently active (current or adjacent).</summary>
        public bool IsPointActive(Vector2 point)
        {
            RoomBounds r = RoomAt(point);
            return r != null && activeRoomIds.Contains(r.RoomId);
        }

        private void Update()
        {
            // D-124: self-correcting room tracking. Triggers can miss a teleport (respawn, hazard return, debug keys),
            // so whenever the player is outside the current room, locate them. A jump to a non-neighbouring room
            // (teleport) snaps the camera; walking into a neighbour blends as usual.
            if (player == null) { player = PlayerController.Active; if (player == null) return; }
            Vector2 p = player.Position + new Vector2(0f, 0.2f);
            if (Current != null && Current.Contains(p)) return;
            RoomBounds room = RoomAt(p);
            if (room == null || room == Current) return;
            bool neighbour = Current != null && IsAdjacent(Current, room);
            Enter(room, instant: !neighbour);
            if (!neighbour && CameraController.Main != null) CameraController.Main.SnapToTarget();
        }

        private bool IsAdjacent(RoomBounds a, RoomBounds b)
        {
            Rect expanded = a.WorldRect;
            expanded.xMin -= adjacencyMargin; expanded.xMax += adjacencyMargin;
            expanded.yMin -= adjacencyMargin; expanded.yMax += adjacencyMargin;
            return b.WorldRect.Overlaps(expanded);
        }

        private void Enter(RoomBounds room, bool instant)
        {
            if (room == null || room == Current) return;
            Current = room;
            if (CameraController.Main != null) CameraController.Main.SetRoom(room, instant);

            activeRoomIds.Clear();
            activeRoomIds.Add(room.RoomId);
            for (int i = 0; i < rooms.Count; i++)
                if (rooms[i] != room && IsAdjacent(room, rooms[i])) activeRoomIds.Add(rooms[i].RoomId);

            foreach (KeyValuePair<string, List<EnemySpawner>> kv in spawnersByRoom)
            {
                bool active = activeRoomIds.Contains(kv.Key);
                for (int i = 0; i < kv.Value.Count; i++) kv.Value[i].SetRoomActive(active);
            }
            foreach (KeyValuePair<string, List<IRoomActivatable>> kv in activatablesByRoom)
            {
                bool active = activeRoomIds.Contains(kv.Key);
                for (int i = 0; i < kv.Value.Count; i++) kv.Value[i].SetRoomActive(active);
            }

            GameEvents.RaiseRoomEntered(room.RoomId);
            OnRoomChanged?.Invoke(room, instant);
        }

        // ------------------------------------------------------------------
        // Registration for runtime-spawned objects (enemies not managed by a spawner)
        // ------------------------------------------------------------------

        public void Register(IRoomActivatable target)
        {
            if (target == null || string.IsNullOrEmpty(target.RoomId)) return;
            if (!activatablesByRoom.TryGetValue(target.RoomId, out List<IRoomActivatable> list)) activatablesByRoom[target.RoomId] = list = new List<IRoomActivatable>();
            if (!list.Contains(target)) list.Add(target);
            target.SetRoomActive(activeRoomIds.Contains(target.RoomId));
        }

        public void Unregister(IRoomActivatable target)
        {
            if (target == null || string.IsNullOrEmpty(target.RoomId)) return;
            if (activatablesByRoom.TryGetValue(target.RoomId, out List<IRoomActivatable> list)) list.Remove(target);
        }

        // ------------------------------------------------------------------
        // Safe ground (Docs/05 legend 'S')
        // ------------------------------------------------------------------

        /// <summary>Nearest explicit 'S' point in the current room, if any.</summary>
        public bool TryNearestSafePoint(Vector2 from, out Vector2 point)
        {
            point = default;
            float best = float.MaxValue;
            string roomId = Current != null ? Current.RoomId : null;
            for (int i = 0; i < safePoints.Count; i++)
            {
                SafeGroundPoint s = safePoints[i];
                if (s == null) continue;
                if (roomId != null && s.RoomId != roomId) continue;
                float d = ((Vector2)s.transform.position - from).sqrMagnitude;
                if (d < best) { best = d; point = s.transform.position; }
            }
            return best < float.MaxValue;
        }

        // ------------------------------------------------------------------
        // Debug helpers
        // ------------------------------------------------------------------

        public void TeleportPlayerTo(Vector2 position)
        {
            if (player == null) player = PlayerController.Active;   // static, no per-frame scene search (Prompt 17)
            if (player == null) return;
            player.Respawn(position);
            RoomBounds room = RoomAt(position);
            if (room != null) Enter(room, instant: true);
            if (CameraController.Main != null) CameraController.Main.SnapToTarget();
        }

        private void SkipRoom()
        {
            if (rooms.Count == 0) return;
            int index = Current != null ? rooms.IndexOf(Current) : -1;
            RoomBounds next = rooms[(index + 1) % rooms.Count];
            TeleportPlayerTo(next.EntryPoint);
        }

        private void TeleportToBoss()
        {
            int start = Current != null ? rooms.IndexOf(Current) : -1;
            for (int k = 1; k <= rooms.Count; k++)
            {
                RoomBounds r = rooms[(start + k) % rooms.Count];
                if (r.IsBossArena) { TeleportPlayerTo(r.EntryPoint); return; }
            }
        }

        private void KillBossInCurrentRoom()
        {
            foreach (Bosses.BossBase boss in FindObjectsByType<Bosses.BossBase>(FindObjectsSortMode.None))
            {
                if (boss.IsDead || !boss.IsFighting) continue;
                if (Current != null && !Current.Contains(boss.transform.position)) continue;
                Health h = boss.GetComponent<Health>();
                if (h != null) h.Kill();
            }
        }

        private void KillAllInCurrentRoom()
        {
            if (Current == null || !spawnersByRoom.TryGetValue(Current.RoomId, out List<EnemySpawner> list)) return;
            for (int i = 0; i < list.Count; i++) list[i].ForceKill();
        }
    }
}
