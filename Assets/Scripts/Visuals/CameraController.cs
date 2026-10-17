using Shatranj.Core;
using Shatranj.Data;
using Shatranj.Level;
using UnityEngine;

namespace Shatranj.Visuals
{
    /// <summary>
    /// Custom follow camera (Docs/02 §7, no Cinemachine): smoothing, horizontal look-ahead, hold-to-look
    /// up/down, clamp to the current <see cref="RoomBounds"/> with a blend between rooms, shake via
    /// <see cref="ScreenShake"/>, zoom and lock for bosses/cutscenes. Child "Rig" is reserved for the parchment.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Camera))]
    public class CameraController : MonoBehaviour
    {
        public static CameraController Main { get; private set; }

        [SerializeField] private CameraConfig config;
        [SerializeField] private MonoBehaviour targetBehaviour;   // must implement ICameraTarget
        [SerializeField] private RoomBounds initialRoom;
        [SerializeField] private float zDepth = -10f;

        private Camera cam;
        private ICameraTarget target;
        private readonly ShakeAccumulator shake = new ShakeAccumulator();

        private Vector2 basePosition;
        private Vector2 followVelocity;
        private float lookAhead, lookAheadVelocity;
        private float verticalLook, verticalLookVelocity, verticalHoldTimer;

        private bool hasBounds;
        private Rect bounds;
        private Rect blendFrom;
        private float blendT = 1f;

        private bool locked;
        private Vector2 lockPosition;

        private float zoomFrom, zoomTo, zoomT = 1f, zoomDuration;

        public Camera Camera => cam;
        public CameraConfig Config => config;
        public RoomBounds CurrentRoom { get; private set; }
        public bool IsLocked => locked;
        public Vector2 BasePosition => basePosition;

        private void Awake()
        {
            Main = this;
            cam = GetComponent<Camera>();
            cam.orthographic = true;
            if (config == null) config = ScriptableObject.CreateInstance<CameraConfig>();
            cam.orthographicSize = config.orthographicSize;
            zoomFrom = zoomTo = config.orthographicSize;
            if (targetBehaviour is ICameraTarget t) target = t;
            basePosition = transform.position;
            if (transform.Find("Rig") == null)
            {
                var rig = new GameObject("Rig");
                rig.transform.SetParent(transform, false);
            }
        }

        private void OnEnable()
        {
            ScreenShake.OnShakeRequested += HandleShakeRequest;
        }

        private void OnDisable()
        {
            ScreenShake.OnShakeRequested -= HandleShakeRequest;
        }

        private void OnDestroy()
        {
            if (Main == this) Main = null;
        }

        private void Start()
        {
            if (target == null) FindTarget();
            if (initialRoom != null) SetRoom(initialRoom, instant: true);
            SnapToTarget();
        }

