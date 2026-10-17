using System.Collections.Generic;
using Shatranj.Data;
using UnityEngine;

namespace Shatranj.Bosses
{
    /// <summary>
    /// Distance-banded weighted move selection (Docs/04 §D Iron Colossus): only moves whose
    /// <see cref="BossMoveConfig.distanceBand"/> includes the current band (0 = any) and whose
    /// <see cref="BossMoveConfig.minStage"/> is reached are candidates; a move that already ran
    /// <c>maxSameInRow</c> times in a row is excluded. Pure logic so it is unit-tested without a scene.
    /// </summary>
    public static class BossMoveSelector
    {
        public const int Close = 1, Mid = 2, Far = 4;

        /// <summary>All bands: distance doesn't matter (Zulmat's weighted random).</summary>
        public const int AnyBand = Close | Mid | Far;

        /// <summary>Band bit for a horizontal distance: close &lt; closeDistance ≤ mid ≤ farDistance &lt; far.</summary>
        public static int BandFor(float distance, float closeDistance, float farDistance)
        {
            if (distance < closeDistance) return Close;
            if (distance > farDistance) return Far;
            return Mid;
        }

        /// <summary>How many times <paramref name="move"/> ends the history (most recent last).</summary>
        public static int TrailingCount(IReadOnlyList<string> history, string move)
        {
            int n = 0;
            for (int i = history.Count - 1; i >= 0 && history[i] == move; i--) n++;
            return n;
        }

        public static bool IsCandidate(BossMoveConfig m, int band, int stage, IReadOnlyList<string> history, int maxSameInRow,
                                       System.Predicate<BossMoveConfig> available = null)
        {
            if (m == null || m.weight <= 0f || m.minStage > stage) return false;
            if (available != null && !available(m)) return false;
            if (m.distanceBand != 0 && (m.distanceBand & band) == 0) return false;
            if (maxSameInRow > 0 && TrailingCount(history, m.name) >= maxSameInRow) return false;
            return true;
        }

        /// <summary>Was <paramref name="move"/> one of the last <paramref name="window"/> moves?</summary>
        public static bool UsedRecently(IReadOnlyList<string> history, string move, int window)
        {
            for (int i = history.Count - 1, n = 0; i >= 0 && n < window; i--, n++)
                if (history[i] == move) return true;
            return false;
        }

        /// <summary>
        /// Like <see cref="Pick"/> with no immediate repeat, but first tries to avoid every move used in the last
        /// <paramref name="window"/> moves (more variety); falls back to plain no-immediate-repeat when that leaves nothing.
        /// </summary>
        public static BossMoveConfig PickVaried(IReadOnlyList<BossMoveConfig> moves, int band, int stage, IReadOnlyList<string> history,
                                                int window, float roll, System.Predicate<BossMoveConfig> available = null)
        {
            if (window > 1)
            {
                BossMoveConfig fresh = Pick(moves, band, stage, history, 1, roll,
                    m => (available == null || available(m)) && !UsedRecently(history, m.name, window));
                if (fresh != null) return fresh;
            }
            return Pick(moves, band, stage, history, 1, roll, available);
        }

        /// <summary>
        /// Picks a move for <paramref name="band"/> using <paramref name="roll"/> in [0, 1). Returns null when no move
        /// fits (the caller walks instead).
        /// </summary>
        public static BossMoveConfig Pick(IReadOnlyList<BossMoveConfig> moves, int band, int stage, IReadOnlyList<string> history, int maxSameInRow, float roll,
                                          System.Predicate<BossMoveConfig> available = null)
        {
            float total = 0f;
            for (int i = 0; i < moves.Count; i++)
                if (IsCandidate(moves[i], band, stage, history, maxSameInRow, available)) total += moves[i].weight;
            if (total <= 0f) return null;

            float r = Mathf.Clamp01(roll) * total;
            BossMoveConfig lastCandidate = null;
            for (int i = 0; i < moves.Count; i++)
            {
                if (!IsCandidate(moves[i], band, stage, history, maxSameInRow, available)) continue;
                lastCandidate = moves[i];
                if (r < moves[i].weight) return moves[i];
                r -= moves[i].weight;
            }
            return lastCandidate;
        }
    }
}
