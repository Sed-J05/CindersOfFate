using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Shatranj.Core
{
    /// <summary>
    /// Everything persisted between sessions (Docs/02 §4): phase, cycle, last bench, flame progress at that bench,
    /// killed enemy ids, defeated boss ids, hearts max, abilities, door flags. Single auto-save slot.
    /// </summary>
    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public int phase = GameConstants.Phase1;
        public int cycle = 1;
        public string lastBenchId = "";
        public string lastBenchScene = "";
        public float lastBenchX;
        public float lastBenchY;
        /// <summary>Flame arc progress saved at the bench; restored on death (Docs/01 §12.8).</summary>
        public float flameT;
        public int maxHearts = GameConstants.HeartsPhase1;
        public bool canDash;
        public bool canFireball;
        public List<string> killedEnemyIds = new List<string>();
        public List<string> defeatedBossIds = new List<string>();
        /// <summary>Generic flags such as "door_orb".</summary>
        public List<string> flags = new List<string>();
        /// <summary>
        /// Story cutscene that started from gameplay but hasn't finished (orb → cs2, final boss → cs3, cs3 → cs1).
        /// Continue resumes it instead of dropping the player into a finished level (Prompt 16).
        /// </summary>
        public string pendingCutscene = "";
        public string pendingNextScene = "";
        public string savedAtUtc = "";

        public bool HasKilled(string enemyId) => killedEnemyIds.Contains(enemyId);
        public void MarkKilled(string enemyId) { if (!killedEnemyIds.Contains(enemyId)) killedEnemyIds.Add(enemyId); }

        public bool HasDefeated(string bossId) => defeatedBossIds.Contains(bossId);
        public void MarkDefeated(string bossId) { if (!defeatedBossIds.Contains(bossId)) defeatedBossIds.Add(bossId); }

        public bool HasFlag(string flag) => flags.Contains(flag);
        public void SetFlag(string flag, bool value)
        {
            if (value) { if (!flags.Contains(flag)) flags.Add(flag); }
            else flags.Remove(flag);
        }

        public PlayerAbilities ToAbilities() => new PlayerAbilities(canDash, canFireball, maxHearts);
        public void FromAbilities(PlayerAbilities a) { canDash = a.canDash; canFireball = a.canFireball; maxHearts = a.maxHearts; }

        /// <summary>Docs/02 §9: new cycle keeps only the cycle number.</summary>
        public void ClearProgressForNewCycle()
        {
            phase = GameConstants.Phase1;
            lastBenchId = "";
            lastBenchScene = "";
            lastBenchX = 0f;
            lastBenchY = 0f;
            flameT = 0f;
            FromAbilities(PlayerAbilities.ForPhase(GameConstants.Phase1));
            killedEnemyIds.Clear();
            defeatedBossIds.Clear();
            flags.Clear();
            pendingCutscene = "";
            pendingNextScene = "";
        }

        public bool HasPendingCutscene => !string.IsNullOrEmpty(pendingCutscene);
    }

    /// <summary>JSON save in Application.persistentDataPath. Atomic write (temp file + move).</summary>
    public static class SaveSystem
    {
        public const string FileName = "shatranj_save.json";

        /// <summary>Tests set this to a temp path; null uses the real location.</summary>
        public static string PathOverride;

        public static string FilePath => string.IsNullOrEmpty(PathOverride)
            ? Path.Combine(Application.persistentDataPath, FileName)
            : PathOverride;

        public static bool Exists => File.Exists(FilePath);

        public static SaveData CreateNew() => new SaveData();

        /// <summary>Writes the data. Returns false (and logs) on IO failure.</summary>
        public static bool Save(SaveData data)
        {
            if (data == null) return false;
            try
            {
                data.savedAtUtc = DateTime.UtcNow.ToString("o");
                string json = JsonUtility.ToJson(data, prettyPrint: true);
                string path = FilePath;
                string dir = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(dir)) Directory.CreateDirectory(dir);

                string tmp = path + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(path)) File.Delete(path);
                File.Move(tmp, path);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"SaveSystem: save failed — {e.Message}");
                return false;
            }
        }

        /// <summary>Returns null when there is no save or the file is unreadable.</summary>
        public static SaveData Load()
        {
            string path = FilePath;
            if (!File.Exists(path)) return null;
            try
            {
                string json = File.ReadAllText(path);
                if (string.IsNullOrWhiteSpace(json)) return null;
                SaveData data = JsonUtility.FromJson<SaveData>(json);
                if (data == null) return null;
                data.killedEnemyIds ??= new List<string>();
                data.defeatedBossIds ??= new List<string>();
                data.flags ??= new List<string>();
                data.pendingCutscene ??= "";
                data.pendingNextScene ??= "";
                data.lastBenchId ??= "";
                data.lastBenchScene ??= "";
                return data;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveSystem: could not read save — {e.Message}");
                return null;
            }
        }

        public static void Delete()
        {
            try
            {
                string path = FilePath;
                if (File.Exists(path)) File.Delete(path);
                if (File.Exists(path + ".tmp")) File.Delete(path + ".tmp");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"SaveSystem: delete failed — {e.Message}");
            }
        }
    }
}
