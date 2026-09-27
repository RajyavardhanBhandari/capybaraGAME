using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace CapybaraGame.UI
{
    public sealed class BoardCellView : MonoBehaviour, IPointerClickHandler
    {
        private int row;
        private int column;
        private Image background;
        private Text mark;
        private Outline outline;
        private GameObject brokenHeart;
        private Text brokenHeartLeft;
        private Text brokenHeartRight;
        private readonly Image[] edges = new Image[4];
        private Action<int, int> clicked;
        private Action<int, int> doubleClicked;
        private CharacterFaceView face;

        public void Initialize(int r, int c, Action<int, int> onClick, Action<int, int> onDoubleClick)
        {
            row = r;
            column = c;
            clicked = onClick;
            doubleClicked = onDoubleClick;
            background = gameObject.GetComponent<Image>() ?? gameObject.AddComponent<Image>();
            outline = gameObject.GetComponent<Outline>() ?? gameObject.AddComponent<Outline>();
            outline.effectDistance = new Vector2(1f, -1f);
            outline.effectColor = new Color(0.18f, 0.15f, 0.14f, 0.08f);

            var faceObject = new GameObject("AnimalFace", typeof(RectTransform));
            faceObject.transform.SetParent(transform, false);
            var faceRect = faceObject.GetComponent<RectTransform>();
            faceRect.anchorMin = Vector2.zero;
            faceRect.anchorMax = Vector2.one;
            faceRect.offsetMin = faceRect.offsetMax = Vector2.zero;
            face = faceObject.AddComponent<CharacterFaceView>();
            face.Build(CharacterId.Capybara, false);
            face.SetVisible(false);

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
            mark.fontSize = 30;
            mark.color = Color.white;
            mark.fontStyle = FontStyle.Bold;
            mark.raycastTarget = false;
            CreateBrokenHeartEffect();

            CreateEdge("Top", 0, new Vector2(0, 1), new Vector2(1, 1), new Vector2(0, -2), new Vector2(0, 0));
            CreateEdge("Right", 1, new Vector2(1, 0), new Vector2(1, 1), new Vector2(-2, 0), new Vector2(0, 0));
            CreateEdge("Bottom", 2, new Vector2(0, 0), new Vector2(1, 0), new Vector2(0, 0), new Vector2(0, 2));
            CreateEdge("Left", 3, new Vector2(0, 0), new Vector2(0, 1), new Vector2(0, 0), new Vector2(2, 0));
        }


        private void CreateBrokenHeartEffect()
        {
            brokenHeart = new GameObject("BrokenHeartEffect", typeof(RectTransform));
            brokenHeart.transform.SetParent(transform, false);
            var root = brokenHeart.GetComponent<RectTransform>();
            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = root.offsetMax = Vector2.zero;

            brokenHeartLeft = CreateHeartPart("HeartLeft", root);
            brokenHeartRight = CreateHeartPart("HeartRight", root);
            brokenHeart.SetActive(false);
        }

        private Text CreateHeartPart(string name, RectTransform parent)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Text));
            go.transform.SetParent(parent, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = new Vector2(.5f, .5f);
            r.pivot = new Vector2(.5f, .5f);
            r.sizeDelta = new Vector2(70f, 70f);
            var t = go.GetComponent<Text>();
            t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text = "♥";
            t.fontSize = 52;
            t.fontStyle = FontStyle.Bold;
            t.alignment = TextAnchor.MiddleCenter;
            t.color = new Color(.95f, .22f, .25f, 0f);
            t.raycastTarget = false;
            return t;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData == null) return;
            if (eventData.clickCount >= 2) doubleClicked?.Invoke(row, column);
            else clicked?.Invoke(row, column);
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

        public void Refresh(PuzzleDefinition puzzle, PuzzleState state, CharacterId active, Color regionColor, bool selected, bool feedbackError, bool reducedMotion)
        {
            background.color = regionColor;
            int index = row * puzzle.columns + column;
            bool occupied = state.placed[index] != -1;
            bool marked = state.marks != null && state.marks[index];
            face.Configure(active, reducedMotion);
            face.SetVisible(occupied);
            mark.text = marked && !occupied ? "×" : string.Empty;
            mark.fontSize = Mathf.Max(28, 120 / puzzle.rows);
            mark.color = feedbackError ? new Color(.94f,.18f,.18f,1f) : Color.white;
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

        public void PlayBrokenHeart(bool reducedMotion)
        {
            if (reducedMotion || brokenHeart == null) return;
            StartCoroutine(BrokenHeartRoutine());
        }


        private System.Collections.IEnumerator BrokenHeartRoutine()
        {
            brokenHeart.SetActive(true);
            var left = brokenHeartLeft.rectTransform;
            var right = brokenHeartRight.rectTransform;
            left.anchoredPosition = new Vector2(-2f, -2f);
            right.anchoredPosition = new Vector2(2f, -2f);
            left.localRotation = Quaternion.Euler(0f, 0f, 0f);
            right.localRotation = Quaternion.Euler(0f, 0f, 0f);
            brokenHeartLeft.color = new Color(.95f,.22f,.25f,0f);
            brokenHeartRight.color = new Color(.95f,.22f,.25f,0f);

            float t = 0f;
            while (t < .42f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / .42f);
                float ease = 1f - Mathf.Pow(1f - p, 3f);
                brokenHeartLeft.color = new Color(.95f,.22f,.25f,1f - p);
                brokenHeartRight.color = new Color(.95f,.22f,.25f,1f - p);
                left.anchoredPosition = new Vector2(Mathf.Lerp(-2f,-20f,ease), Mathf.Lerp(-2f,16f,ease));
                right.anchoredPosition = new Vector2(Mathf.Lerp(2f,20f,ease), Mathf.Lerp(-2f,16f,ease));
                left.localRotation = Quaternion.Euler(0f,0f,Mathf.Lerp(0f, -16f, ease));
                right.localRotation = Quaternion.Euler(0f,0f,Mathf.Lerp(0f, 16f, ease));
                yield return null;
            }
            brokenHeart.SetActive(false);
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
        private PuzzleState lastState;
        private Color[] regionColors;
        private CharacterId activeCharacter;
        private int selectedIndex = -1;
        private bool reducedMotion;
        private Action<int, int> onTap;
        private Action<int, int> onDoubleTap;

        public void Build(PuzzleDefinition definition, Color[] colors, CharacterId character, bool reduced, Action<int, int> tap, Action<int, int> doubleTap)
        {
            puzzle = definition;
            lastState = null;
            regionColors = colors;
            activeCharacter = character;
            reducedMotion = reduced;
            onTap = tap;
            onDoubleTap = doubleTap;
            rect = GetComponent<RectTransform>() ?? gameObject.AddComponent<RectTransform>();
            grid = GetComponent<GridLayoutGroup>() ?? gameObject.AddComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
            grid.constraintCount = puzzle.columns;
            grid.spacing = new Vector2(3f, 3f);
            grid.padding = new RectOffset(2, 2, 2, 2);
            grid.childAlignment = TextAnchor.MiddleCenter;
            RebuildCells();
            Canvas.ForceUpdateCanvases();
            LayoutCells();
        }

        private void RebuildCells()
        {
            foreach (Transform child in transform) Destroy(child.gameObject);
            cells.Clear();
            for (int r = 0; r < puzzle.rows; r++)
            {
                for (int c = 0; c < puzzle.columns; c++)
                {
                    var cellObject = new GameObject($"Cell_{r}_{c}", typeof(RectTransform), typeof(Image));
                    cellObject.transform.SetParent(transform, false);
                    var view = cellObject.AddComponent<BoardCellView>();
                    view.Initialize(r, c, onTap, onDoubleTap);
                    cells.Add(view);
                }
            }
            grid.enabled = false;
            UpdateCellSize();
            LayoutCells();
        }

        public void Refresh(PuzzleState state)
        {
            lastState = state;
            for (int i = 0; i < cells.Count; i++)
            {
                int r = i / puzzle.columns;
                int c = i % puzzle.columns;
                cells[i].Refresh(puzzle, state, activeCharacter, regionColors[puzzle.RegionAt(r, c) % regionColors.Length], i == selectedIndex, false, reducedMotion);
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
            cells[index].PlayBrokenHeart(reducedMotion);
            StartCoroutine(ErrorMarkRoutine(index));
        }

        private System.Collections.IEnumerator ErrorMarkRoutine(int index)
        {
            float t = 0f;
            while (t < .55f)
            {
                t += Time.unscaledDeltaTime;
                for (int i = 0; i < cells.Count; i++)
                {
                    if (i == index && lastState != null)
                    {
                        int r = i / puzzle.columns;
                        int col = i % puzzle.columns;
                        cells[i].Refresh(puzzle, lastState, activeCharacter, regionColors[puzzle.RegionAt(r,col) % regionColors.Length], false, true, reducedMotion);
                    }
                }
                yield return null;
            }
            if (lastState != null) Refresh(lastState);
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