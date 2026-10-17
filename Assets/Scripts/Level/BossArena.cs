using System;
using System.Collections.Generic;
using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// One boss room (Docs/05 legend 'M' / 'Z', gates 'G', drop slots '1'–'9'). Engages when the player enters
    /// the room and the boss is not yet defeated: closes the gates and raises <see cref="OnEngaged"/> (BossBase,
    /// Prompt 8, spawns the boss). Defeat opens the gates and persists; player death releases and resets.
    /// </summary>
    public class BossArena : MonoBehaviour
    {
        [SerializeField] private string bossId = "";
        [SerializeField] private string roomId = "";
        [SerializeField] private Vector2 bossSpawnPoint;
        [SerializeField] private List<Vector2> dropSlots = new List<Vector2>();
        [SerializeField] private List<BossGate> gates = new List<BossGate>();
        [SerializeField] private GameObject bossPrefab;

        [SerializeField, Tooltip("How far past a gate (toward the arena centre) the player must be before the gates close.")]
        private float engageMargin = 0.8f;

        private bool engaged;
        private bool playerInRoom;
        private Player.PlayerController player;

        public string BossId => bossId;
        public string RoomId => roomId;
        public Vector2 BossSpawnPoint => bossSpawnPoint;
        public IReadOnlyList<Vector2> DropSlots => dropSlots;
        public IReadOnlyList<BossGate> Gates => gates;
        public GameObject BossPrefab { get => bossPrefab; set => bossPrefab = value; }
        public bool IsEngaged => engaged;
        public bool IsDefeated => GameManager.HasInstance && GameManager.Instance.Save.HasDefeated(bossId);

        public event Action<BossArena> OnEngaged;
        public event Action<BossArena> OnReleased;
        /// <summary>Player died mid-fight: the boss must reset to its initial state (Docs/01 §10).</summary>
        public event Action<BossArena> OnResetRequested;

        public void Configure(string boss, string room, Vector2 spawn, List<Vector2> slots, List<BossGate> arenaGates)
        {
            bossId = boss;
            roomId = room;
            bossSpawnPoint = spawn;
            dropSlots = slots ?? new List<Vector2>();
            gates = arenaGates ?? new List<BossGate>();
        }

        private void OnEnable()
        {
            GameEvents.OnRoomEntered += HandleRoomEntered;
            GameEvents.OnBossDefeated += HandleBossDefeated;
            GameEvents.OnPlayerDied += HandlePlayerDied;
        }

        private void OnDisable()
        {
            GameEvents.OnRoomEntered -= HandleRoomEntered;
            GameEvents.OnBossDefeated -= HandleBossDefeated;
            GameEvents.OnPlayerDied -= HandlePlayerDied;
        }

        private void HandleRoomEntered(string id)
        {
            playerInRoom = id == roomId;
        }

        private void Update()
        {
            if (!playerInRoom || engaged || IsDefeated) return;
            if (player == null) player = FindFirstObjectByType<Player.PlayerController>();
            if (player == null) return;
            if (PlayerIsInsideGates(player.Position)) Engage();
        }

        /// <summary>True once the player is on the arena side of every gate, past the margin (so a gate never closes on them).</summary>
        private bool PlayerIsInsideGates(Vector2 playerPos)
        {
            float centerX = bossSpawnPoint.x;
            for (int i = 0; i < gates.Count; i++)
            {
                if (gates[i] == null) continue;
                float gateX = gates[i].transform.position.x;
                float inward = Mathf.Sign(centerX - gateX);            // direction from the gate toward the arena
                if (Mathf.Approximately(inward, 0f)) continue;
                if ((playerPos.x - gateX) * inward < engageMargin) return false;
            }
            return true;
        }

        private Bosses.BossBase instance;

        public Bosses.BossBase Instance => instance;

        public void Engage()
        {
            engaged = true;
            for (int i = 0; i < gates.Count; i++) if (gates[i] != null) gates[i].Close();
            SpawnBoss();
            OnEngaged?.Invoke(this);
        }

        /// <summary>Spawns a fresh boss at the 'M'/'Z' marker (a fresh instance is also how the boss "resets").</summary>
        private void SpawnBoss()
        {
            if (bossPrefab == null) return;
            if (instance != null) Destroy(instance.gameObject);
            GameObject go = Instantiate(bossPrefab, bossSpawnPoint, Quaternion.identity, transform);
            instance = go.GetComponent<Bosses.BossBase>();
            if (instance != null)
            {
                instance.Bind(this);
                instance.Begin();
            }
        }

        private void DespawnBoss()
        {
            if (instance == null) return;
            Destroy(instance.gameObject);
            instance = null;
        }

        private void HandleBossDefeated(string id)
        {
            if (id != bossId) return;
            instance = null; // the boss destroys itself after its death animation
            if (GameManager.HasInstance)
            {
                GameManager.Instance.Save.MarkDefeated(bossId);
                GameManager.Instance.WriteSave();
            }
            Release();
        }

        private void HandlePlayerDied()
        {
            if (!engaged) return;
            engaged = false;
            DespawnBoss();   // Docs/01 §10: bosses reset — the next engage spawns a fresh one
            for (int i = 0; i < gates.Count; i++) if (gates[i] != null) gates[i].Open();
            OnResetRequested?.Invoke(this);
        }

        public void Release()
        {
            engaged = false;
            for (int i = 0; i < gates.Count; i++) if (gates[i] != null) gates[i].Open();
            OnReleased?.Invoke(this);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.5f, 0.2f, 0.9f);
            Gizmos.DrawWireCube(bossSpawnPoint + new Vector2(0f, 1.2f), new Vector3(1.6f, 2.4f, 0f));
            Gizmos.color = new Color(1f, 0.9f, 0.3f, 0.9f);
            for (int i = 0; i < dropSlots.Count; i++) Gizmos.DrawWireSphere(dropSlots[i], 0.3f);
#if UNITY_EDITOR
            UnityEditor.Handles.Label(bossSpawnPoint + new Vector2(-0.5f, 2.7f), "BOSS " + bossId);
#endif
        }
    }
}
