using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace Shatranj.Player
{
    /// <summary>Sandbox overlay: state, velocity, grounded, coyote/buffer timers, wall contacts. Legacy UI Text so it needs no TMP import.</summary>
    public class PlayerDebugHud : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Text text;
        [SerializeField] private float refreshInterval = 0.05f;

        private readonly StringBuilder sb = new StringBuilder(384);
        private float timer;
        private float fps;
        private PlayerHealth playerHealth;
        private PlayerSoul playerSoul;
        private PlayerDash playerDash;
        private PlayerSpells playerSpells;

        private void Start()
        {
            if (player == null) return;
            playerHealth = player.GetComponent<PlayerHealth>();
            playerSoul = player.GetComponent<PlayerSoul>();
            playerDash = player.GetComponent<PlayerDash>();
            playerSpells = player.GetComponent<PlayerSpells>();
        }

        private void Update()
        {
            if (Time.unscaledDeltaTime > 0f)
                fps = Mathf.Lerp(fps, 1f / Time.unscaledDeltaTime, 0.1f);

            timer += Time.unscaledDeltaTime;
            if (timer < refreshInterval) return;
            timer = 0f;

            if (text == null) return;
            if (player == null)
            {
                text.text = "no player";
                return;
            }

            Vector2 v = player.Velocity;
            PlayerMotor m = player.Motor;
            sb.Clear();
            sb.Append("State    ").Append(player.State).Append("  (t ").Append(player.StateTime.ToString("0.00")).Append(")\n");
            sb.Append("Velocity ").Append(v.x.ToString("0.00")).Append(", ").Append(v.y.ToString("0.00")).Append('\n');
            sb.Append("Grounded ").Append(m.IsGrounded ? "yes" : "no").Append(m.OnOneWay ? " (one-way)" : "").Append('\n');
            sb.Append("Coyote   ").Append(Mathf.Max(0f, player.CoyoteRemaining).ToString("0.00"));
            sb.Append("  JumpBuf ").Append(player.Input.JumpBufferAge < 10f ? player.Input.JumpBufferAge.ToString("0.00") : "-").Append('\n');
            sb.Append("Wall L/R ").Append(m.TouchingWallLeft ? "L" : "-").Append(m.TouchingWallRight ? "R" : "-");
            sb.Append("  Ceiling ").Append(m.HitCeiling ? "yes" : "no").Append('\n');
            sb.Append("Facing   ").Append(player.Facing > 0 ? "right" : "left");
            sb.Append("  Input ").Append(player.Input.MoveX.ToString("0")).Append(',').Append(player.Input.MoveY.ToString("0")).Append('\n');
            if (playerHealth != null)
                sb.Append("Hearts   ").Append(playerHealth.Hearts).Append('/').Append(playerHealth.MaxHearts)
                  .Append(playerHealth.IsInvulnerable ? "  (i-frames)" : "").Append('\n');
            if (playerSoul != null)
                sb.Append("Soul     ").Append(playerSoul.Soul).Append('/').Append(playerSoul.Max)
                  .Append(playerSpells != null && playerSpells.IsHealing ? "  healing " + (playerSpells.HealProgress * 100f).ToString("0") + "%" : "").Append('\n');
            if (playerDash != null)
                sb.Append("Dash     ").Append(playerDash.Unlocked ? "unlocked" : "LOCKED (F4)")
                  .Append("  cd ").Append(playerDash.CooldownRemaining.ToString("0.00"))
                  .Append("  air ").Append(playerDash.AirDashesLeft).Append('\n');
            sb.Append("FPS      ").Append(fps.ToString("0")).Append("   timeScale ").Append(Time.timeScale.ToString("0.00")).Append('\n');
            sb.Append("A/D move  Space jump  J attack (W+J up)  K tap fireball / hold heal  L dash\n");
            sb.Append("F1 god  F2 soul  F4 phase toggle  F7 hitbox gizmos  F9 reload\n");
            sb.Append("1 Crawler 2 Lunger 3 Kite 4 Warden 5 Knight 6 Bat 7 Charger 8 Thrower  Shift+1 Warden Shift+2 Caller Shift+3 Colossus  0 clear\n");
            sb.Append("F10 HUD debug  F11 visuals  F12 flame scale");
            text.text = sb.ToString();
        }
    }
}
