using UnityEngine;
using UnityEngine.Tilemaps;

namespace Shatranj.Level
{
    /// <summary>
    /// Static lava (Docs/04 §F, Docs/05 legend 'L'): animates the lava tiles by swapping every placed frame for the next
    /// one (Tile_Castle_Lava_00..03, 4 frames @ 6 fps, Docs/06 §5.1) — no AnimatedTile package needed. Damage lives on
    /// the baked Hazard collider next to it (DamageDealer, 1 heart, DamageType.Lava: i-frames, no teleport, the hurt
    /// knockback pops the player up out of the pit). Scaled time, so pause and hit-stop freeze the lava too.
    /// </summary>
    [RequireComponent(typeof(Tilemap))]
    public class LavaSurface : MonoBehaviour
    {
        [SerializeField] private TileBase[] frames = new TileBase[0];
        [SerializeField, Min(0.1f)] private float framesPerSecond = 6f;

        private Tilemap map;
        private int frame;
        private float timer;

        public int FrameCount => frames != null ? frames.Length : 0;

        private void Awake() => map = GetComponent<Tilemap>();

        private void Update()
        {
            if (frames == null || frames.Length < 2) return;
            timer += Time.deltaTime;
            float step = 1f / framesPerSecond;
            if (timer < step) return;
            timer -= step;
            if (timer > step) timer = 0f;   // after a long hitch, don't spin through frames

            TileBase current = frames[frame];
            frame = (frame + 1) % frames.Length;
            TileBase next = frames[frame];
            if (current != null && next != null && current != next) map.SwapTile(current, next);
        }
    }
}
