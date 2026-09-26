using CapybaraGame.Audio;
using CapybaraGame.Characters;
using CapybaraGame.Core;
using CapybaraGame.Gameplay;
using CapybaraGame.Levels;
using CapybaraGame.Progression;
using CapybaraGame.Platform;
using CapybaraGame.Services;
using CapybaraGame.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CapybaraGame
{
    public static class GameBootstrap
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (Object.FindFirstObjectByType<GameRoot>() != null) return;
            var root = new GameObject("GameRoot");
            root.AddComponent<GameRoot>();
        }
    }

    public sealed partial class GameRoot : MonoBehaviour
    {
        private LocalSave save;
        private CharacterSystem characters;
        private GameplayController gameplay;
        private AudioService audioService;
        private HapticService haptics;
        private BoardView board;
        private Canvas canvas;
        private Font font;
        private readonly System.Collections.Generic.List<GameObject> spawned = new System.Collections.Generic.List<GameObject>();
        private int levelSelectPage;
        private readonly Color background = new Color(.965f, .945f, .925f);
        private readonly Color card = new Color(1f, .985f, .965f);
        private readonly Color text = new Color(.18f, .15f, .14f);
        private readonly Color[] regionColors =
        {
            new Color(.48f,.73f,.90f), new Color(.55f,.78f,.48f), new Color(.96f,.67f,.36f),
            new Color(.82f,.55f,.72f), new Color(.96f,.80f,.45f), new Color(.55f,.78f,.70f),
            new Color(.69f,.61f,.86f), new Color(.92f,.55f,.55f), new Color(.58f,.72f,.86f),
            new Color(.72f,.72f,.72f)
        };

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            save = SaveService.Load();
            characters = new CharacterSystem(save);
            gameplay = new GameplayController();
            gameplay.StateChanged += RefreshGameplay;
            gameplay.EventRaised += OnGameplayEvent;
            gameplay.Completed += OnCompleted;
            gameplay.Failed += OnFailed;

            audioService = gameObject.AddComponent<AudioService>();
            audioService.Initialize(save);
            haptics = new HapticService();
            haptics.Initialize(save);

            BuildCanvas();
            ShowHome();
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = .5f;

            font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (EventSystem.current == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
                DontDestroyOnLoad(es);
            }
        }

        private void Clear()
        {
            foreach (var go in spawned) if (go) Destroy(go);
            spawned.Clear();
            board = null;
        }

        private GameObject Panel(Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject("Panel", typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = color;
            spawned.Add(go);
            return go;
        }

        private Text Label(Transform parent, string value, int size, TextAnchor alignment, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject("Text", typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            var t = go.GetComponent<Text>();
            t.font = font; t.text = value; t.fontSize = size; t.alignment = alignment;
            t.color = color; t.horizontalOverflow = HorizontalWrapMode.Wrap;
            spawned.Add(go);
            return t;
        }

        private Button Button(Transform parent, string title, int size, Color fill, UnityEngine.Events.UnityAction action, Vector2 min, Vector2 max)
        {
            var go = new GameObject("Button", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            go.GetComponent<Image>().color = fill;
            var button = go.GetComponent<Button>();
            if (action != null) button.onClick.AddListener(action);

            var textObject = new GameObject("ButtonText", typeof(Text));
            textObject.transform.SetParent(go.transform, false);
            var tr = textObject.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(12, 0); tr.offsetMax = new Vector2(-12, 0);
            var t = textObject.GetComponent<Text>();
            t.font = font; t.text = title; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter; t.color = text;
            t.raycastTarget = false;
            spawned.Add(go);
            return button;
        }

        private void ShowHome()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, "CAPYBARA", 64, TextAnchor.MiddleCenter, text, new Vector2(.08f,.86f), new Vector2(.92f,.97f));
            Label(bg.transform, "A tiny puzzle. A big little world.", 28, TextAnchor.MiddleCenter, text, new Vector2(.08f,.80f), new Vector2(.92f,.86f));
            Label(bg.transform, $"LEVEL {save.unlockedLevel}   •   COINS {save.coins}", 30, TextAnchor.MiddleCenter, text, new Vector2(.08f,.72f), new Vector2(.92f,.79f));

            Button(bg.transform, "PLAY", 44, new Color(.52f,.78f,.47f),
                () => StartLevel(save.unlockedLevel), new Vector2(.08f,.58f), new Vector2(.92f,.68f));
            Button(bg.transform, "LEVEL SELECT", 28, card, () => { levelSelectPage = Mathf.Max(0, (save.unlockedLevel - 1) / 40); ShowLevelSelect(); }, new Vector2(.08f,.41f), new Vector2(.92f,.47f));

            Button(bg.transform, "RULES", 26, card, ShowRules, new Vector2(.08f,.48f), new Vector2(.44f,.55f));
            Button(bg.transform, "SETTINGS", 26, card, ShowSettings, new Vector2(.56f,.48f), new Vector2(.92f,.55f));
            Label(bg.transform, "Progression is local and works offline.", 22, TextAnchor.MiddleCenter, text, new Vector2(.08f,.33f), new Vector2(.92f,.39f));
            Label(bg.transform, $"Treats: {save.treats}", 28, TextAnchor.MiddleCenter, text, new Vector2(.08f,.25f), new Vector2(.92f,.32f));
        }

        private void ShowLevelSelect()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, "LEVEL SELECT", 50, TextAnchor.MiddleCenter, text, new Vector2(.06f,.88f), new Vector2(.94f,.96f));
            Label(bg.transform, $"COMPLETED  {ProgressionModel.GetCompletedCount(save)} / {LevelCatalog.MaxLevel}    PAGE {levelSelectPage + 1}/{pageCount}", 22, TextAnchor.MiddleCenter, text, new Vector2(.04f,.83f), new Vector2(.96f,.88f));

            const int columns = 4;
            const int pageSize = 40;
            int pageCount = (LevelCatalog.MaxLevel + pageSize - 1) / pageSize;
            levelSelectPage = Mathf.Clamp(levelSelectPage, 0, pageCount - 1);
            int firstLevel = levelSelectPage * pageSize + 1;
            for (int i = 0; i < pageSize; i++)
            {
                int level = firstLevel + i;
                if (level > LevelCatalog.MaxLevel) break;
                int row = i / columns;
                int col = i % columns;
                float x0 = .06f + col * .225f;
                float x1 = x0 + .205f;
                float y1 = .76f - row * .075f;
                float y0 = y1 - .062f;
                bool unlocked = ProgressionModel.IsUnlocked(save, level);
                bool completed = ProgressionModel.IsCompleted(save, level);
                string label = completed ? "✓ " + level : unlocked ? level.ToString() : "🔒";
                Color fill = completed ? new Color(.55f,.78f,.58f) : unlocked ? card : new Color(.88f,.87f,.84f);
                Button(bg.transform, label, 24, fill, unlocked ? (UnityEngine.Events.UnityAction)(() => StartLevel(level)) : null,
                    new Vector2(x0,y0), new Vector2(x1,y1));
            }
            if (levelSelectPage > 0) Button(bg.transform, "‹ PREVIOUS", 22, card, () => { levelSelectPage--; ShowLevelSelect(); }, new Vector2(.06f,.07f), new Vector2(.30f,.14f));
            if (levelSelectPage < pageCount - 1) Button(bg.transform, "NEXT ›", 22, card, () => { levelSelectPage++; ShowLevelSelect(); }, new Vector2(.70f,.07f), new Vector2(.94f,.14f));
            Button(bg.transform, "HOME", 22, card, ShowHome, new Vector2(.36f,.07f), new Vector2(.64f,.14f));
        }

        private void ShowRules()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, "HOW TO PLAY", 50, TextAnchor.MiddleCenter, text, new Vector2(.08f,.82f), new Vector2(.92f,.92f));
            Label(bg.transform, "1. One character per colored region\n\n2. One character per row\n\n3. One character per column\n\n4. Characters cannot touch, even diagonally", 32, TextAnchor.MiddleLeft, text, new Vector2(.10f,.35f), new Vector2(.90f,.78f));
            Button(bg.transform, "BACK", 30, card, ShowHome, new Vector2(.18f,.12f), new Vector2(.82f,.20f));
        }

        private void ShowSettings()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, "SETTINGS", 50, TextAnchor.MiddleCenter, text, new Vector2(.08f,.82f), new Vector2(.92f,.92f));
            Button(bg.transform, $"SOUND: {(save.sound ? "ON" : "OFF")}", 30, card, () => { save.sound = !save.sound; audioService.SetEnabled(save.sound); SaveService.Save(save); ShowSettings(); }, new Vector2(.12f,.62f), new Vector2(.88f,.70f));
            Button(bg.transform, $"HAPTICS: {(save.haptics ? "ON" : "OFF")}", 30, card, () => { save.haptics = !save.haptics; haptics.SetEnabled(save.haptics); SaveService.Save(save); ShowSettings(); }, new Vector2(.12f,.51f), new Vector2(.88f,.59f));
            Button(bg.transform, $"REDUCED MOTION: {(save.reducedMotion ? "ON" : "OFF")}", 30, card, () => { save.reducedMotion = !save.reducedMotion; SaveService.Save(save); ShowSettings(); }, new Vector2(.12f,.40f), new Vector2(.88f,.48f));
            Button(bg.transform, "BACK", 30, card, ShowHome, new Vector2(.18f,.12f), new Vector2(.82f,.20f));
        }

        private void StartLevel(int level)
        {
            if (!ProgressionService.IsUnlocked(save, level)) level = save.unlockedLevel;
            gameplay.LoadLevel(level, characters.Active);
            BuildGameplay();
        }

        private void BuildGameplay()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            var puzzle = gameplay.Puzzle;

            Button(bg.transform, "←", 32, card, ShowLevelSelect, new Vector2(.05f,.92f), new Vector2(.16f,.98f));
            Label(bg.transform, $"LEVEL {gameplay.LevelId}", 38, TextAnchor.MiddleCenter, text, new Vector2(.20f,.92f), new Vector2(.80f,.98f));
            Button(bg.transform, "Ⅱ", 28, card, PauseGame, new Vector2(.84f,.92f), new Vector2(.95f,.98f));

            Label(bg.transform, $"LIVES  {LifeText(gameplay.State.livesRemaining)}", 27, TextAnchor.MiddleCenter, text, new Vector2(.06f,.84f), new Vector2(.48f,.90f));
            Label(bg.transform, $"{CharacterCatalog.Name(gameplay.ActiveCharacter)}  •  {puzzle.difficultyBand}", 23, TextAnchor.MiddleCenter, text, new Vector2(.50f,.84f), new Vector2(.94f,.90f));

            var boardObject = new GameObject("Board", typeof(RectTransform));
            boardObject.transform.SetParent(bg.transform, false);
            var br = boardObject.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(.06f,.28f); br.anchorMax = new Vector2(.94f,.82f);
            br.offsetMin = br.offsetMax = Vector2.zero;
            board = boardObject.AddComponent<BoardView>();
            board.Build(puzzle, regionColors, gameplay.ActiveCharacter, save.reducedMotion, OnCellTapped);
            board.Refresh(gameplay.State);

            Button(bg.transform, "HINT", 24, card, () => UseHint(), new Vector2(.06f,.18f), new Vector2(.30f,.24f));
            Button(bg.transform, "REVEAL", 24, card, () => UseReveal(), new Vector2(.35f,.18f), new Vector2(.65f,.24f));
            Button(bg.transform, "EXTRA LIFE", 22, card, () => UseExtraLife(), new Vector2(.70f,.18f), new Vector2(.94f,.24f));
            Label(bg.transform, $"COINS  {save.coins}", 22, TextAnchor.MiddleCenter, text, new Vector2(.08f,.08f), new Vector2(.92f,.13f));
        }

        private static string LifeText(int lives)
        {
            lives = Mathf.Clamp(lives, 0, 3);
            return new string('♥', lives) + new string('♡', 3 - lives);
        }

        private void OnGameplayEvent(GameplayEvent e)
        {
            if (e.Type == GameplayEventType.MoveCorrect)
            {
                audioService.Play(SfxType.Placement);
                haptics.Play(HapticType.Light);
                if (board != null && e.Row >= 0) board.PlayPlacementFeedback(e.Row, e.Column);
            }
            else if (e.Type == GameplayEventType.CharacterRemoved)
            {
                audioService.Play(SfxType.Removal);
                haptics.Play(HapticType.Tiny);
            }
            else if (e.Type == GameplayEventType.MoveIncorrect)
            {
                audioService.Play(SfxType.Invalid);
                haptics.Play(HapticType.Medium);
                if (board != null && e.Row >= 0) board.PlayErrorFeedback(e.Row, e.Column);
            }
            else if (e.Type == GameplayEventType.LifeLost)
            {
                // The invalid-move cue already communicates the mistake; this event remains available for future UI analytics.
            }
        }

        private void OnCellTapped(int row, int column)
        {
            if (gameplay.CurrentState != GameplayState.Playing) return;
            gameplay.TapCell(row, column);
            if (gameplay.CurrentState == GameplayState.Playing) RefreshGameplay();
        }

        private void RefreshGameplay()
        {
            if (board == null || gameplay.State == null) return;
            board.Refresh(gameplay.State);
            var livesLabel = FindObjectsOfType<Text>();
            foreach (var t in livesLabel)
            {
                if (t.text != null && t.text.StartsWith("LIVES  "))
                    t.text = $"LIVES  {LifeText(gameplay.State.livesRemaining)}";
            }
        }

        private void OnCompleted(RewardResult reward)
        {
            audioService.Play(SfxType.Completion);
            haptics.Play(HapticType.Celebration);
            ProgressionModel.ApplyCompletion(save, gameplay.LevelId, reward);
            gameplay.GrantReward();
            ShowResult(true, reward);
        }

        private void OnFailed()
        {
            audioService.Play(SfxType.LifeLost);
            haptics.Play(HapticType.Medium);
            ShowResult(false, RewardCalculator.CalculateFailure());
        }

        private void ShowResult(bool solved, RewardResult reward)
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, solved ? "🎉 COMPLETE" : "PUZZLE FAILED", 52, TextAnchor.MiddleCenter, text, new Vector2(.08f,.68f), new Vector2(.92f,.80f));
            Label(bg.transform, solved
                ? $"Remaining lives: {gameplay.State.livesRemaining}\n🍓 +{reward.Treats} Treats\n🪙 +{reward.Coins} Coins"
                : "🍓 +0 Treats\n🪙 +0 Coins", 34, TextAnchor.MiddleCenter, text, new Vector2(.10f,.48f), new Vector2(.90f,.63f));

            if (solved)
            {
                Button(bg.transform, gameplay.LevelId < 500 ? "CONTINUE" : "HOME", 36, new Color(.52f,.78f,.47f),
                    () => { gameplay.Advance(); int next = LevelCatalog.NextLevel(gameplay.LevelId); if (next > 0 && ProgressionModel.IsUnlocked(save, next)) StartLevel(next); else ShowLevelSelect(); },
                    new Vector2(.12f,.30f), new Vector2(.88f,.39f));
            }
            else
            {
                Button(bg.transform, "RETRY", 34, new Color(.52f,.78f,.47f),
                    () => StartLevel(gameplay.LevelId), new Vector2(.12f,.31f), new Vector2(.88f,.39f));
                Button(bg.transform, "HOME", 28, card, ShowHome, new Vector2(.25f,.20f), new Vector2(.75f,.27f));
            }
        }

        private void PauseGame()
        {
            gameplay.Pause();
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, "PAUSED", 52, TextAnchor.MiddleCenter, text, new Vector2(.08f,.70f), new Vector2(.92f,.80f));
            Button(bg.transform, "RESUME", 34, new Color(.52f,.78f,.47f), () => { gameplay.Resume(); BuildGameplay(); }, new Vector2(.12f,.52f), new Vector2(.88f,.61f));
            Button(bg.transform, "RESTART", 30, card, () => { gameplay.Restart(); BuildGameplay(); }, new Vector2(.12f,.41f), new Vector2(.88f,.49f));
            Button(bg.transform, "EXIT", 30, card, () => { gameplay.Exit(); ShowHome(); }, new Vector2(.12f,.30f), new Vector2(.88f,.38f));
        }

        private void UseHint()
        {
            const int cost = 100;
            if (save.coins < cost) return;
            if (!gameplay.TryGetSolutionCell(out var row, out var col)) return;
            save.coins -= cost;
            SaveService.Save(save);
            board.SetSelected(row, col);
            board.Refresh(gameplay.State);
        }

        private void UseReveal()
        {
            const int cost = 175;
            if (save.coins < cost) return;
            if (!gameplay.TryGetSolutionCell(out var row, out var col)) return;
            save.coins -= cost;
            SaveService.Save(save);
            gameplay.TapCell(row, col);
            if (gameplay.CurrentState == GameplayState.Playing) RefreshGameplay();
        }

        private void UseExtraLife()
        {
            const int cost = 250;
            if (save.coins < cost) return;
            save.coins -= cost;
            gameplay.State.livesRemaining = Mathf.Min(GameplayController.MaxLives, gameplay.State.livesRemaining + 1);
            SaveService.Save(save);
            RefreshGameplay();
        }
    }
}