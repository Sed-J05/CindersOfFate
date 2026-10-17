using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using UnityEngine.InputSystem;
#endif

namespace Shatranj.Enemies
{
    /// <summary>
    /// Sandbox only (Docs/02 §13): number keys spawn prefabs at the mouse cursor (1 = first prefab …), Shift + number
    /// spawns from the second page (bosses), 0 clears. Ground enemies drop onto the floor below the cursor.
    /// Stands down while the F10 HUD panel uses the digits.
    /// </summary>
    public class SandboxEnemySpawner : MonoBehaviour
    {
        [SerializeField] private GameObject[] prefabs = new GameObject[0];
        [SerializeField] private GameObject[] shiftPrefabs = new GameObject[0];
        [SerializeField] private float dropSearchDistance = 30f;

        private readonly List<GameObject> spawned = new List<GameObject>();
        private readonly RaycastHit2D[] hits = new RaycastHit2D[2];

        public IReadOnlyList<GameObject> Prefabs => prefabs;
        public IReadOnlyList<GameObject> ShiftPrefabs => shiftPrefabs;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
        private void Update()
        {
            Keyboard kb = Keyboard.current;
            if (kb == null || UI.HudDebugPanel.IsOpen) return;

            bool shift = kb.leftShiftKey.isPressed || kb.rightShiftKey.isPressed;
            GameObject[] page = shift ? shiftPrefabs : prefabs;
            int count = Mathf.Min(page.Length, 9);
            for (int i = 0; i < count; i++)
                if (kb[(Key)((int)Key.Digit1 + i)].wasPressedThisFrame) Spawn(page[i]);

            if (kb.digit0Key.wasPressedThisFrame) Clear();
        }

        private void Spawn(GameObject prefab)
        {
            if (prefab == null) return;

            Vector2 pos = CursorWorld();
            var enemy = prefab.GetComponent<EnemyBase>();
            bool flyer = enemy != null && enemy.Config != null && enemy.Config.locomotion == Data.EnemyLocomotion.Flyer;
            if (!flyer)
            {
                var filter = new ContactFilter2D();
                filter.SetLayerMask(GameLayers.WalkableMask);
                filter.useTriggers = false;
                if (Physics2D.Raycast(pos, Vector2.down, filter, hits, dropSearchDistance) > 0) pos = hits[0].point;
            }

            spawned.Add(Instantiate(prefab, pos, Quaternion.identity));
            Debug.Log($"[Sandbox] spawned {prefab.name} at {pos}");
        }

        private Vector2 CursorWorld()
        {
            Camera cam = Camera.main;
            if (Mouse.current != null && cam != null)
            {
                Vector2 screen = Mouse.current.position.ReadValue();
                Vector3 w = cam.ScreenToWorldPoint(new Vector3(screen.x, screen.y, -cam.transform.position.z));
                return w;
            }
            PlayerController p = PlayerController.Active;
            return p != null ? p.Position + new Vector2(3f * p.Facing, 2f) : Vector2.zero;
        }

        private void Clear()
        {
            for (int i = 0; i < spawned.Count; i++) if (spawned[i] != null) Destroy(spawned[i]);
            spawned.Clear();
            Debug.Log("[Sandbox] cleared spawned enemies");
        }
#endif
    }
}
