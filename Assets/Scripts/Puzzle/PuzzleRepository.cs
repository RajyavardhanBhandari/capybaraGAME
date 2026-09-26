using System.Collections.Generic;
using CapybaraGame.Core;
using UnityEngine;

namespace CapybaraGame.Puzzle
{
    public static class PuzzleRepository
    {
        private static readonly int[][] Solutions =
        {
            new[] { 0,2,4,6,8,1,3,5,7,9 },
            new[] { 9,7,5,3,1,8,6,4,2,0 },
            new[] { 1,3,5,7,9,0,2,4,6,8 }
        };

        public static PuzzleDefinition Get(int level)
        {
            int index = Mathf.Clamp((level - 1) % Solutions.Length, 0, Solutions.Length - 1);
            var solution = Solutions[index];
            var regions = BuildRegions(solution);

            return new PuzzleDefinition
            {
                id = $"P{level:000}",
                seed = 1000 + level,
                rows = 10,
                columns = 10,
                regions = regions,
                solution = solution,
                difficulty = Mathf.Clamp(0.25f + level * 0.01f, 0.25f, 0.95f),
                difficultyBand = level % 10 == 0 ? "Hard Challenge" : level < 4 ? "Easy" : "Normal"
            };
        }

        private static int[] BuildRegions(int[] solution)
        {
            var regions = new int[100];

            for (int r = 0; r < 10; r++)
            {
                for (int c = 0; c < 10; c++)
                {
                    int bestRegion = 0;
                    int bestDistance = int.MaxValue;

                    for (int region = 0; region < 10; region++)
                    {
                        int distance = Mathf.Abs(r - region) + Mathf.Abs(c - solution[region]);
                        if (distance < bestDistance)
                        {
                            bestDistance = distance;
                            bestRegion = region;
                        }
                    }

                    regions[r * 10 + c] = bestRegion;
                }
            }

            return regions;
        }
    }
}
