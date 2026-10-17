using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// Marker placed by Build Levels for every enemy glyph (Docs/05 §8) with a stable id
    /// <c>&lt;roomId&gt;_&lt;glyph&gt;_&lt;index&gt;</c> used by the kill-save. Spawns its prefab when the room
    /// becomes active and the enemy is not recorded as killed. Prefabs are assigned by Build Prefabs (Prompt 7).
    /// </summary>
    [DisallowMultipleComponent]
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private string stableId = "";
        [SerializeField] private string roomId = "";
        [SerializeField] private string enemyId = "";
        [SerializeField] private string glyph = "";
        [SerializeField] private GameObject prefab;

        private GameObject instance;
        private bool roomActive;

        public string StableId => stableId;
        public string RoomId => roomId;
        public string EnemyId => enemyId;
        public string Glyph => glyph;
        public GameObject Prefab { get => prefab; set => prefab = value; }
        public GameObject Instance => instance;
        public bool RoomActive => roomActive;
        public bool IsKilled => GameManager.HasInstance && GameManager.Instance.Save.HasKilled(stableId);

        public void Configure(string id, string room, string enemy, char glyphChar, GameObject enemyPrefab)
        {
            stableId = id;
            roomId = room;
            enemyId = enemy;
            glyph = glyphChar.ToString();
            prefab = enemyPrefab;
        }

        /// <summary>Called by the RoomManager. Spawns on first activation; toggles AI afterwards.</summary>
        public void SetRoomActive(bool active)
        {
            roomActive = active;
            if (active && instance == null && prefab != null && !IsKilled) Spawn();
            if (instance == null) return;
            // D-124: an enemy that wandered (or lunged) into an active room stays active even if its home room isn't,
            // so it never freezes — colliders off — in front of the player.
            bool effective = active || (RoomManager.Instance != null && RoomManager.Instance.IsPointActive(instance.transform.position));
            ApplyActivation(effective);
        }

        private void Spawn()
        {
            instance = Instantiate(prefab, transform.position, Quaternion.identity, transform);
            instance.name = prefab.name + "_" + stableId;
            foreach (ISpawnedEnemy s in instance.GetComponents<ISpawnedEnemy>()) s.OnSpawnedBy(this);
        }

        private void ApplyActivation(bool active)
        {
            IRoomActivatable[] activatables = instance.GetComponents<IRoomActivatable>();
            if (activatables.Length == 0) { instance.SetActive(active); return; }
            for (int i = 0; i < activatables.Length; i++) activatables[i].SetRoomActive(active);
        }

        /// <summary>The spawned enemy died: persist it (Docs/01 §10 — regular enemies stay dead).</summary>
        public void MarkKilled()
        {
            if (GameManager.HasInstance) GameManager.Instance.Save.MarkKilled(stableId);
            GameEvents.RaiseEnemyKilled(stableId);
            instance = null;
        }

        /// <summary>Debug (F5): kill the live instance if any.</summary>
        public void ForceKill()
        {
            if (instance == null) return;
            var health = instance.GetComponent<Health>();
            if (health != null) health.Kill(gameObject);
            else { Destroy(instance); MarkKilled(); }
        }

        private void OnDrawGizmos()
        {
            bool p2 = glyph.Length > 0 && Glyphs.EnemyPhase(glyph[0]) == 2;
            Gizmos.color = p2 ? new Color(0.45f, 0.75f, 1f, 0.9f) : new Color(1f, 0.3f, 0.3f, 0.9f);
            Gizmos.DrawWireCube(transform.position + new Vector3(0f, 0.5f, 0f), new Vector3(0.6f, 1f, 0f));
#if UNITY_EDITOR
            UnityEditor.Handles.Label(transform.position + new Vector3(-0.2f, 1.3f, 0f), glyph + " " + stableId);
#endif
        }
    }
}
