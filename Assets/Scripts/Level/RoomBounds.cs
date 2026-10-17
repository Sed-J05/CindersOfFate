using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// A room's world rectangle (Docs/05 §3 rule 2). The camera clamps to it; the trigger on the RoomBounds layer
    /// tells the <see cref="RoomManager"/> (or, without one, the camera directly) when the player enters.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomBounds : MonoBehaviour
    {
        [SerializeField] private string roomId = "ROOM";
        [SerializeField] private Vector2 size = new Vector2(20f, 11f);
        [SerializeField] private int phase = 1;
        [SerializeField] private string section = "";
        [SerializeField] private string theme = "cave";
        [SerializeField] private bool isBossArena;
        [SerializeField] private Vector2 entryPoint;
        [SerializeField] private int orderIndex;

        private BoxCollider2D box;

        public string RoomId => roomId;
        public Vector2 Size => size;
        public Vector2 Center => transform.position;
        public int Phase => phase;
        public string Section => section;
        public string Theme => theme;
        public bool IsBossArena => isBossArena;
        /// <summary>Where F3 / F6 teleports drop the player (a safe floor spot inside the room).</summary>
        public Vector2 EntryPoint => entryPoint;
        public int OrderIndex => orderIndex;

        /// <summary>World-space rectangle of this room.</summary>
        public Rect WorldRect
        {
            get
            {
                Vector2 c = transform.position;
                return new Rect(c.x - size.x * 0.5f, c.y - size.y * 0.5f, size.x, size.y);
            }
        }

        public bool Contains(Vector2 point) => WorldRect.Contains(point);

        /// <summary>Builder entry point (sandbox / simple rooms).</summary>
        public void Configure(string id, Vector2 worldSize)
        {
            roomId = id;
            size = worldSize;
            entryPoint = transform.position;
            SyncCollider();
        }

        /// <summary>Builder entry point (level pipeline).</summary>
        public void Configure(string id, Vector2 worldSize, int roomPhase, string roomSection, string roomTheme, bool bossArena, Vector2 entry, int order)
        {
            roomId = id;
            size = worldSize;
            phase = roomPhase;
            section = roomSection;
            theme = roomTheme;
            isBossArena = bossArena;
            entryPoint = entry;
            orderIndex = order;
            SyncCollider();
        }

        private void Awake()
        {
            gameObject.layer = GameLayers.RoomBounds;
            SyncCollider();
        }

        private void SyncCollider()
        {
            if (box == null) box = GetComponent<BoxCollider2D>();
            if (box == null) return;
            box.isTrigger = true;
            box.size = size;
            box.offset = Vector2.zero;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.layer != GameLayers.Player) return;
            if (RoomManager.Instance != null)
            {
                RoomManager.Instance.NotifyPlayerEntered(this);
                return;
            }
            if (Visuals.CameraController.Main != null) Visuals.CameraController.Main.SetRoom(this);
            GameEvents.RaiseRoomEntered(roomId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = isBossArena ? new Color(1f, 0.4f, 0.3f, 0.7f) : new Color(0.3f, 0.9f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(entryPoint, 0.3f);
        }
    }
}
