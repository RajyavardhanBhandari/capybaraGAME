using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using UnityEngine;
using UnityEngine.UI;

namespace CapybaraGame.UI
{
    public sealed class BoardCellView : MonoBehaviour
    {
        private int row;
        private int column;
        private Image background;
        private Text mark;
        private Outline outline;
        private readonly Image[] edges = new Image[4];
        private Button button;
        private Action<int, int> clicked;

        public void Initialize(int r, int c, Action<int, int> onClick)
        {
            row = r;
            column = c;
            clicked = onClick;
            background = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            button = gameObject.GetComponent<Button>() ?? gameObject.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => clicked?.Invoke(row, column));

            outline = gameObject.GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1f, -1f);
            outline.effectColor = new Color(0.18f, 0.15f, 0.14f, 0.08f);

            var labelObject = new GameObject("Character", typeof(Text));
            labelObject.transform.SetParent(transform, false);
            var labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            mark = labelObject.GetComponent<Text>();
            mark.alignment = TextAnchor.MiddleCenter;
            mark.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            mark.fontSize = 26;
            mark.color = Color.white;
            mark.raycastTarget = false;

            CreateEdge("Top", 0, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -2), new Vector2(0, 0));
            CreateEdge("Right", 1, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-2, 0), new Vector2(0, 0));
            CreateEdge("Bottom", 2, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 2));
            CreateEdge("Left", 3, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(2, 0));
        }

        private void CreateEdge(string name, int index, Vector2 min, Vector2 max, Vector2 offsetMin, Vector2 offsetMax)
        {
            var edgeObject = new GameObject(name, typeof(Image));
            edgeObject.transform.SetParent(transform, false);
            var rect = edgeObject.GetComponent<RectTransform>();
            rect.anchorMin = min;
            rect.anchorMax = max;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
            var image = edgeObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.15f, 0.14f, 0.35f);
            image.raycastTarget = false;
            edges[index] = image;
        }

        public void Refresh(PuzzleDefinition puzzle, PuzzleState state, CharacterId active, Color regionColor, bool selected, bool feedbackError)
        {
            background.color = regionColor;
            int index = row * puzzle.columns + column;
            bool occupied = state.placed[index] != -1;
            mark.text = occupied ? CharacterCatalog.Symbol(active) : string.Empty;
            mark.fontSize = occupied ? Mathf.Max(18, 150 / puzzle.rows) : 26;
            outline.effectColor = selected
                ? new Color(0.10f, 0.10f, 0.10f, 0.70f)
                : feedbackError
                    ? new Color(0.85f, 0.25f, 0.20f, 0.85f)
                    : new Color(0.18f, 0.15f, 0.14f, 0.08f);
            outline.effectDistance = selected ? new Vector2(3f, -3f) : new Vector2(1f, -1f);
            RefreshEdges(puzzle);
        }

        private void RefreshEdges(PuzzleDefinition puzzle)
        {
            var region = puzzle.RegionAt(row, column);
            edges[0].gameObject.SetActive(row == 0 || puzzle.RegionAt(row - 1, column) != region);
            edges[1].gameObject.SetActive(column == puzzle.columns - 1 || puzzle.RegionAt(row, column + 1) != region);
            edges[2].gameObject.SetActive(row == puzzle.rows - 1 || puzzle.RegionAt(row + 1, column) != region);
            edges[3].gameObject.SetActive(column == 0 || puzzle.RegionAt(row, column - 1) != region);
        }

        public void Pulse(bool reducedMotion)
        {
            if (!reducedMotion) StartCoroutine(PulseRoutine());
        }

        public void Shake(bool reducedMotion)
        {
            if (!reducedMotion) StartCoroutine(ShakeRoutine());
        }

        private System.Collections.IEnumerator PulseRoutine()
        {
            var rect = transform as RectTransform;
            if (rect == null) yield break;
            rect.localScale = Vector3.one * 0.82f;
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / 0.18f);
                rect.localScale = Vector3.one * Mathf.Lerp(0.82f, 1.04f, p);
                yield return null;
            }
            rect.localScale = Vector3.one;
        }

        private System.Collections.IEnumerator ShakeRoutine()
        {
            var rect = transform as RectTransform;
            if (rect == null) yield break;
            var start = rect.anchoredPosition;
            float t = 0f;
            while (t < 0.18f)
            {
                t += Time.unscaledDeltaTime;
                float strength = 5f * (1f - t / 0.18f);
                rect.anchoredPosition = start + new Vector2(Mathf.Sin(t * 95f) * strength, 0f);
                yield return null;
            }
            rect.anchoredPosition = start;
        }
    }

    public sealed class BoardView : MonoBehaviour
    {
        private readonly List<BoardCellView> cells = new List<BoardCellView>();
        private GridLayoutGroup grid;
        private RectTransform rect;
        private PuzzleDefinition puzzle;
        private Color[] regionColors;
        private CharacterId activeCharacter;
        private int selectedIndex = -1;
        private bool reducedMotion;

        public void Build(PuzzleDefinition definition, Color[] colors, CharacterId character, bool reduced, Action<int, int> click)
        {
            puzzle = definition;
            regionColors = colors;
            activeCharacter = character;
            reducedMotion = reduced;
            rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            grid = GetComponent<GridLayoutGroup>() ?? gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = puzzle.columns;
            grid.spacing = new Vector2(3f, 3f);
            grid.padding = new RectOffset(2, 2, 2, 2);
            grid.childAlignment = TextAnchor.MiddleCenter;
            RebuildCells(click);
            Canvas.ForceUpdateCanvases();
            LayoutCells();
        }

        private void RebuildCells(Action<int, int> click)
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
            cells.Clear();
            for (int r = 0; r < puzzle.rows; r++)
            {
                for (int c = 0; c < puzzle.columns; c++)
                {
                    var cellObject = new GameObject($"Cell_{r}_{c}", typeof(RectTransform), typeof(Image), typeof(Button));
                    cellObject.transform.SetParent(transform, false);
                    var view = cellObject.AddComponent<BoardCellView>();
                    view.Initialize(r, c, click);
                    cells.Add(view);
                }
            }
            grid.enabled = false;
            UpdateCellSize();
            LayoutCells();
        }

        public void Refresh(PuzzleState state)
        {
            for (int i = 0; i < cells.Count; i++)
            {
                int r = i / puzzle.columns;
                int c = i % puzzle.columns;
                cells[i].Refresh(puzzle, state, activeCharacter, regionColors[puzzle.RegionAt(r, c) % regionColors.Length], i == selectedIndex, false);
            }
        }

        public void SetSelected(int row, int column)
        {
            selectedIndex = row * puzzle.columns + column;
        }

        public void PlayPlacementFeedback(int row, int column)
        {
            if (puzzle == null || row < 0 || column < 0 || row >= puzzle.rows || column >= puzzle.columns) return;
            int index = row * puzzle.columns + column;
            if (index < 0 || index >= cells.Count) return;
            cells[index].Pulse(reducedMotion);
        }

        public void PlayErrorFeedback(int row, int column)
        {
            if (puzzle == null || row < 0 || column < 0 || row >= puzzle.rows || column >= puzzle.columns) return;
            int index = row * puzzle.columns + column;
            if (index < 0 || index >= cells.Count) return;
            cells[index].Shake(reducedMotion);
        }

        private void UpdateCellSize()
        {
            if (rect == null || puzzle == null) return;
            float width = rect.rect.width;
            float height = rect.rect.height;
            if (width <= 1f || height <= 1f) return;
            float side = Mathf.Min(width, height);
            float cell = Mathf.Max(28f, (side - (puzzle.columns - 1) * 3f - 4f) / puzzle.columns);
            if (grid != null) grid.cellSize = new Vector2(cell, cell);
        }

        private void LayoutCells()
        {
            if (rect == null || puzzle == null || cells.Count != puzzle.CellCount) return;
            float width = rect.rect.width;
            float height = rect.rect.height;
            if (width <= 1f || height <= 1f) return;

            float side = Mathf.Min(width, height);
            float cell = Mathf.Max(28f, (side - (puzzle.columns - 1) * 3f - 4f) / puzzle.columns);
            float total = puzzle.columns * cell + (puzzle.columns - 1) * 3f;
            float startX = -total * 0.5f + cell * 0.5f;
            float startY = total * 0.5f - cell * 0.5f;

            for (int i = 0; i < cells.Count; i++)
            {
                var cellRect = cells[i].transform as RectTransform;
                if (cellRect == null) continue;
                int r = i / puzzle.columns;
                int col = i % puzzle.columns;
                cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                cellRect.pivot = new Vector2(0.5f, 0.5f);
                cellRect.sizeDelta = new Vector2(cell, cell);
                cellRect.anchoredPosition = new Vector2(startX + col * (cell + 3f), startY - r * (cell + 3f));
            }
        }

        private void LateUpdate()
        {
            if (cells.Count > 0) LayoutCells();
        }

        private void OnRectTransformDimensionsChange()
        {
            UpdateCellSize();
            LayoutCells();
        }
    }
}