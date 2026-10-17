using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// A background cut-out layer that lives under the camera rig and scrolls at a fraction of the camera's
    /// horizontal movement (Docs/06 §5.2: far 0.1, mid 0.3). Tiled sprite, wrapped so it never runs out.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxLayer : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField, Range(0f, 1f)] private float factor = 0.1f;
        [SerializeField] private float depth = 12f;

        private SpriteRenderer sr;

        public float Factor { get => factor; set => factor = value; }
        public SpriteRenderer Renderer => sr != null ? sr : (sr = GetComponent<SpriteRenderer>());

        private void Awake()
        {
            sr = GetComponent<SpriteRenderer>();
            sr.drawMode = SpriteDrawMode.Tiled;
            if (targetCamera == null) targetCamera = Camera.main;
        }

        private void LateUpdate()
        {
            if (targetCamera == null || sr.sprite == null) return;
            float halfH = targetCamera.orthographicSize;
            float screenW = halfH * 2f * targetCamera.aspect;
            Vector2 spriteSize = sr.sprite.bounds.size;
            float tileW = Mathf.Max(0.01f, spriteSize.x);

            // Height fills the screen; width covers the screen plus a tile on each side for wrapping.
            float scaleY = (halfH * 2f) / Mathf.Max(0.01f, spriteSize.y);
            transform.localScale = new Vector3(scaleY, scaleY, 1f);
            float scaledTile = tileW * scaleY;
            sr.size = new Vector2((screenW + 2f * scaledTile) / scaleY, spriteSize.y);

            float x = -Mathf.Repeat(targetCamera.transform.position.x * factor, scaledTile);
            transform.localPosition = new Vector3(x + scaledTile * 0.5f, 0f, depth);
        }
    }
}
