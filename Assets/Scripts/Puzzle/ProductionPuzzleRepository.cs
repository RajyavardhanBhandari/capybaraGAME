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

            PuzzleGenerationResult result = null;
            PuzzleDefinition selected = null;
            for (int attempt = 0; attempt < 6; attempt++)
            {
                var config = definition.CreatePuzzleConfig();
                config.seed = definition.Seed + attempt * 7919;
                result = PuzzleGenerator.Generate(config);
                if (result.Puzzle == null) continue;
                if (!PuzzleExperienceLibrary.IsTooSimilar(result.Puzzle) || attempt == 5)
                {
                    selected = result.Puzzle;
                    break;
                }
            }

            if (selected == null)
            {
                var fallback = definition.CreatePuzzleConfig();
                fallback.targetBand = null;
                result = PuzzleGenerator.Generate(fallback);
                selected = result.Puzzle;
            }

            if (selected == null)
                throw new InvalidOperationException("Production puzzle generation failed for level " + level + ": " + result.FailureReason);

            selected.id = "P" + level.ToString("000");
            selected.isHardChallenge = definition.IsHardChallenge;
            var quality = PuzzleDifficultyEvaluator.Evaluate(selected);
            selected.difficulty = quality.Score;
            selected.difficultyBand = quality.Band.ToString();
            selected.fingerprint = PuzzleFingerprint.Compute(selected);
            cache[level] = selected;
            PuzzleExperienceLibrary.Remember(selected);
            return selected.Clone();
        }

        public static void ClearCache() => cache.Clear();
    }
}