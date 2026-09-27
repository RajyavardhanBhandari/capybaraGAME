using System.Collections.Generic;

namespace CapybaraGame.Puzzle
{
    /// <summary>Runtime fingerprint memory used to avoid repeatedly serving near-identical puzzle structures.</summary>
    public static class PuzzleExperienceLibrary
    {
        private static readonly List<PuzzleDefinition> recent = new List<PuzzleDefinition>();
        private const int MaxRecent = 24;

        public static bool IsTooSimilar(PuzzleDefinition candidate)
        {
            if (candidate == null) return true;
            for (int i = 0; i < recent.Count; i++)
                if (PuzzleFingerprint.StructurallySimilar(recent[i], candidate))
                    return true;
            return false;
        }

        public static void Remember(PuzzleDefinition puzzle)
        {
            if (puzzle == null) return;
            recent.Add(puzzle.Clone());
            while (recent.Count > MaxRecent) recent.RemoveAt(0);
        }

        public static void Clear() => recent.Clear();
    }
}