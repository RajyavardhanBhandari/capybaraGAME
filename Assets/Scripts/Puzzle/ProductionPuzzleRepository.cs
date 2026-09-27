using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Levels;
using UnityEngine;

namespace CapybaraGame.Puzzle
{
    public static class ProductionPuzzleRepository
    {
        private static readonly Dictionary<int, PuzzleDefinition> Cache = new Dictionary<int, PuzzleDefinition>();
        private static LevelDatabaseAsset levelDatabase;
        private static PuzzleDatabaseAsset puzzleDatabase;

        public static PuzzleDefinition Get(int level)
        {
            if (!LevelCatalog.TryGet(level, out var definition))
                throw new ArgumentOutOfRangeException(nameof(level));

            if (Cache.TryGetValue(level, out var cached)) return cached.Clone();

            EnsureDatabasesLoaded();

            PuzzleDefinition puzzle = null;
            if (levelDatabase != null && puzzleDatabase != null &&
                levelDatabase.levels != null && puzzleDatabase.puzzles != null)
            {
                for (int i = 0; i < levelDatabase.levels.Count; i++)
                {
                    var entry = levelDatabase.levels[i];
                    if (entry == null || entry.Id != level) continue;

                    for (int p = 0; p < puzzleDatabase.puzzles.Count; p++)
                    {
                        var candidate = puzzleDatabase.puzzles[p];
                        if (candidate != null && candidate.id == entry.PuzzleId)
                        {
                            puzzle = candidate.Clone();
                            break;
                        }
                    }
                    break;
                }
            }

            if (puzzle == null)
            {
                var result = PuzzleGenerator.Generate(definition.CreatePuzzleConfig());
                if (result.Puzzle == null)
                {
                    var fallback = definition.CreatePuzzleConfig();
                    fallback.targetBand = null;
                    result = PuzzleGenerator.Generate(fallback);
                }

                if (result.Puzzle == null)
                    throw new InvalidOperationException(
                        "Production puzzle generation failed for level " + level + ": " + result.FailureReason);

                puzzle = result.Puzzle;
                puzzle.id = definition.PuzzleId;
            }

            puzzle.isHardChallenge = definition.IsHardChallenge;
            Cache[level] = puzzle.Clone();
            return puzzle.Clone();
        }

        public static void ClearCache()
        {
            Cache.Clear();
            levelDatabase = null;
            puzzleDatabase = null;
        }

        private static void EnsureDatabasesLoaded()
        {
            if (levelDatabase == null)
                levelDatabase = Resources.Load<LevelDatabaseAsset>("Levels/LevelDatabase");
            if (puzzleDatabase == null)
                puzzleDatabase = Resources.Load<PuzzleDatabaseAsset>("Levels/PuzzleDatabase");
        }
    }
}