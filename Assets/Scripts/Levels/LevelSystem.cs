using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Levels
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public int Id;
        public string DisplayName;
        public PuzzleDifficultyBand Difficulty;
        public PuzzleCategory Category;
        public int BoardSize;
        public int RegionCount;
        public int Seed;
        public bool IsHardChallenge;
        public bool IsBreather;
        public bool IsTeaching;
        public bool IsUnlockedByPrevious;

        public PuzzleGenerationConfig CreatePuzzleConfig()
        {
            return new PuzzleGenerationConfig
            {
                rows = BoardSize,
                columns = BoardSize,
                regionCount = RegionCount,
                seed = Seed,
                generationVersion = PuzzleRepository.GeneratorVersion,
                maxAttempts = 5000,
                targetBand = Difficulty
            };
        }
    }

    public static class LevelCatalog
    {
        public const int MaxLevel = 500;
        private static readonly Dictionary<int, LevelDefinition> Cache = new Dictionary<int, LevelDefinition>();

        private sealed class Tier
        {
            public int MaxLevel;
            public int BoardSize;
            public PuzzleDifficultyBand Difficulty;
            public PuzzleCategory Category;
            public Tier(int maxLevel, int boardSize, PuzzleDifficultyBand difficulty, PuzzleCategory category)
            {
                MaxLevel = maxLevel; BoardSize = boardSize; Difficulty = difficulty; Category = category;
            }
        }

        private static readonly Tier[] Tiers =
        {
            new Tier(20, 4, PuzzleDifficultyBand.Easy, PuzzleCategory.QuickSolve),
            new Tier(50, 5, PuzzleDifficultyBand.Easy, PuzzleCategory.Balanced),
            new Tier(100, 5, PuzzleDifficultyBand.Medium, PuzzleCategory.Sparse),
            new Tier(200, 6, PuzzleDifficultyBand.Medium, PuzzleCategory.ComplexRegions),
            new Tier(300, 6, PuzzleDifficultyBand.Hard, PuzzleCategory.DeepLogic),
            new Tier(400, 7, PuzzleDifficultyBand.Hard, PuzzleCategory.ChainReaction),
            new Tier(500, 7, PuzzleDifficultyBand.Hard, PuzzleCategory.DeepLogic)
        };

        public static bool TryGet(int level, out LevelDefinition definition)
        {
            if (level < 1 || level > MaxLevel) { definition = null; return false; }
            definition = Get(level);
            return true;
        }

        public static LevelDefinition Get(int level)
        {
            if (level < 1 || level > MaxLevel) throw new ArgumentOutOfRangeException(nameof(level));
            if (Cache.TryGetValue(level, out var cached)) return Clone(cached);
            var tier = FindTier(level);
            var definition = new LevelDefinition
            {
                Id = level,
                DisplayName = "Level " + level,
                Difficulty = level % 10 == 0 ? PuzzleDifficultyBand.Hard : tier.Difficulty,
                Category = tier.Category,
                BoardSize = tier.BoardSize,
                RegionCount = tier.BoardSize,
                Seed = PuzzleSeed.ForLevel(level),
                IsHardChallenge = level % 10 == 0,
                IsBreather = level > 4 && level % 10 == 5,
                IsTeaching = level <= 4,
                IsUnlockedByPrevious = level > 1
            };
            Cache[level] = definition;
            return Clone(definition);
        }

        public static int NextLevel(int level) => level < MaxLevel ? level + 1 : -1;
        public static void ClearCache() => Cache.Clear();

        private static Tier FindTier(int level)
        {
            for (int i = 0; i < Tiers.Length; i++) if (level <= Tiers[i].MaxLevel) return Tiers[i];
            return Tiers[Tiers.Length - 1];
        }

        private static LevelDefinition Clone(LevelDefinition source)
        {
            return new LevelDefinition
            {
                Id = source.Id, DisplayName = source.DisplayName, Difficulty = source.Difficulty,
                Category = source.Category, BoardSize = source.BoardSize, RegionCount = source.RegionCount,
                Seed = source.Seed, IsHardChallenge = source.IsHardChallenge,
                IsBreather = source.IsBreather, IsTeaching = source.IsTeaching,
                IsUnlockedByPrevious = source.IsUnlockedByPrevious
            };
        }
    }
}