#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using CapybaraGame.Core;
using CapybaraGame.Levels;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Editor
{
    public sealed class LevelPreviewWindow : EditorWindow
    {
        private int level = 1;
        private PuzzleDefinition puzzle;

        public static void Open()
        {
            var window = GetWindow<LevelPreviewWindow>("Level Preview");
            window.Load();
        }

        [MenuItem("Capybara/Level System/Level Preview")]
        private static void MenuOpen() => Open();

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Phase 5 — Level Preview", EditorStyles.boldLabel);
            level = EditorGUILayout.IntSlider("Level", level, 1, LevelCatalog.MaxLevel);

            if (GUILayout.Button("Load Level")) Load();
            if (puzzle == null) return;

            var definition = LevelCatalog.Get(level);
            EditorGUILayout.LabelField("Puzzle ID", puzzle.id);
            EditorGUILayout.LabelField("Seed", puzzle.seed.ToString());
            EditorGUILayout.LabelField("Generation", puzzle.generatorVersion);
            EditorGUILayout.LabelField("Grid", puzzle.rows + "×" + puzzle.columns);
            EditorGUILayout.LabelField("Difficulty", puzzle.difficulty.ToString("0.0") + " / " + puzzle.difficultyBand);
            EditorGUILayout.LabelField("Style", puzzle.category);
            EditorGUILayout.LabelField("Hard Challenge", definition.IsHardChallenge ? "YES #" + definition.HardChallengeIndex : "NO");
            EditorGUILayout.LabelField("Fingerprint", puzzle.fingerprint);
            EditorGUILayout.LabelField("Solutions", puzzle.solutionCount.ToString());

            EditorGUILayout.Space(8);
            DrawBoard(puzzle);
            var solved = PuzzleSolver.Solve(puzzle, 2);
            EditorGUILayout.LabelField(
                $"Solver: nodes={solved.Metrics.NodesVisited}, candidates={solved.Metrics.CandidateChecks}, " +
                $"forced={solved.Metrics.ForcedMoves}, depth={solved.Metrics.MaximumDepth}, " +
                $"backtracks={solved.Metrics.Backtracks}");
        }

        private void Load()
        {
            try
            {
                puzzle = ProductionPuzzleRepository.Get(level);
            }
            catch (System.Exception e)
            {
                puzzle = null;
                Debug.LogException(e);
            }
        }

        private void DrawBoard(PuzzleDefinition p)
        {
            float size = Mathf.Min(position.width - 20f, 520f);
            float cell = size / p.columns;
            var rect = GUILayoutUtility.GetRect(size, size);
            for (int r = 0; r < p.rows; r++)
                for (int c = 0; c < p.columns; c++)
                {
                    var cellRect = new Rect(rect.x + c * cell, rect.y + r * cell, cell, cell);
                    var hue = (p.RegionAt(r, c) * 0.071f) % 1f;
                    EditorGUI.DrawRect(cellRect, Color.HSVToRGB(hue, .18f, .96f));
                    Handles.color = new Color(.25f, .25f, .25f, .35f);
                    Handles.DrawLine(new Vector3(cellRect.x, cellRect.y), new Vector3(cellRect.xMax, cellRect.y));
                    Handles.DrawLine(new Vector3(cellRect.x, cellRect.y), new Vector3(cellRect.x, cellRect.yMax));
                    if (p.solution[r] == c)
                        EditorGUI.LabelField(cellRect, "●", new GUIStyle(EditorStyles.boldLabel) { alignment = TextAnchor.MiddleCenter });
                }
        }
    }
}
#endif