        /// <summary>Finds the first ICameraTarget in the scene (player). Called once at Start, never per frame.</summary>
        public void FindTarget()
        {
            MonoBehaviour[] all = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i] is ICameraTarget t) { SetTarget(t, all[i]); return; }
            }
        }

        public void SetTarget(ICameraTarget newTarget, MonoBehaviour behaviour = null)
        {
            target = newTarget;
            targetBehaviour = behaviour;
        }

        private void LateUpdate()
        {
            float dt = Time.deltaTime;
            float udt = Time.unscaledDeltaTime;

            Vector2 desired = locked ? lockPosition : ComputeDesired(dt);

            basePosition = Vector2.SmoothDamp(basePosition, desired, ref followVelocity, config.followSmoothTime, config.maxFollowSpeed, dt);
            basePosition = Clamp(basePosition, dt);

            if (zoomT < 1f)
            {
                zoomT = zoomDuration <= 0f ? 1f : Mathf.Min(1f, zoomT + udt / zoomDuration);
                cam.orthographicSize = Mathf.Lerp(zoomFrom, zoomTo, Easing.Evaluate(Ease.InOutQuad, zoomT));
            }

            Vector2 shakeOffset = shake.Evaluate(udt) * config.shakeMultiplier;
            transform.position = new Vector3(basePosition.x + shakeOffset.x, basePosition.y + shakeOffset.y, zDepth);
        }

        private Vector2 ComputeDesired(float dt)
        {
            if (target == null) return basePosition;

            // Horizontal look-ahead in the facing direction.
            float lookTarget = target.FacingDirection * config.lookAheadDistance;
            lookAhead = Mathf.SmoothDamp(lookAhead, lookTarget, ref lookAheadVelocity, config.lookAheadSmoothTime, Mathf.Infinity, dt);

            // Hold W/S for a moment to peek up/down.
            float look = target.VerticalLook;
            if (look != 0f && target.CameraTargetGrounded) verticalHoldTimer += dt; else verticalHoldTimer = 0f;
            float vertTarget = verticalHoldTimer >= config.verticalLookHoldTime ? Mathf.Sign(look) * config.verticalLookDistance : 0f;
            verticalLook = Mathf.SmoothDamp(verticalLook, vertTarget, ref verticalLookVelocity, config.verticalLookSmoothTime, Mathf.Infinity, dt);

            Vector2 p = target.Position;
            return new Vector2(p.x + lookAhead, p.y + config.verticalOffset + verticalLook);
        }

        private Vector2 Clamp(Vector2 p, float dt)
        {
            if (!hasBounds) return p;

            Rect r = bounds;
            if (blendT < 1f)
            {
                blendT = config.roomBlendTime <= 0f ? 1f : Mathf.Min(1f, blendT + dt / config.roomBlendTime);
                float k = Easing.Evaluate(Ease.InOutQuad, blendT);
                r = new Rect(
                    Mathf.Lerp(blendFrom.xMin, bounds.xMin, k),
                    Mathf.Lerp(blendFrom.yMin, bounds.yMin, k),
                    Mathf.Lerp(blendFrom.width, bounds.width, k),
                    Mathf.Lerp(blendFrom.height, bounds.height, k));
            }

            float halfH = cam.orthographicSize;
            float halfW = halfH * cam.aspect;

            p.x = r.width <= halfW * 2f ? r.center.x : Mathf.Clamp(p.x, r.xMin + halfW, r.xMax - halfW);
            p.y = r.height <= halfH * 2f ? r.center.y : Mathf.Clamp(p.y, r.yMin + halfH, r.yMax - halfH);
            return p;
        }

        // ------------------------------------------------------------------
        // Public API
        // ------------------------------------------------------------------

        /// <summary>Switches the clamp rectangle. Blends over roomBlendTime unless <paramref name="instant"/>.</summary>
        public void SetRoom(RoomBounds room, bool instant = false)
        {
            if (room == null) { ClearBounds(); return; }
            Rect next = room.WorldRect;
            if (hasBounds && !instant)
            {
                blendFrom = CurrentEffectiveRect();
                blendT = 0f;
            }
            else
            {
                blendT = 1f;
            }
            bounds = next;
            hasBounds = true;
            CurrentRoom = room;
        }

        private Rect CurrentEffectiveRect()
        {
            if (blendT >= 1f) return bounds;
            float k = Easing.Evaluate(Ease.InOutQuad, blendT);
            return new Rect(
                Mathf.Lerp(blendFrom.xMin, bounds.xMin, k),
                Mathf.Lerp(blendFrom.yMin, bounds.yMin, k),
                Mathf.Lerp(blendFrom.width, bounds.width, k),
                Mathf.Lerp(blendFrom.height, bounds.height, k));
        }

        public void ClearBounds()
        {
            hasBounds = false;
            CurrentRoom = null;
        }

        public void Shake(float amplitude, float duration) =>
            shake.Add(new ShakeRequest(amplitude, duration, config.shakeFrequency));

        private void HandleShakeRequest(ShakeRequest request) => shake.Add(request);

        /// <summary>Animates orthographic size (boss intros).</summary>
        public void SetZoom(float size, float time = -1f)
        {
            zoomFrom = cam.orthographicSize;
            zoomTo = size;
            zoomDuration = time < 0f ? config.defaultZoomTime : time;
            zoomT = zoomDuration <= 0f ? 1f : 0f;
            if (zoomT >= 1f) cam.orthographicSize = zoomTo;
        }

        public void ResetZoom(float time = -1f) => SetZoom(config.orthographicSize, time);

        /// <summary>Holds the camera at a fixed point (cutscenes, boss arenas). Still clamped and shakeable.</summary>
        public void Lock(Vector2 position)
        {
            locked = true;
            lockPosition = position;
        }

        public void Unlock() => locked = false;

        /// <summary>Jumps straight to the target with no smoothing (spawn, respawn, scene start).</summary>
        public void SnapToTarget()
        {
            if (target == null && !locked) return;
            lookAhead = target != null ? target.FacingDirection * config.lookAheadDistance : 0f;
            lookAheadVelocity = 0f;
            verticalLook = 0f;
            verticalLookVelocity = 0f;
            followVelocity = Vector2.zero;
            Vector2 desired = locked ? lockPosition : new Vector2(target.Position.x + lookAhead, target.Position.y + config.verticalOffset);
            blendT = 1f;
            basePosition = Clamp(desired, 0f);
            transform.position = new Vector3(basePosition.x, basePosition.y, zDepth);
        }
    }
}
