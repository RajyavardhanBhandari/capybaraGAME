using System;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Levels
{
    [Serializable]
    public sealed class ProgressionBandConfig
    {
        public int startLevel;
        public int endLevel;
        public float targetDifficultyMin;
        public float targetDifficultyMax;
        public int[] allowedGridSizes;
        public float difficultyVariance = 8f;
        public float[] styleWeights = new float[6];

        public bool Contains(int level) => level >= startLevel && level <= endLevel;
    }

    [Serializable]
    public sealed class LevelVarietyConfig
    {
        public int maxSameGridSizeInRow = 2;
        public int maxSameStyleInRow = 2;
        public int recentFingerprintWindow = 12;
        public int minimumFingerprintDistance = 2;
        public int varietyTargetPerTen = 3;
        public int varietyTargetPerFifty = 5;
    }

    [Serializable]
    public sealed class LevelGenerationConfig
    {
        public int launchLevelCount = 500;
        public int generationVersion = 1;
        public int candidateCount = 10000;
        public int seed = 20260927;
        public int hardChallengeFrequency = 10;
        public DifficultyProfile difficultyProfile = new DifficultyProfile();
        public LevelVarietyConfig variety = new LevelVarietyConfig();
        public ProgressionBandConfig[] bands = DefaultBands();

        public static ProgressionBandConfig[] DefaultBands()
        {
            return new[]
            {
                Band(1, 100, 0f, 42f, new[] { 4, 5, 6 }, 8f, .40f, .35f, .15f, .05f, .03f, .02f),
                Band(101, 200, 28f, 55f, new[] { 4, 5, 6 }, 9f, .20f, .30f, .18f, .12f, .10f, .10f),
                Band(201, 300, 42f, 70f, new[] { 5, 6, 7 }, 10f, .10f, .20f, .20f, .18f, .14f, .18f),
                Band(301, 400, 55f, 82f, new[] { 5, 6, 7 }, 10f, .08f, .14f, .20f, .20f, .16f, .22f),
                Band(401, 500, 65f, 96f, new[] { 5, 6, 7 }, 11f, .05f, .10f, .18f, .20f, .20f, .27f)
            };
        }

        static ProgressionBandConfig Band(
            int start, int end, float min, float max, int[] grids, float variance,
            float balanced, float sparse, float complex, float deep, float quick, float chain)
        {
            return new ProgressionBandConfig
            {
                startLevel = start,
                endLevel = end,
                targetDifficultyMin = min,
                targetDifficultyMax = max,
                allowedGridSizes = grids,
                difficultyVariance = variance,
                styleWeights = new[] { balanced, sparse, complex, deep, quick, chain }
            };
        }

        public ProgressionBandConfig GetBand(int level)
        {
            if (bands != null)
                foreach (var band in bands)
                    if (band.Contains(level)) return band;
            return bands == null || bands.Length == 0 ? null : bands[bands.Length - 1];
        }
    }

    public static class LevelProgressionRules
    {
        public static int ProgressionBandForLevel(int level)
        {
            if (level <= 100) return 1;
            if (level <= 200) return 2;
            if (level <= 300) return 3;
            if (level <= 400) return 4;
            return 5;
        }

        public static bool IsHardChallenge(int level, int frequency)
            => frequency > 0 && level > 0 && level % frequency == 0;

        public static int HardChallengeIndex(int level, int frequency)
            => IsHardChallenge(level, frequency) ? level / frequency : 0;

        public static PuzzleDifficultyBand CategoryForScore(float score, DifficultyProfile profile)
            => score < profile.EasyMax ? PuzzleDifficultyBand.Easy :
               score < profile.MediumMax ? PuzzleDifficultyBand.Medium :
               PuzzleDifficultyBand.Hard;

        public static float HardChallengeTargetScore(int index)
        {
            if (index < 1) throw new ArgumentOutOfRangeException(nameof(index));
            return Math.Min(96f, 55f + (index - 1) * (36f / 49f));
        }
    }
}