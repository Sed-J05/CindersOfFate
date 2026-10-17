using Shatranj.Core;
using Shatranj.Player;
using Shatranj.Visuals;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Shatranj.Level
{
    /// <summary>
    /// Runs once when a level scene starts: if the save's last bench is in this scene, moves the player there
    /// (and lights it); otherwise the player stays at the 'P' spawn. Then snaps the camera and the room.
    /// </summary>
    public class LevelSceneSetup : MonoBehaviour
    {
        [SerializeField] private Transform defaultSpawn;

        // ---- Music hooks (Prompt 17; ids listed in Docs/AUDIO_HOOKS.md) ----
        public const string MusicPhase1 = "music_phase1", MusicPhase2 = "music_phase2";
        public const string MusicBoss = "music_boss", MusicFinalBoss = "music_final_boss";

        private void OnEnable()
        {
            GameEvents.OnBossStarted += HandleBossStarted;
            GameEvents.OnBossDefeated += HandleBossEnded;
            GameEvents.OnBossReset += HandleBossEnded;
        }

        private void OnDisable()
        {
            GameEvents.OnBossStarted -= HandleBossStarted;
            GameEvents.OnBossDefeated -= HandleBossEnded;
            GameEvents.OnBossReset -= HandleBossEnded;
        }

        private static string PhaseMusic => GameManager.HasInstance && GameManager.Instance.Phase >= GameConstants.Phase2 ? MusicPhase2 : MusicPhase1;

        private void HandleBossStarted(string bossId, string displayName, float[] stages) =>
            AudioManager.PlayMusic(bossId == "zulmat" ? MusicFinalBoss : MusicBoss);

        private void HandleBossEnded(string bossId) => AudioManager.PlayMusic(PhaseMusic);

        private void Start()
        {
            AudioManager.PlayMusic(PhaseMusic);
            PlayerController player = FindFirstObjectByType<PlayerController>();
            if (player == null) return;
            var health = player.GetComponent<PlayerHealth>();

            Vector2 spawn = defaultSpawn != null ? (Vector2)defaultSpawn.position : player.Position;
            bool fromBench = false;

            if (GameManager.HasInstance)
            {
                SaveData save = GameManager.Instance.Save;
                if (!string.IsNullOrEmpty(save.lastBenchId) && save.lastBenchScene == SceneManager.GetActiveScene().name)
                {
                    foreach (Bench bench in FindObjectsByType<Bench>(FindObjectsSortMode.None))
                    {
                        if (bench.BenchId != save.lastBenchId) continue;
                        spawn = bench.RespawnPoint;
                        bench.SetLit(true);
                        fromBench = true;
                        break;
                    }
                }
            }

            if (fromBench) player.Respawn(spawn);
            if (health != null) health.RespawnPosition = spawn;
            if (CameraController.Main != null) CameraController.Main.SnapToTarget();
            if (RoomManager.Instance != null) RoomManager.Instance.LocatePlayer(instant: true);

            // Docs/08 §3/§7: the phase card ("PHASE II — THE INVERTED THEATRE") shows once on arrival.
            if (GameManager.HasInstance && !string.IsNullOrEmpty(GameManager.Instance.PendingTitleCard))
            {
                string card = GameManager.Instance.PendingTitleCard;
                GameManager.Instance.PendingTitleCard = null;
                string[] parts = card.Split('|');
                UI.ScreenCard.Show(this, parts[0], parts.Length > 1 ? parts[1] : "", 0f, 0.6f, 2.2f);
            }
        }
    }
}
