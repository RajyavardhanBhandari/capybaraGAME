using System.Collections.Generic;
using CapybaraGame.Characters;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;
using CapybaraGame.Services;
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

    public sealed class GameRoot : MonoBehaviour
    {
        private LocalSave save;
        private CharacterSystem characters;
        private PuzzleDefinition puzzle;
        private PuzzleState state;
        private GameScreen screen = GameScreen.Home;

        private Canvas canvas;
        private Font font;
        private readonly List<GameObject> spawned = new();

        private readonly Color background = new(0.965f, 0.945f, 0.925f);
        private readonly Color card = new(1f, 0.985f, 0.965f);
        private readonly Color text = new(0.18f, 0.15f, 0.14f);
        private readonly Color[] regionColors =
        {
            new(0.48f,0.73f,0.90f), new(0.55f,0.78f,0.48f), new(0.96f,0.67f,0.36f),
            new(0.82f,0.55f,0.72f), new(0.96f,0.80f,0.45f), new(0.55f,0.78f,0.70f),
            new(0.69f,0.61f,0.86f), new(0.92f,0.55f,0.55f), new(0.58f,0.72f,0.86f),
            new(0.72f,0.72f,0.72f)
        };

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            save = SaveService.Load();
            characters = new CharacterSystem(save);
            BuildCanvas();
            ShowHome();
        }

        private void BuildCanvas()
        {
            var canvasObject = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform);
            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080, 1920);
            scaler.matchWidthOrHeight = 0.5f;

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
        }

        private GameObject Panel(Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject("Panel", typeof(Image));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            go.GetComponent<Image>().color = color;
            spawned.Add(go);
            return go;
        }

        private Text Label(Transform parent, string value, int size, TextAnchor alignment, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = new GameObject("Text", typeof(Text));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin; rect.anchorMax = anchorMax;
            rect.offsetMin = offsetMin; rect.offsetMax = offsetMax;
            var t = go.GetComponent<Text>();
            t.font = font; t.text = value; t.fontSize = size; t.alignment = alignment;
            t.color = color; t.horizontalOverflow = HorizontalWrapMode.Wrap;
            spawned.Add(go);
            return t;
        }

        private Button Button(Transform parent, string title, int size, Color fill, UnityEngine.Events.UnityAction action, float height = 92)
        {
            var go = new GameObject("Button", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, height);
            var image = go.GetComponent<Image>();
            image.color = fill;
            var b = go.GetComponent<Button>();
            b.onClick.AddListener(action);

            var textGo = new GameObject("ButtonText", typeof(Text));
            textGo.transform.SetParent(go.transform, false);
            var tr = textGo.GetComponent<RectTransform>();
            tr.anchorMin = Vector2.zero; tr.anchorMax = Vector2.one;
            tr.offsetMin = new Vector2(20, 0); tr.offsetMax = new Vector2(-20, 0);
            var t = textGo.GetComponent<Text>();
            t.font = font; t.text = title; t.fontSize = size; t.alignment = TextAnchor.MiddleCenter;
            t.color = text;
            spawned.Add(go);
            return b;
        }

        private void ShowHome()
        {
            Clear();
            screen = GameScreen.Home;

            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Label(bg.transform, "CAPYBARA", 64, TextAnchor.MiddleCenter, text, new Vector2(0,0.86f), new Vector2(1,0.98f), Vector2.zero, Vector2.zero);
            Label(bg.transform, "A tiny puzzle. A big little world.", 30, TextAnchor.MiddleCenter, new Color(.38f,.34f,.32f), new Vector2(.08f,.80f), new Vector2(.92f,.87f), Vector2.zero, Vector2.zero);

            var stats = Panel(bg.transform, card, new Vector2(.07f,.69f), new Vector2(.93f,.79f), Vector2.zero, Vector2.zero);
            Label(stats.transform, $"Level {save.unlockedLevel}     •     Coins {save.coins}", 34, TextAnchor.MiddleCenter, text, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);

            var play = Button(bg.transform, "PLAY", 44, new Color(.52f,.78f,.47f), () => StartLevel(save.unlockedLevel), 120);
            var pr = play.GetComponent<RectTransform>(); pr.anchorMin = new Vector2(.08f,.52f); pr.anchorMax = new Vector2(.92f,.62f); pr.offsetMin = pr.offsetMax = Vector2.zero;

            var daily = Button(bg.transform, "DAILY PUZZLE  •  Today's shared challenge", 30, new Color(.92f,.78f,.48f), ShowDaily, 96);
            var dr = daily.GetComponent<RectTransform>(); dr.anchorMin = new Vector2(.08f,.41f); dr.anchorMax = new Vector2(.92f,.49f); dr.offsetMin = dr.offsetMax = Vector2.zero;

            var row = new GameObject("Nav", typeof(HorizontalLayoutGroup));
            row.transform.SetParent(bg.transform, false);
            var rr = row.GetComponent<RectTransform>();
            rr.anchorMin = new Vector2(.06f,.08f); rr.anchorMax = new Vector2(.94f,.34f); rr.offsetMin = rr.offsetMax = Vector2.zero;
            var layout = row.GetComponent<HorizontalLayoutGroup>(); layout.spacing = 14; layout.childForceExpandWidth = true;
            AddNav(row.transform, "SHOP", ShowShop);
            AddNav(row.transform, "LEADERBOARD", ShowLeaderboard);
            AddNav(row.transform, "PROFILE", ShowProfile);
        }

        private void AddNav(Transform parent, string title, UnityEngine.Events.UnityAction action)
        {
            var b = Button(parent, title, 22, card, action, 90);
            b.GetComponent<LayoutElement>()?.gameObject.SetActive(true);
        }

        private void StartLevel(int level)
        {
            Clear();
            screen = GameScreen.Gameplay;
            puzzle = PuzzleRepository.Get(level);
            state = new PuzzleState(puzzle.id);
            state.status = PuzzleStatus.Playing;
            BuildGameplay(false);
        }

        private void BuildGameplay(bool daily)
        {
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Label(bg.transform, daily ? "DAILY PUZZLE" : $"LEVEL {puzzle.id.Replace("P","")}", 38, TextAnchor.MiddleCenter, text, new Vector2(.12f,.92f), new Vector2(.88f,.98f), Vector2.zero, Vector2.zero);
            Label(bg.transform, $"Lives  {state.livesRemaining}     •     {CharacterCatalog.Name(characters.Active)}     •     {puzzle.difficultyBand}", 25, TextAnchor.MiddleCenter, text, new Vector2(.04f,.85f), new Vector2(.96f,.92f), Vector2.zero, Vector2.zero);\n            Label(bg.transform, "1 per region  •  1 per row  •  1 per column  •  no touching", 20, TextAnchor.MiddleCenter, new Color(.38f,.34f,.32f), new Vector2(.05f,.80f), new Vector2(.95f,.85f), Vector2.zero, Vector2.zero);

            var board = new GameObject("Board", typeof(GridLayoutGroup));
            board.transform.SetParent(bg.transform, false);
            var br = board.GetComponent<RectTransform>();
            br.anchorMin = new Vector2(.055f,.31f); br.anchorMax = new Vector2(.945f,.84f); br.offsetMin = br.offsetMax = Vector2.zero;
            var grid = board.GetComponent<GridLayoutGroup>();
            grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount; grid.constraintCount = 10;
            grid.spacing = new Vector2(5,5); grid.padding = new RectOffset(5,5,5,5);
            grid.cellSize = new Vector2(82,82);
            grid.childAlignment = TextAnchor.MiddleCenter;

            for (int i = 0; i < 100; i++)
            {
                int row = i / 10, col = i % 10;
                CreateCell(board.transform, row, col);
            }

            var hint = Button(bg.transform, "HINT  •  100", 24, card, () => SpendHint(daily), 88);
            var hr = hint.GetComponent<RectTransform>(); hr.anchorMin = new Vector2(.06f,.19f); hr.anchorMax = new Vector2(.30f,.25f); hr.offsetMin = hr.offsetMax = Vector2.zero;
            var reveal = Button(bg.transform, "REVEAL  •  175", 24, card, () => RevealCell(daily), 88);
            var rr = reveal.GetComponent<RectTransform>(); rr.anchorMin = new Vector2(.38f,.19f); rr.anchorMax = new Vector2(.62f,.25f); rr.offsetMin = rr.offsetMax = Vector2.zero;
            var life = Button(bg.transform, "+ LIFE  •  250", 24, card, () => ExtraLife(daily), 88);
            var lr = life.GetComponent<RectTransform>(); lr.anchorMin = new Vector2(.70f,.19f); lr.anchorMax = new Vector2(.94f,.25f); lr.offsetMin = lr.offsetMax = Vector2.zero;

            var back = Button(bg.transform, "← HOME", 25, card, ShowHome, 74);
            var backR = back.GetComponent<RectTransform>(); backR.anchorMin = new Vector2(.08f,.06f); backR.anchorMax = new Vector2(.32f,.11f); backR.offsetMin = backR.offsetMax = Vector2.zero;
        }

        private void CreateCell(Transform parent, int row, int col)
        {
            var go = new GameObject($"Cell_{row}_{col}", typeof(Image), typeof(Button));
            go.transform.SetParent(parent, false);
            var image = go.GetComponent<Image>();
            image.color = regionColors[puzzle.RegionAt(row,col) % regionColors.Length];
            var button = go.GetComponent<Button>();
            button.onClick.AddListener(() => ClickCell(row, col));

            var label = new GameObject("Mark", typeof(Text));
            label.transform.SetParent(go.transform, false);
            var rect = label.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var t = label.GetComponent<Text>(); t.font = font; t.fontSize = 32; t.alignment = TextAnchor.MiddleCenter; t.color = Color.white;
            t.text = "";
            spawned.Add(go);
        }

        private void RefreshBoard()
        {
            // Rebuild the gameplay screen to keep the Phase 1 implementation deterministic and simple.
            BuildGameplay(screen == GameScreen.Daily);
        }

        private void ClickCell(int row, int col)
        {
            int idx = row * 10 + col;
            if (state.status != PuzzleStatus.Playing) return;

            if (state.placed[idx] != -1)
            {
                state.placed[idx] = -1;
                RefreshBoard();
                return;
            }

            if (!PuzzleValidator.IsPlacementValid(puzzle, state.placed, row, col))
            {
                state.livesRemaining--;
                if (state.livesRemaining <= 0)
                {
                    state.livesRemaining = 0;
                    state.status = PuzzleStatus.Failed;
                    ShowResult(false, screen == GameScreen.Daily);
                    return;
                }

                RefreshBoard();
                return;
            }

            state.placed[idx] = (int)characters.Active;
            if (PuzzleValidator.IsSolved(puzzle, state.placed))
            {
                state.status = PuzzleStatus.Solved;
                CompleteLevel(screen == GameScreen.Daily);
                return;
            }

            RefreshBoard();
        }

        private void CompleteLevel(bool daily)
        {
            if (!daily)
            {
                save.coins += 150;
                if (puzzle.id == $"P{save.unlockedLevel:000}") save.unlockedLevel++;
                SaveService.Save(save);
            }
            ShowResult(true, daily);
        }

        private void ShowResult(bool solved, bool daily)
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Label(bg.transform, solved ? "PUZZLE SOLVED!" : "TRY AGAIN", 54, TextAnchor.MiddleCenter, text, new Vector2(.05f,.66f), new Vector2(.95f,.79f), Vector2.zero, Vector2.zero);
            Label(bg.transform, solved ? $"Great work. {state.livesRemaining} lives left." : "The cats got away this time.", 30, TextAnchor.MiddleCenter, text, new Vector2(.08f,.57f), new Vector2(.92f,.65f), Vector2.zero, Vector2.zero);

            if (daily)
            {
                int treats = solved ? state.livesRemaining : 0;
                Label(bg.transform, $"Daily Treats  •  {treats}", 34, TextAnchor.MiddleCenter, text, new Vector2(.1f,.48f), new Vector2(.9f,.55f), Vector2.zero, Vector2.zero);
            }
            else if (solved)
            {
                Label(bg.transform, "🪙 +150 coins", 34, TextAnchor.MiddleCenter, text, new Vector2(.1f,.48f), new Vector2(.9f,.55f), Vector2.zero, Vector2.zero);
            }

            var primary = Button(bg.transform, daily ? "BACK HOME" : solved ? "NEXT LEVEL" : "RETRY", 34, new Color(.52f,.78f,.47f), () =>
            {
                if (daily || !solved) StartLevel(save.unlockedLevel);
                else StartLevel(save.unlockedLevel);
            }, 108);
            var pr = primary.GetComponent<RectTransform>(); pr.anchorMin = new Vector2(.12f,.33f); pr.anchorMax = new Vector2(.88f,.41f); pr.offsetMin = pr.offsetMax = Vector2.zero;
            if (daily) primary.onClick.RemoveAllListeners(); if (daily) primary.onClick.AddListener(ShowHome);
        }

        private void SpendHint(bool daily)
        {
            if (daily || save.coins < 100) return;
            save.coins -= 100;
            SaveService.Save(save);
            RevealCell(false);
        }

        private void RevealCell(bool daily)
        {
            if (daily || save.coins < 175) return;
            save.coins -= 175;
            SaveService.Save(save);
            for (int r = 0; r < 10; r++)
            {
                int c = puzzle.solution[r];
                int idx = r * 10 + c;
                if (state.placed[idx] == -1)
                {
                    state.placed[idx] = (int)characters.Active;
                    break;
                }
            }
            if (PuzzleValidator.IsSolved(puzzle, state.placed))
            {
                state.status = PuzzleStatus.Solved;
                CompleteLevel(false);
            }
            else RefreshBoard();
        }

        private void ExtraLife(bool daily)
        {
            if (daily || save.coins < 250) return;
            save.coins -= 250; state.livesRemaining++;
            SaveService.Save(save); RefreshBoard();
        }

        private void ShowDaily()
        {
            Clear();
            screen = GameScreen.Daily;
            puzzle = PuzzleRepository.Get(1);
            state = new PuzzleState("DAILY-" + System.DateTime.UtcNow.ToString("yyyyMMdd"));
            state.status = PuzzleStatus.Playing;
            BuildGameplay(true);
        }

        private void ShowShop()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Label(bg.transform, "SHOP", 50, TextAnchor.MiddleCenter, text, new Vector2(0,.88f), new Vector2(1,.96f), Vector2.zero, Vector2.zero);
            Label(bg.transform, $"Coins  🪙 {save.coins}", 28, TextAnchor.MiddleCenter, text, new Vector2(0,.82f), new Vector2(1,.88f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < CharacterCatalog.All.Length; i++)
            {
                var id = CharacterCatalog.All[i];
                int price = i == 0 ? 0 : 500 + i * 500;
                var b = Button(bg.transform, $"{CharacterCatalog.Symbol(id)}  {CharacterCatalog.Name(id)}  {(price == 0 ? "FREE" : "🪙 "+price)}", 28, card,
                    () => BuyCharacter(id, price), 100);
                var r = b.GetComponent<RectTransform>(); r.anchorMin = new Vector2(.1f,.65f-i*.12f); r.anchorMax = new Vector2(.9f,.74f-i*.12f); r.offsetMin = r.offsetMax = Vector2.zero;
            }
            var back = Button(bg.transform, "← HOME", 28, card, ShowHome, 80);
            var br = back.GetComponent<RectTransform>(); br.anchorMin = new Vector2(.1f,.08f); br.anchorMax = new Vector2(.9f,.15f); br.offsetMin = br.offsetMax = Vector2.zero;
        }

        private void BuyCharacter(CharacterId id, int price)
        {
            if (id != CharacterId.Capybara && save.coins < price) return;
            if (id != CharacterId.Capybara) save.coins -= price;
            characters.Select(id, save);
            ShowShop();
        }

        private void ShowLeaderboard()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Label(bg.transform, "WEEKLY LEADERBOARD", 44, TextAnchor.MiddleCenter, text, new Vector2(.03f,.87f), new Vector2(.97f,.95f), Vector2.zero, Vector2.zero);
            Label(bg.transform, "Daily Treats • server-authoritative in production", 25, TextAnchor.MiddleCenter, text, new Vector2(.06f,.81f), new Vector2(.94f,.87f), Vector2.zero, Vector2.zero);
            string[] names = {"You", "Mochi", "Biscuit", "Nori", "Bean"};
            for (int i=0;i<names.Length;i++)
            {
                var row = Panel(bg.transform, i==0 ? new Color(.82f,.91f,.76f) : card, new Vector2(.08f,.70f-i*.105f), new Vector2(.92f,.78f-i*.105f), Vector2.zero, Vector2.zero);
                Label(row.transform, $"{i+1}.  {names[i]}                         {12-i*2}", 26, TextAnchor.MiddleCenter, text, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            }
            var back = Button(bg.transform, "← HOME", 28, card, ShowHome, 80);
            var br = back.GetComponent<RectTransform>(); br.anchorMin = new Vector2(.1f,.08f); br.anchorMax = new Vector2(.9f,.15f); br.offsetMin = br.offsetMax = Vector2.zero;
        }

        private void ShowProfile()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Label(bg.transform, "PROFILE", 50, TextAnchor.MiddleCenter, text, new Vector2(0,.87f), new Vector2(1,.95f), Vector2.zero, Vector2.zero);
            Label(bg.transform, $"{CharacterCatalog.Symbol(characters.Active)}  {CharacterCatalog.Name(characters.Active)}", 42, TextAnchor.MiddleCenter, text, new Vector2(.05f,.74f), new Vector2(.95f,.83f), Vector2.zero, Vector2.zero);
            Label(bg.transform, $"Progression Level  {save.unlockedLevel}\nCoins  🪙 {save.coins}\nCharacters  1 / 5", 30, TextAnchor.MiddleCenter, text, new Vector2(.08f,.52f), new Vector2(.92f,.70f), Vector2.zero, Vector2.zero);
            var settings = Button(bg.transform, $"Sound {(save.sound ? "ON" : "OFF")}  •  Haptics {(save.haptics ? "ON" : "OFF")}", 25, card, () => { save.sound=!save.sound; save.haptics=!save.haptics; SaveService.Save(save); ShowProfile(); }, 90);
            var sr = settings.GetComponent<RectTransform>(); sr.anchorMin = new Vector2(.1f,.38f); sr.anchorMax = new Vector2(.9f,.45f); sr.offsetMin = sr.offsetMax = Vector2.zero;
            var back = Button(bg.transform, "← HOME", 28, card, ShowHome, 80);
            var br = back.GetComponent<RectTransform>(); br.anchorMin = new Vector2(.1f,.08f); br.anchorMax = new Vector2(.9f,.15f); br.offsetMin = br.offsetMax = Vector2.zero;
        }
    }
}
