using System;
using System.Collections.Generic;
using CapybaraGame.Core;

namespace CapybaraGame.Levels
{
    [Serializable]
    public sealed class LevelDatabase
    {
        public int databaseVersion = 1;
        public int generationVersion = 1;
        public List<LevelDefinition> levels = new List<LevelDefinition>();

        public bool TryGet(int level, out LevelDefinition definition)
        {
            definition = null;
            if (levels == null) return false;
            for (int i = 0; i < levels.Count; i++)
                if (levels[i].Id == level) { definition = levels[i].Clone(); return true; }
            return false;
        }

        public bool IsComplete(int expectedCount)
        {
            if (levels == null || levels.Count != expectedCount) return false;
            var seen = new HashSet<int>();
            foreach (var level in levels)
                if (level == null || level.Id < 1 || level.Id > expectedCount || !seen.Add(level.Id))
                    return false;
            return seen.Count == expectedCount;
        }
    }

    [Serializable]
    public sealed class PuzzleDatabase
    {
        public int databaseVersion = 1;
        public int generationVersion = 1;
        public List<PuzzleRecord> puzzles = new List<PuzzleRecord>();
    }

    [Serializable]
    public sealed class PuzzleRecord
    {
        public string PuzzleId;
        public PuzzleDefinitionData Data;

        public PuzzleRecord Clone() => new PuzzleRecord { PuzzleId = PuzzleId, Data = Data };
    }

    [Serializable]
    public sealed class PuzzleDefinitionData
    {
        public string id;
        public int seed;
        public int rows;
        public int columns;
        public int regionCount;
        public int[] regions;
        public int[] solution;
        public float difficulty;
        public string difficultyBand;
        public string category;
        public string fingerprint;
        public string generatorVersion;
        public int solutionCount;
        public bool isHardChallenge;
    }
}