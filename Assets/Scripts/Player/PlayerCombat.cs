using System.Collections.Generic;
using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Docs/03 §4 nail attacks: side (alternating A/B) and up (W held), timed hitboxes from PlayerConfig
    /// (active 0.08 s starting 0.06 s after the press), 0.40 s cooldown with a 0.15 s input buffer,
    /// self-recoil on grounded hits, hit-stop, slash FX, soul on connect. No down attack / pogo.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerCombat : MonoBehaviour
    {
        private PlayerController controller;
        private PlayerInput input;
        private PlayerConfig config;
        private PlayerFxPools fx;
        private PlayerSoul soul;

        private readonly List<Collider2D> hitThisSwing = new List<Collider2D>(8);
        private readonly Collider2D[] overlaps = new Collider2D[8];
        private ContactFilter2D targetFilter;

        private float cooldownTimer;
        private float swingTimer;
        private bool swinging;
        private bool attackUp;
        private bool alternate;

        public bool IsSwinging => swinging;
        public bool HitboxActive => swinging && swingTimer >= config.hitboxStartDelay && swingTimer < config.hitboxStartDelay + config.hitboxActiveTime;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            input = GetComponent<PlayerInput>();
            fx = GetComponent<PlayerFxPools>();
            soul = GetComponent<PlayerSoul>();
            config = controller.Config;

            targetFilter = new ContactFilter2D();
            targetFilter.SetLayerMask(GameLayers.PlayerAttackTargetsMask);
            targetFilter.useTriggers = true;
        }

        private void OnEnable()
        {
            controller.OnStateChanged += HandleStateChanged;
        }

        private void OnDisable()
        {
            controller.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(PlayerState previous, PlayerState next)
        {
            bool inAttack = next == PlayerState.AttackSide || next == PlayerState.AttackUp;
            if (swinging && !inAttack) swinging = false; // interrupted by dash / hurt / death
        }

        private bool CanStartAttack()
        {
            if (cooldownTimer > 0f) return false;
            switch (controller.State)
            {
                case PlayerState.Idle:
                case PlayerState.Run:
                case PlayerState.JumpRise:
                case PlayerState.Fall:
                case PlayerState.Land:
                    return true;
                default:
                    return false; // healing, dashing, hurt, cast, dead, cutscene, resting, already swinging
            }
        }

        private void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            if (cooldownTimer > 0f) cooldownTimer -= dt;

            if (!swinging && CanStartAttack() && input.AttackBuffered(config.attackBufferTime))
                StartAttack();

            if (swinging)
            {
                swingTimer += dt;
                if (HitboxActive) ScanHitbox();
            }
        }

        private void StartAttack()
        {
            input.ConsumeAttack();
            attackUp = input.UpHeld;             // S + J = side attack (no down attack), W + J = up attack
            if (!attackUp) alternate = !alternate;
            cooldownTimer = config.attackCooldown;
            swingTimer = 0f;
            swinging = true;
            hitThisSwing.Clear();

            controller.EnterTimedState(attackUp ? PlayerState.AttackUp : PlayerState.AttackSide, config.attackAnimTime);

            GetHitbox(out Vector2 center, out Vector2 size);
            if (fx != null) fx.PlaySlash(center, size, attackUp, controller.Facing, alternate, config.slashFxLifetime);
            AudioManager.Play(attackUp ? "player_attack_up" : "player_attack");
        }

        /// <summary>Current attack box in world space (mirrored by facing).</summary>
        public void GetHitbox(out Vector2 center, out Vector2 size)
        {
            Vector2 p = transform.position;
            if (attackUp)
            {
                center = p + config.upHitboxOffset;
                size = config.upHitboxSize;
            }
            else
            {
                center = p + new Vector2(config.sideHitboxOffset.x * controller.Facing, config.sideHitboxOffset.y);
                size = config.sideHitboxSize;
            }
        }

        private void ScanHitbox()
        {
            GetHitbox(out Vector2 center, out Vector2 size);
            int n = Physics2D.OverlapBox(center, size, 0f, targetFilter, overlaps);
            for (int i = 0; i < n; i++)
            {
                Collider2D col = overlaps[i];
                if (col == null || hitThisSwing.Contains(col)) continue;
                hitThisSwing.Add(col);

                IDamageable target = col.GetComponentInParent<IDamageable>();
                if (target == null) continue;

                Vector2 targetCenter = col.bounds.center;
                DamageInfo info = attackUp
                    ? new DamageInfo(config.attackDamage, Vector2.up, 1f, gameObject, DamageType.Melee)
                    : DamageInfo.Horizontal(config.attackDamage, transform.position.x, targetCenter.x, 1f, DamageType.Melee, gameObject);

                if (target.TakeDamage(info)) OnHitLanded(col, targetCenter, info);
            }
        }

        private void OnHitLanded(Collider2D col, Vector2 targetCenter, DamageInfo info)
        {
            bool boss = col.GetComponentInParent<IBossTarget>() != null;
            HitStop.Do(boss ? config.hitStopOnBossHit : config.hitStopOnHit);

            if (!attackUp && controller.IsGrounded)
                controller.ApplyRecoil(-controller.Facing * config.recoilSpeed, config.recoilTime);

            Vector2 sparkAt = col.bounds.ClosestPoint(transform.position + new Vector3(0f, config.sideHitboxOffset.y, 0f));
            if (fx != null) fx.PlayHitSpark(sparkAt, config.hitSparkLifetime);
            if (soul != null && info.GrantsSoul) soul.GainFromHit(targetCenter);
            AudioManager.Play(boss ? "player_hit_boss" : "player_hit_enemy");
        }

        private void OnDrawGizmos()
        {
            if (!DebugCommands.ShowHitboxGizmos || config == null || controller == null) return;

            Vector2 p = transform.position;
            Vector2 side = p + new Vector2(config.sideHitboxOffset.x * controller.Facing, config.sideHitboxOffset.y);
            Vector2 up = p + config.upHitboxOffset;

            Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.35f);
            Gizmos.DrawWireCube(side, config.sideHitboxSize);
            Gizmos.DrawWireCube(up, config.upHitboxSize);

            if (HitboxActive)
            {
                GetHitbox(out Vector2 c, out Vector2 s);
                Gizmos.color = new Color(1f, 0.9f, 0.2f, 0.5f);
                Gizmos.DrawCube(c, s);
            }
        }
    }
}
