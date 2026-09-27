#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using CapybaraGame.Levels;

namespace CapybaraGame.Editor
{
    public sealed class LevelGenerationWindow : EditorWindow
    {
        private int candidateCount = 10000;
        private int seed = 20260927;
        private int generationVersion = 1;
        private int launchCount = 500;
        private string lastReport = "Not generated.";

        [MenuItem("Capybara/Level System/Level Generator")]
        public static void Open() => GetWindow<LevelGenerationWindow>("Level Generator");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Phase 5 — Level Generator", EditorStyles.boldLabel);
            candidateCount = EditorGUILayout.IntField("Candidates", candidateCount);
            launchCount = EditorGUILayout.IntField("Launch Levels", launchCount);
            seed = EditorGUILayout.IntField("Seed", seed);
            generationVersion = EditorGUILayout.IntField("Generation Version", generationVersion);

            if (GUILayout.Button("Generate Candidate Pool + Level Database"))
            {
                var config = new LevelGenerationConfig
                {
                    candidateCount = Mathf.Max(500, candidateCount),
                    launchLevelCount = Mathf.Clamp(launchCount, 1, 500),
                    seed = seed,
                    generationVersion = generationVersion
                };

                var output = LevelBatchGenerator.Generate(config);
                Save(output, config);

                lastReport =
                    $"Generated={output.Report.GeneratedCandidates}\n" +
                    $"Unique={output.Report.UniqueCandidates}\n" +
                    $"Duplicate Rate={output.Report.DuplicateRate:P1}\n" +
                    $"Variety Rejections={output.Report.SimilarCandidatesRejected}\n" +
                    $"Selected={output.Report.SelectedLevels}\n" +
                    $"Hard Challenges={output.Report.HardChallenges}\n" +
                    $"Elapsed={output.Report.Elapsed.TotalSeconds:0.00}s\n" +
                    string.Join("\n", output.Warnings.ToArray());

                Debug.Log("Phase 5 level generation complete.\n" + lastReport);
            }

            EditorGUILayout.Space(8);
            EditorGUILayout.HelpBox(lastReport, MessageType.Info);
            if (GUILayout.Button("Open Level Preview")) LevelPreviewWindow.Open();
            if (GUILayout.Button("Run 500-Level Validation")) LevelValidationWindow.RunValidation();
        }

        private static void Save(LevelGenerationOutput output, LevelGenerationConfig config)
        {
            const string directory = "Assets/Resources/Levels";
            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);

            var levelPath = directory + "/LevelDatabase.asset";
            var puzzlePath = directory + "/PuzzleDatabase.asset";

            var levelAsset = AssetDatabase.LoadAssetAtPath<LevelDatabaseAsset>(levelPath);
            if (levelAsset == null)
            {
                levelAsset = CreateInstance<LevelDatabaseAsset>();
                AssetDatabase.CreateAsset(levelAsset, levelPath);
            }

            var puzzleAsset = AssetDatabase.LoadAssetAtPath<PuzzleDatabaseAsset>(puzzlePath);
            if (puzzleAsset == null)
            {
                puzzleAsset = CreateInstance<PuzzleDatabaseAsset>();
                AssetDatabase.CreateAsset(puzzleAsset, puzzlePath);
            }

            Undo.RecordObject(levelAsset, "Generate Level Database");
            Undo.RecordObject(puzzleAsset, "Generate Puzzle Database");

            levelAsset.databaseVersion = 1;
            levelAsset.generationVersion = config.generationVersion;
            levelAsset.levels = output.LevelDatabase.levels;

            puzzleAsset.databaseVersion = 1;
            puzzleAsset.generationVersion = config.generationVersion;
            puzzleAsset.puzzles = output.PuzzlePool;

            EditorUtility.SetDirty(levelAsset);
            EditorUtility.SetDirty(puzzleAsset);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            File.WriteAllText(
                directory + "/LevelDatabase.json",
                JsonUtility.ToJson(output.LevelDatabase, true));
            File.WriteAllText(
                directory + "/PuzzleDatabase.json",
                JsonUtility.ToJson(output.PuzzlePool == null
                    ? new PuzzleDatabase()
                    : new PuzzleDatabase
                    {
                        databaseVersion = 1,
                        generationVersion = config.generationVersion,
                        puzzles = output.PuzzlePool
                    }, false));
            AssetDatabase.Refresh();
        }
    }
}
#endif