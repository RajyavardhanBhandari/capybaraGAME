using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Levels;

namespace CapybaraGame.Puzzle
{
    public static class ProductionPuzzleRepository
    {
        static readonly Dictionary<int, PuzzleDefinition> cache = new Dictionary<int, PuzzleDefinition>();

        public static PuzzleDefinition Get(int level)
        {
            if (!LevelCatalog.TryGet(level, out var definition))
                throw new ArgumentOutOfRangeException(nameof(level));

            if (cache.TryGetValue(level, out var cached)) return cached.Clone();

            var result = PuzzleGenerator.Generate(definition.CreatePuzzleConfig());
            if (result.Puzzle == null)
            {
                var fallback = definition.CreatePuzzleConfig();
                fallback.targetBand = null;
                result = PuzzleGenerator.Generate(fallback);
            }

            if (result.Puzzle == null)
                throw new InvalidOperationException("Production puzzle generation failed for level " + level + ": " + result.FailureReason);

            result.Puzzle.id = "P" + level.ToString("000");
            result.Puzzle.isHardChallenge = definition.IsHardChallenge;
            cache[level] = result.Puzzle;
            return result.Puzzle.Clone();
        }

        public static void ClearCache() => cache.Clear();
    }
}