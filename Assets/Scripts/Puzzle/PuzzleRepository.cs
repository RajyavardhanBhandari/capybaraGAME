using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using UnityEngine;

namespace CapybaraGame.Puzzle
{
    /// <summary>
    /// Deterministic Phase 2 puzzle content generator.
    /// It generates the launch-scale P001-P500 content shape without storing
    /// hundreds of large serialized puzzle files in the repository.
    /// </summary>
    public static class PuzzleRepository
    {
        public const int LaunchLevelCount = 500;
        public const string GeneratorVersion = "phase2-v1";

        private static readonly Dictionary<int, PuzzleDefinition> Cache =
            new Dictionary<int, PuzzleDefinition>();

        public static PuzzleDefinition Get(int level)
        {
            level = Mathf.Clamp(level, 1, LaunchLevelCount);

            if (Cache.TryGetValue(level, out var cached))
                return cached;

            int seed = 100000 + level * 7919;
            int[] solution = GenerateSolution(seed);
            int[] regions = BuildConnectedRegions(solution, seed);
            float difficulty = EstimateDifficulty(level, regions, solution);

            var puzzle = new PuzzleDefinition
            {
                id = $"P{level:000}",
                seed = seed,
                rows = 10,
                columns = 10,
                regions = regions,
                solution = solution,
                difficulty = difficulty,
                difficultyBand = GetDifficultyBand(level, difficulty),
                generatorVersion = GeneratorVersion
            };

            Cache[level] = puzzle;
            return puzzle;
        }

        public static string Fingerprint(PuzzleDefinition puzzle)
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 31 + puzzle.seed;
                hash = hash * 31 + puzzle.rows;
                hash = hash * 31 + puzzle.columns;

                for (int i = 0; i < puzzle.regions.Length; i++)
                    hash = hash * 31 + puzzle.regions[i];

                for (int i = 0; i < puzzle.solution.Length; i++)
                    hash = hash * 31 + puzzle.solution[i];

                return $"{hash:X8}";
            }
        }

        private static int[] GenerateSolution(int seed)
        {
            var random = new System.Random(seed);
            var result = new int[10];
            var used = new bool[10];

            for (int i = 0; i < result.Length; i++)
                result[i] = -1;

            if (!BacktrackSolution(0, result, used, random))
                throw new InvalidOperationException($"Unable to generate puzzle solution for seed {seed}.");

            return result;
        }

        private static bool BacktrackSolution(
            int row,
            int[] result,
            bool[] used,
            System.Random random)
        {
            if (row == result.Length)
                return true;

            var candidates = new List<int>();
            for (int col = 0; col < 10; col++)
            {
                if (used[col]) continue;

                if (row > 0 && Math.Abs(result[row - 1] - col) <= 1)
                    continue;

                candidates.Add(col);
            }

            Shuffle(candidates, random);

            foreach (int col in candidates)
            {
                result[row] = col;
                used[col] = true;

                if (BacktrackSolution(row + 1, result, used, random))
                    return true;

                used[col] = false;
                result[row] = -1;
            }

            return false;
        }

        private static void Shuffle(List<int> values, System.Random random)
        {
            for (int i = values.Count - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int temp = values[i];
                values[i] = values[j];
                values[j] = temp;
            }
        }

        private static int[] BuildConnectedRegions(int[] solution, int seed)
        {
            const int size = 10;
            int[] regions = new int[size * size];
            int[] distance = new int[size * size];
            var queue = new Queue<int>();

            for (int i = 0; i < regions.Length; i++)
            {
                regions[i] = -1;
                distance[i] = int.MaxValue;
            }

            // Multi-source expansion from the ten solution cells.
            // Every region therefore contains its intended solution anchor
            // and grows as a connected area.
            for (int row = 0; row < size; row++)
            {
                int index = row * size + solution[row];
                regions[index] = row;
                distance[index] = 0;
                queue.Enqueue(index);
            }

            var random = new System.Random(seed ^ 0x5F3759DF);

            while (queue.Count > 0)
            {
                int index = queue.Dequeue();
                int row = index / size;
                int col = index % size;

                var neighbors = new List<int>(4);
                AddNeighbor(neighbors, row - 1, col);
                AddNeighbor(neighbors, row + 1, col);
                AddNeighbor(neighbors, row, col - 1);
                AddNeighbor(neighbors, row, col + 1);
                Shuffle(neighbors, random);

                foreach (int next in neighbors)
                {
                    int nextRow = next / size;
                    int nextCol = next % size;
                    int nextDistance = distance[index] + 1;

                    if (regions[next] == -1)
                    {
                        regions[next] = regions[index];
                        distance[next] = nextDistance;
                        queue.Enqueue(next);
                    }
                    else if (nextDistance == distance[next] &&
                             random.Next(2) == 0 &&
                             regions[next] != regions[index])
                    {
                        // Deterministic tie-breaking through the seeded RNG.
                        regions[next] = regions[index];
                    }
                }
            }

            return regions;
        }

        private static void AddNeighbor(List<int> list, int row, int col)
        {
            if (row < 0 || row >= 10 || col < 0 || col >= 10) return;
            list.Add(row * 10 + col);
        }

        private static float EstimateDifficulty(int level, int[] regions, int[] solution)
        {
            float progression = Mathf.Clamp01((level - 1) / 499f);
            float regionBalance = CalculateRegionBalance(regions);
            float spacing = CalculateSolutionSpacing(solution);

            // This is a tuning estimate, not a measured solve-time score.
            float score = 0.20f
                        + progression * 0.55f
                        + (1f - regionBalance) * 0.15f
                        + spacing * 0.10f;

            return Mathf.Clamp(score, 0.20f, 0.98f);
        }

        private static float CalculateRegionBalance(int[] regions)
        {
            var counts = new int[10];
            for (int i = 0; i < regions.Length; i++)
                counts[regions[i]]++;

            float deviation = 0f;
            for (int i = 0; i < counts.Length; i++)
                deviation += Mathf.Abs(counts[i] - 10);

            return Mathf.Clamp01(1f - deviation / 100f);
        }

        private static float CalculateSolutionSpacing(int[] solution)
        {
            float total = 0f;
            for (int row = 1; row < solution.Length; row++)
                total += Mathf.Abs(solution[row] - solution[row - 1]);

            return Mathf.Clamp01(total / 81f);
        }

        private static string GetDifficultyBand(int level, float difficulty)
        {
            if (level % 10 == 0)
                return "Hard Challenge";

            if (difficulty < 0.38f)
                return "Easy";

            if (difficulty < 0.62f)
                return "Normal";

            if (difficulty < 0.82f)
                return "Hard";

            return "Expert";
        }
    }
}
