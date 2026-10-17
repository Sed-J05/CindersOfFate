using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Player;
using UnityEngine;

namespace Shatranj.Enemies
{
    /// <summary>
    /// Docs/04 §C2 Bat (3 HP, flyer). Hangs asleep from the ceiling until the player comes within 8 u (or hits it) →
    /// Wake 0.4 s → erratic Perlin wander at 3.5 u/s circling the player at 3–4 u → every 2.5 s a dive-bite
    /// (Telegraph 0.35 s, 8 u/s toward where the player was) → flutters away and resumes circling. Bats in the same
    /// room desync their dive timers by +0.6 s each so they never dive together. Scales with the flame (pivot centre).
    /// </summary>
    public class Bat : EnemyBase
    {
        private enum State { Sleep, Wake, Circle, Telegraph, Dive, Recover }

        private const float OrbitSpeed = 1.3f;          // rad/s
        private const float NoiseFrequency = 0.6f;
        private const float RecoverLift = 1f;

        private static readonly Dictionary<string, int> wakeCountByRoom = new Dictionary<string, int>();

        private State state = State.Sleep;
        private float timer;
        private float attackTimer;
        private float desync;
        private Vector2 anchor;
        private Vector2 lastMoveTarget;
        private Vector2 diveTarget, diveDir, recoverDir;
        private float travelled;
        private float angle;
        private float noiseSeed;
        private readonly RaycastHit2D[] hits = new RaycastHit2D[2];

        protected override bool IsAttacking => state == State.Telegraph || state == State.Dive;

        private float Radius => Mathf.Max(0.05f, config.colliderRadius * FlameScale * 0.9f);

        protected override void Start()
        {
            base.Start();
            noiseSeed = Random.value * 100f;
            anchor = rb.position;
            lastMoveTarget = anchor;
            if (config.startsAsleep)
            {
                HangFromCeiling();
                state = State.Sleep;
                SetAnim("Sleep");
            }
            else
            {
                state = State.Circle;
                attackTimer = config.attackInterval;
                SetAnim("Fly");
            }
        }

        private void HangFromCeiling()
        {
            var filter = new ContactFilter2D();
            filter.SetLayerMask(GameLayers.WallMask);
            filter.useTriggers = false;
            if (Physics2D.Raycast(anchor, Vector2.up, filter, hits, 3f) > 0)
                anchor = hits[0].point - new Vector2(0f, config.bodySize.y * 0.5f * FlameScale + 0.02f);
            rb.position = anchor;
            transform.position = anchor;
            lastMoveTarget = anchor;
        }

        protected override void TickAI(float dt)
        {
            // Absorb position changes made by others since our last move (FlameScaler's wall nudge).
            anchor += rb.position - lastMoveTarget;

            bool hasPlayer = TryGetPlayer(out PlayerController p);
            Vector2 chest = hasPlayer ? PlayerChest(p) : anchor;

            switch (state)
            {
                case State.Sleep:
                    if (hasPlayer && (chest - anchor).sqrMagnitude <= config.detectRange * config.detectRange) EnterWake();
                    break;

                case State.Wake:
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        state = State.Circle;
                        attackTimer = config.attackInterval + desync;
                        SetAnim("Fly");
                    }
                    break;

                case State.Circle:
                {
                    Vector2 target;
                    if (hasPlayer)
                    {
                        angle += OrbitSpeed * dt;
                        float n = Mathf.PerlinNoise(noiseSeed, Time.time * NoiseFrequency);
                        float r = Mathf.Lerp(config.circleRadiusMin, config.circleRadiusMax, n) * FlameScale;
                        // Erratic orbit above the player (upper half of the circle, Perlin jitter on radius and height).
                        target = chest + new Vector2(Mathf.Cos(angle) * r, Mathf.Abs(Mathf.Sin(angle)) * r * 0.6f + 1f
                                 + (Mathf.PerlinNoise(noiseSeed + 7f, Time.time * NoiseFrequency * 1.7f) - 0.5f));
                        SetFacing(p.Position.x >= anchor.x ? 1 : -1);
                        attackTimer -= dt;
                        if (attackTimer <= 0f) { EnterTelegraph(chest); break; }
                    }
                    else target = anchor;
                    Vector2 step = Vector2.MoveTowards(anchor, target, config.wanderSpeed * dt) - anchor;
                    step += EnemySensors.Separation(anchor, config.separationRadius, transform) * config.wanderSpeed * dt;
                    SweepMove(ref anchor, step, Radius, 0f);
                    break;
                }

                case State.Telegraph:
                    timer -= dt;
                    if (timer <= 0f) EnterDive();
                    break;

                case State.Dive:
                {
                    Vector2 step = diveDir * (config.attackSpeed * dt);
                    float moved = SweepMove(ref anchor, step, Radius, 0f, slide: false);   // a dive ends on impact
                    travelled += moved;
                    if (moved < step.magnitude - 1e-4f || travelled >= config.attackMaxDistance) EnterRecover();
                    break;
                }

                case State.Recover:
                    SweepMove(ref anchor, recoverDir * (config.returnSpeed * dt), Radius, 0f);
                    timer -= dt;
                    if (timer <= 0f)
                    {
                        state = State.Circle;
                        attackTimer = config.attackInterval;
                        SetAnim("Fly");
                    }
                    break;
            }

            rb.MovePosition(anchor);
            lastMoveTarget = anchor;
        }

        protected override void ApplyKnockback(Vector2 velocity, float dt) => SweepMove(ref anchor, velocity * dt, Radius, 0f);

        protected override void OnHurt(DamageInfo info)
        {
            if (state == State.Sleep) EnterWake();
        }

        private void EnterWake()
        {
            state = State.Wake;
            timer = config.wakeTime;
            SetAnim("Wake", true);
            AudioManager.Play("bat_wake");

            // Desync: each bat that wakes in this room gets +0.6 s more on its first dive timer.
            string room = RoomId ?? "";
            wakeCountByRoom.TryGetValue(room, out int index);
            wakeCountByRoom[room] = index + 1;
            desync = (index % 4) * config.desyncOffset;
        }

        private void EnterTelegraph(Vector2 target)
        {
            state = State.Telegraph;
            timer = config.telegraphTime;
            diveTarget = target;
            SetAnim("Telegraph", true);
            BeginTelegraph();
            AudioManager.Play("bat_telegraph");
        }

        private void EnterDive()
        {
            EndTelegraph();
            state = State.Dive;
            travelled = 0f;
            Vector2 d = diveTarget - anchor;
            diveDir = d.sqrMagnitude > 1e-4f ? d.normalized : Vector2.down;
            SetFacing(diveDir.x >= 0f ? 1 : -1);
            SetAnim("Dive", true);
        }

        private void EnterRecover()
        {
            state = State.Recover;
            timer = config.attackRecoveryTime;
            recoverDir = new Vector2(-diveDir.x * 0.6f, RecoverLift).normalized;
            SetAnim("Fly");
        }

        protected override void OnDeath() => EndTelegraph();

        private void OnDestroy()
        {
            // Keep the desync counter small: forget this bat's slot when its room has none left (cheap, approximate).
            string room = RoomId ?? "";
            if (wakeCountByRoom.TryGetValue(room, out int n) && n > 0 && state != State.Sleep) wakeCountByRoom[room] = n - 1;
        }
    }
}
