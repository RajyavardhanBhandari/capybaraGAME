using System;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Levels
{
    [Serializable]
    public sealed class LevelDefinition
    {
        public int Id;
        public int Version = 1;
        public string PuzzleId;
        public int Seed;
        public int GenerationVersion;
        public int BoardSize;
        public int RegionCount;
        public float DifficultyScore;
        public PuzzleDifficultyBand Difficulty;
        public PuzzleCategory Style;
        public bool IsHardChallenge;
        public int HardChallengeIndex;
        public int ProgressionBand;
        public string Fingerprint;

        public PuzzleGenerationConfig CreatePuzzleConfig(DifficultyProfile profile = null)
        {
            return new PuzzleGenerationConfig
            {
                rows = BoardSize,
                columns = BoardSize,
                regionCount = RegionCount,
                seed = Seed,
                generationVersion = GenerationVersion,
                maxAttempts = 5000,
                requireUniqueSolution = true,
                requireConnectedRegions = true,
                difficultyProfile = profile ?? new DifficultyProfile(),
                targetBand = Difficulty,
                category = Style
            };
        }

        public LevelDefinition Clone()
        {
            return (LevelDefinition)MemberwiseClone();
        }
    }
}