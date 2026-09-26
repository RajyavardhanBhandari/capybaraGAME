using System;
using System.Collections.Generic;
using CapybaraGame.Core;

namespace CapybaraGame.Puzzle
{
    public static class PuzzleRepository
    {
        public const int LaunchLevelCount = 500;
        public const int GeneratorVersion = 1;
        private static readonly Dictionary<int, PuzzleDefinition> Cache = new Dictionary<int, PuzzleDefinition>();

        public static PuzzleDefinition Get(int level)
        {
            if (level < 1 || level > LaunchLevelCount)
                throw new ArgumentOutOfRangeException(nameof(level));
            if (Cache.TryGetValue(level, out var cached)) return cached.Clone();
            var puzzle = ProductionPuzzleRepository.Get(level);
            Cache[level] = puzzle.Clone();
            return puzzle.Clone();
        }

        public static PuzzleGenerationResult Generate(PuzzleGenerationConfig config)
            => PuzzleGenerator.Generate(config);

        public static string Fingerprint(PuzzleDefinition puzzle)
            => PuzzleFingerprint.Compute(puzzle);

        public static int StableSeed(int level)
        {
            unchecked
            {
                uint x = (uint)level * 0x9E3779B9u;
                x ^= 0xC0FEBABEu;
                x ^= x >> 16;
                x *= 0x85EBCA6Bu;
                x ^= x >> 13;
                return (int)x;
            }
        }

        public static void ClearCache()
        {
            Cache.Clear();
            ProductionPuzzleRepository.ClearCache();
        }
    }
}