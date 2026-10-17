using Shatranj.Core;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Player
{
    /// <summary>
    /// Docs/03 §5 "shadow power": max 99, +11 per connecting nail hit (added when the orb arrives),
    /// spent by heal and fireball (33 each). Reset to 0 on death. F2 fills it.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(PlayerController))]
    public class PlayerSoul : MonoBehaviour
    {
        private PlayerController controller;
        private PlayerFxPools fx;
        private PlayerConfig config;
        private int soul;

        private static readonly Vector2 ChestOffset = new Vector2(0f, 1.0f);

        public int Soul => soul;
        public int Max => config.soulMax;
        public float Fraction => config.soulMax > 0 ? (float)soul / config.soulMax : 0f;

        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            fx = GetComponent<PlayerFxPools>();
            config = controller.Config;
        }

        private void OnEnable()
        {
            DebugCommands.OnFullSoulRequested += FillFull;
        }

        private void OnDisable()
        {
            DebugCommands.OnFullSoulRequested -= FillFull;
        }

        private void Start()
        {
            GameEvents.RaiseSoulChanged(soul, config.soulMax);
        }

        /// <summary>Spawns the flying orb from the hit point; soul is added on arrival.</summary>
        public void GainFromHit(Vector2 from)
        {
            if (fx != null) fx.SpawnSoulOrb(from, transform, ChestOffset, config.soulOrbTravelTime, AddHitSoul);
            else AddHitSoul();
        }

        private void AddHitSoul()
        {
            if (controller.State == PlayerState.Dead) return;
            Add(config.soulPerHit);
            AudioManager.Play("soul_gain");
        }

        public void Add(int amount)
        {
            int before = soul;
            soul = Mathf.Clamp(soul + amount, 0, config.soulMax);
            if (soul != before) GameEvents.RaiseSoulChanged(soul, config.soulMax);
        }

        public bool CanSpend(int cost) => soul >= cost;

        public bool Spend(int cost)
        {
            if (soul < cost) return false;
            soul -= cost;
            GameEvents.RaiseSoulChanged(soul, config.soulMax);
            return true;
        }

        public void ResetSoul()
        {
            soul = 0;
            GameEvents.RaiseSoulChanged(soul, config.soulMax);
        }

        private void FillFull()
        {
            soul = config.soulMax;
            GameEvents.RaiseSoulChanged(soul, config.soulMax);
        }
    }
}
