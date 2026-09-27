#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using CapybaraGame.Core;
using CapybaraGame.Levels;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Editor
{
    public static class LevelValidationWindow
    {
        [MenuItem("Capybara/Level System/Validate 500 Levels")]
        public static void RunValidation()
        {
            int failures = 0;
            int hardChallenges = 0;
            var fingerprints = new HashSet<string>();
            var recent = new Queue<PuzzleDefinition>();
            var config = LevelCatalog.Configuration;

            for (int level = 1; level <= LevelCatalog.MaxLevel; level++)
            {
                try
                {
                    var definition = LevelCatalog.Get(level);
                    var puzzle = ProductionPuzzleRepository.Get(level);

                    if (definition.Id != level) Fail(ref failures, "Wrong level ID " + level);
                    if (definition.IsHardChallenge != (level % config.hardChallengeFrequency == 0))
                        Fail(ref failures, "Hard Challenge cadence failed at " + level);
                    if (definition.IsHardChallenge) hardChallenges++;

                    if (puzzle == null ||
                        !PuzzleValidator.IsRegionMapValid(puzzle) ||
                        PuzzleSolver.Solve(puzzle, 2).SolutionCount != 1)
                        Fail(ref failures, "Puzzle validity/uniqueness failed at " + level);

                    if (!fingerprints.Add(puzzle.fingerprint))
                        Fail(ref failures, "Duplicate fingerprint at " + level);

                    foreach (var previous in recent)
                        if (PuzzleFingerprint.StructuralDistance(puzzle, previous) <= config.variety.minimumFingerprintDistance)
                            Fail(ref failures, "Similar recent puzzle at " + level);

                    recent.Enqueue(puzzle);
                    while (recent.Count > config.variety.recentFingerprintWindow) recent.Dequeue();
                }
                catch (System.Exception e)
                {
                    failures++;
                    Debug.LogException(e);
                }
            }

            if (hardChallenges != 50)
                Fail(ref failures, "Expected 50 Hard Challenges, found " + hardChallenges);

            var levelAsset = Resources.Load<LevelDatabaseAsset>("Levels/LevelDatabase");
            var puzzleAsset = Resources.Load<PuzzleDatabaseAsset>("Levels/PuzzleDatabase");
            if (levelAsset != null)
            {
                if (levelAsset.levels == null || levelAsset.levels.Count != LevelCatalog.MaxLevel)
                    Fail(ref failures, "Persistent LevelDatabase asset is not a complete 500-level database.");
            }
            else
            {
                Debug.LogWarning("Persistent LevelDatabase asset has not been generated yet.");
            }

            if (puzzleAsset != null)
            {
                if (puzzleAsset.puzzles == null || puzzleAsset.puzzles.Count != LevelCatalog.MaxLevel)
                    Fail(ref failures, "Persistent PuzzleDatabase asset is not a complete 500-puzzle database.");
            }
            else
            {
                Debug.LogWarning("Persistent PuzzleDatabase asset has not been generated yet.");
            }

            string result = failures == 0
                ? "PHASE 5 VALIDATION PASSED"
                : "PHASE 5 VALIDATION FAILED: " + failures + " issue(s)";

            const string reportDirectory = "Assets/Data/Levels";
            if (!Directory.Exists(reportDirectory)) Directory.CreateDirectory(reportDirectory);
            File.WriteAllText(
                Path.Combine(reportDirectory, "LevelValidationReport.txt"),
                result + System.Environment.NewLine +
                "HardChallenges=" + hardChallenges + System.Environment.NewLine +
                "Fingerprints=" + fingerprints.Count + System.Environment.NewLine);

            AssetDatabase.Refresh();
            Debug.Log(result);
            EditorUtility.DisplayDialog("Capybara Level Validation", result, "OK");
        }

        private static void Fail(ref int failures, string message)
        {
            failures++;
            Debug.LogError(message);
        }
    }
}
#endif