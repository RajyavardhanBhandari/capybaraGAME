using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Levels
{
    public static class LevelCatalog
    {
        public const int MaxLevel = 500;
        public const int CurrentVersion = 1;
        private static readonly Dictionary<int, LevelDefinition> Cache = new Dictionary<int, LevelDefinition>();
        private static readonly LevelGenerationConfig Config = new LevelGenerationConfig();

        public static bool TryGet(int level, out LevelDefinition definition)
        {
            if (level < 1 || level > MaxLevel)
            {
                definition = null;
                return false;
            }

            definition = Get(level);
            return true;
        }

        public static LevelDefinition Get(int level)
        {
            if (level < 1 || level > MaxLevel)
                throw new ArgumentOutOfRangeException(nameof(level));

            if (Cache.TryGetValue(level, out var cached))
                return cached.Clone();

            var band = Config.GetBand(level);
            var hard = LevelProgressionRules.IsHardChallenge(level, Config.hardChallengeFrequency);
            var style = SelectStyle(level, band);
            var grid = SelectGridSize(level, band);
            var definition = new LevelDefinition
            {
                Id = level,
                Version = CurrentVersion,
                PuzzleId = "P" + level.ToString("000"),
                Seed = PuzzleSeed.ForLevel(level),
                GenerationVersion = Config.generationVersion,
                BoardSize = grid,
                RegionCount = grid,
                Difficulty = hard ? PuzzleDifficultyBand.Hard : TargetBand(level, band),
                Style = style,
                IsHardChallenge = hard,
                HardChallengeIndex = LevelProgressionRules.HardChallengeIndex(level, Config.hardChallengeFrequency),
                ProgressionBand = LevelProgressionRules.ProgressionBandForLevel(level),
                Fingerprint = string.Empty
            };

            Cache[level] = definition;
            return definition.Clone();
        }

        public static int NextLevel(int level) => level >= 1 && level < MaxLevel ? level + 1 : -1;

        public static void ClearCache() => Cache.Clear();

        public static LevelGenerationConfig Configuration => Config;

        private static PuzzleDifficultyBand TargetBand(int level, ProgressionBandConfig band)
        {
            if (band == null) return PuzzleDifficultyBand.Easy;
            float normalized = band.targetDifficultyMin +
                               (band.targetDifficultyMax - band.targetDifficultyMin) *
                               ((level - band.startLevel) / (float)Math.Max(1, band.endLevel - band.startLevel));
            return LevelProgressionRules.CategoryForScore(normalized, Config.difficultyProfile);
        }

        private static int SelectGridSize(int level, ProgressionBandConfig band)
        {
            if (band == null || band.allowedGridSizes == null || band.allowedGridSizes.Length == 0) return 5;
            uint x = unchecked((uint)level * 2654435761u + (uint)Config.seed);
            return band.allowedGridSizes[(int)(x % (uint)band.allowedGridSizes.Length)];
        }

        private static PuzzleCategory SelectStyle(int level, ProgressionBandConfig band)
        {
            if (band == null || band.styleWeights == null || band.styleWeights.Length < 6)
                return PuzzleCategory.Balanced;

            uint x = unchecked((uint)level * 2246822519u + (uint)Config.seed);
            float sample = (x / (float)uint.MaxValue);
            float total = 0f;
            for (int i = 0; i < 6; i++)
            {
                total += band.styleWeights[i];
                if (sample <= total) return (PuzzleCategory)i;
            }
            return PuzzleCategory.ChainReaction;
        }
    }
}