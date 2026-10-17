using Shatranj.Core;
using UnityEngine;

namespace Shatranj.Level
{
    /// <summary>
    /// A room's world rectangle (Docs/05 §3 rule 2). The camera clamps to it; the trigger on the
    /// RoomBounds layer tells the camera (and, from Prompt 6, the RoomManager) when the player enters.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(BoxCollider2D))]
    public class RoomBounds : MonoBehaviour
    {
        [SerializeField] private string roomId = "ROOM";
        [SerializeField] private Vector2 size = new Vector2(20f, 11f);

        private BoxCollider2D box;

        public string RoomId => roomId;
        public Vector2 Size => size;
        public Vector2 Center => transform.position;

        /// <summary>World-space rectangle of this room.</summary>
        public Rect WorldRect
        {
            get
            {
                Vector2 c = transform.position;
                return new Rect(c.x - size.x * 0.5f, c.y - size.y * 0.5f, size.x, size.y);
            }
        }

        /// <summary>Builder entry point: sets id and size and syncs the trigger collider.</summary>
        public void Configure(string id, Vector2 worldSize)
        {
            roomId = id;
            size = worldSize;
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
            if (Visuals.CameraController.Main != null) Visuals.CameraController.Main.SetRoom(this);
            GameEvents.RaiseRoomEntered(roomId);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.3f, 0.9f, 1f, 0.6f);
            Gizmos.DrawWireCube(transform.position, new Vector3(size.x, size.y, 0f));
        }
    }
}
