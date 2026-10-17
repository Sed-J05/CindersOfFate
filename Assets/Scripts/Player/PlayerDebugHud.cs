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

        private readonly StringBuilder sb = new StringBuilder(256);
        private float timer;
        private float fps;

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
            sb.Append("FPS      ").Append(fps.ToString("0")).Append("   timeScale ").Append(Time.timeScale.ToString("0.00")).Append('\n');
            sb.Append("A/D move  Space jump  W/S look  F1-F9 debug  F9 reload");
            text.text = sb.ToString();
        }
    }
}
