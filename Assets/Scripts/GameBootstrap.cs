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
        private Sprite roundedSprite;
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

            EnsureRuntimeCamera();
            BuildCanvas();
            ShowHome();
        }

        private void EnsureRuntimeCamera()
        {
            var existing = Object.FindFirstObjectByType<Camera>();
            if (existing != null)
            {
                if (existing.GetComponent<AudioListener>() == null)
                    existing.gameObject.AddComponent<AudioListener>();
                return;
            }

            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.transform.SetParent(transform, false);
            var camera = cameraObject.GetComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            camera.orthographic = true;
            camera.orthographicSize = 5f;
            camera.transform.position = new Vector3(0f, 0f, -10f);
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

            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
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

        private Sprite RoundedSprite()
        {
            if (roundedSprite != null) return roundedSprite;
            const int size = 128, radius = 22;
            var tex = new Texture2D(size,size,TextureFormat.RGBA32,false);
            tex.filterMode = FilterMode.Bilinear;
            var px = new Color[size*size];
            for(int y=0;y<size;y++) for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(radius-x,0f,x-(size-1-radius));
                float dy=Mathf.Max(radius-y,0f,y-(size-1-radius));
                float a=(dx==0f||dy==0f)?1f:Mathf.Clamp01(radius+1f-Mathf.Sqrt(dx*dx+dy*dy));
                px[y*size+x]=new Color(1f,1f,1f,a);
            }
            tex.SetPixels(px); tex.Apply();
            roundedSprite=Sprite.Create(tex,new Rect(0,0,size,size),new Vector2(.5f,.5f),size,0,SpriteMeshType.FullRect,new Vector4(radius,radius,radius,radius),false);
            return roundedSprite;
        }

        private GameObject Panel(Transform parent, Color color, Vector2 min, Vector2 max)
        {
            var go = new GameObject("Panel", typeof(Image), typeof(Shadow));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin = min; rect.anchorMax = max; rect.offsetMin = rect.offsetMax = Vector2.zero;
            var image=go.GetComponent<Image>();
            image.color=color;
            if(min!=Vector2.zero || max!=Vector2.one) image.sprite=RoundedSprite();
            image.raycastTarget=false;
            var shadow=go.GetComponent<Shadow>();
            shadow.effectColor=new Color(.15f,.12f,.10f,.08f);
            shadow.effectDistance=new Vector2(0,-4);
            shadow.useGraphicAlpha=true;
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
            var go = new GameObject("Button", typeof(Image), typeof(Button), typeof(Shadow));
            go.transform.SetParent(parent, false);
            var rect = go.GetComponent<RectTransform>();
            rect.anchorMin=min; rect.anchorMax=max; rect.offsetMin=rect.offsetMax=Vector2.zero;
            var image=go.GetComponent<Image>();
            image.color=fill; image.sprite=RoundedSprite();
            var shadow=go.GetComponent<Shadow>();
            shadow.effectColor=new Color(.15f,.12f,.10f,.10f); shadow.effectDistance=new Vector2(0,-3); shadow.useGraphicAlpha=true;
            var button=go.GetComponent<Button>();
            button.transition=Selectable.Transition.ColorTint;
            button.colors=new ColorBlock{normalColor=fill,highlightedColor=Color.Lerp(fill,Color.white,.12f),pressedColor=Color.Lerp(fill,Color.black,.08f),selectedColor=fill,disabledColor=new Color(fill.r,fill.g,fill.b,.45f),colorMultiplier=1f};
            button.navigation=new Navigation{mode=Navigation.Mode.None};
            if(action!=null)button.onClick.AddListener(action);

            var textObject=new GameObject("ButtonText",typeof(Text));
            textObject.transform.SetParent(go.transform,false);
            var tr=textObject.GetComponent<RectTransform>();
            tr.anchorMin=Vector2.zero; tr.anchorMax=Vector2.one; tr.offsetMin=new Vector2(12,0); tr.offsetMax=new Vector2(-12,0);
            var t=textObject.GetComponent<Text>();
            t.font=font; t.text=title; t.fontSize=size; t.alignment=TextAnchor.MiddleCenter; t.color=text; t.fontStyle=FontStyle.Bold;
            t.raycastTarget=false;
            spawned.Add(go);
            return button;
        }

        private void ShowHome()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            Label(bg.transform,"CAPYBARA",46,TextAnchor.MiddleCenter,text,new Vector2(.08f,.87f),new Vector2(.92f,.96f));
            Label(bg.transform,"A tiny puzzle. A big little world.",22,TextAnchor.MiddleCenter,new Color(.40f,.35f,.31f),new Vector2(.08f,.82f),new Vector2(.92f,.87f));

            var mascot=new GameObject("CapybaraMascot",typeof(RectTransform));
            mascot.transform.SetParent(bg.transform,false);
            var mr=mascot.GetComponent<RectTransform>();
            mr.anchorMin=new Vector2(.30f,.59f); mr.anchorMax=new Vector2(.70f,.80f); mr.offsetMin=mr.offsetMax=Vector2.zero;
            var face=mascot.AddComponent<CharacterFaceView>(); face.Build(CharacterId.Capybara,save.reducedMotion); face.SetVisible(true);

            Label(bg.transform,$"LEVEL {save.unlockedLevel}",24,TextAnchor.MiddleCenter,text,new Vector2(.08f,.53f),new Vector2(.42f,.58f));
            Label(bg.transform,$"🪙 {save.coins}",24,TextAnchor.MiddleCenter,text,new Vector2(.58f,.53f),new Vector2(.92f,.58f));

            Button(bg.transform,"PLAY",38,new Color(.55f,.78f,.48f),()=>StartLevel(save.unlockedLevel),new Vector2(.10f,.40f),new Vector2(.90f,.49f));
            Button(bg.transform,"HOW TO PLAY",22,card,ShowRules,new Vector2(.10f,.31f),new Vector2(.48f,.37f));
            Button(bg.transform,"SETTINGS",22,card,ShowSettings,new Vector2(.52f,.31f),new Vector2(.90f,.37f));

            Label(bg.transform,$"{CharacterCatalog.ResourceIcon(characters.Active)}  {save.treats} {CharacterCatalog.ResourceName(characters.Active)}",21,TextAnchor.MiddleCenter,new Color(.40f,.35f,.31f),new Vector2(.10f,.21f),new Vector2(.90f,.27f));
            Label(bg.transform,"Solve one puzzle. Win it. Move forward.",19,TextAnchor.MiddleCenter,new Color(.50f,.45f,.41f),new Vector2(.10f,.15f),new Vector2(.90f,.20f));
        }

        private void ShowLevelSelect()
        {
            Clear();
            var bg = Panel(canvas.transform, background, Vector2.zero, Vector2.one);
            Label(bg.transform, "LEVEL SELECT", 50, TextAnchor.MiddleCenter, text, new Vector2(.06f,.88f), new Vector2(.94f,.96f));
            const int columns = 4;
            const int pageSize = 40;
            int pageCount = (LevelCatalog.MaxLevel + pageSize - 1) / pageSize;
            Label(bg.transform, $"COMPLETED  {ProgressionModel.GetCompletedCount(save)} / {LevelCatalog.MaxLevel}    PAGE {levelSelectPage + 1}/{pageCount}", 22, TextAnchor.MiddleCenter, text, new Vector2(.04f,.83f), new Vector2(.96f,.88f));
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
            if (!ProgressionModel.IsUnlocked(save, level)) level = save.unlockedLevel;
            save.currentLevel = level;
            SaveService.Save(save);
            gameplay.LoadLevel(level, characters.Active);
            BuildGameplay();
        }

        private void BuildGameplay()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            var puzzle=gameplay.Puzzle;

            Button(bg.transform,"Ⅱ",26,card,PauseGame,new Vector2(.06f,.92f),new Vector2(.16f,.97f));
            Label(bg.transform,$"PUZZLE {gameplay.LevelId}",30,TextAnchor.MiddleCenter,text,new Vector2(.20f,.92f),new Vector2(.80f,.97f));
            Label(bg.transform,$"{CharacterCatalog.ResourceIcon(gameplay.ActiveCharacter)}  {gameplay.State.livesRemaining} {CharacterCatalog.ResourceName(gameplay.ActiveCharacter)}",22,TextAnchor.MiddleCenter,new Color(.40f,.35f,.31f),new Vector2(.62f,.84f),new Vector2(.94f,.90f));
            Label(bg.transform,$"{CharacterCatalog.Name(gameplay.ActiveCharacter)}  ·  {puzzle.difficultyBand}",20,TextAnchor.MiddleCenter,new Color(.48f,.42f,.38f),new Vector2(.06f,.84f),new Vector2(.60f,.90f));

            var boardObject=new GameObject("Board",typeof(RectTransform));
            boardObject.transform.SetParent(bg.transform,false);
            var br=boardObject.GetComponent<RectTransform>();
            br.anchorMin=new Vector2(.06f,.29f); br.anchorMax=new Vector2(.94f,.82f); br.offsetMin=br.offsetMax=Vector2.zero;
            board=boardObject.AddComponent<BoardView>();
            board.Build(puzzle,regionColors,gameplay.ActiveCharacter,save.reducedMotion,OnCellMarked,OnCellDoubleTapped);
            board.Refresh(gameplay.State);

            Label(bg.transform,"TAP  ×",17,TextAnchor.MiddleCenter,new Color(.48f,.43f,.39f),new Vector2(.06f,.23f),new Vector2(.47f,.28f));
            Label(bg.transform,"DOUBLE-TAP  PLACE",17,TextAnchor.MiddleCenter,new Color(.48f,.43f,.39f),new Vector2(.53f,.23f),new Vector2(.94f,.28f));
            Button(bg.transform,"HINT  · 100",20,card,UseHint,new Vector2(.06f,.14f),new Vector2(.29f,.20f));
            Button(bg.transform,"REVEAL  · 175",20,card,UseReveal,new Vector2(.385f,.14f),new Vector2(.615f,.20f));
            Button(bg.transform,"EXTRA  · 250",20,card,UseExtraLife,new Vector2(.71f,.14f),new Vector2(.94f,.20f));
            Label(bg.transform,$"🪙 {save.coins}",19,TextAnchor.MiddleCenter,new Color(.45f,.40f,.36f),new Vector2(.25f,.07f),new Vector2(.75f,.12f));
        }

        private static string LifeText(int lives)
        {
            return Mathf.Clamp(lives,0,3).ToString();
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

        private void OnCellMarked(int row,int column)
        {
            if(gameplay.CurrentState!=GameplayState.Playing)return;
            gameplay.MarkCell(row,column);
            if(gameplay.CurrentState==GameplayState.Playing)RefreshGameplay();
        }

        private void OnCellDoubleTapped(int row,int column)
        {
            if(gameplay.CurrentState!=GameplayState.Playing)return;
            gameplay.TapCell(row,column);
            if(gameplay.CurrentState==GameplayState.Playing)RefreshGameplay();
        }

        private void RefreshGameplay()
        {
            if(board==null||gameplay.State==null)return;
            board.Refresh(gameplay.State);
            var labels=FindObjectsOfType<Text>();
            string target=$"{CharacterCatalog.ResourceIcon(gameplay.ActiveCharacter)}  {gameplay.State.livesRemaining} {CharacterCatalog.ResourceName(gameplay.ActiveCharacter)}";
            foreach(var t in labels)
                if(t.text!=null && (t.text.Contains("berries")||t.text.Contains("fish")||t.text.Contains("bones")||t.text.Contains("bamboo")))
                    t.text=target;
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

        private void ShowResult(bool solved,RewardResult reward)
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            var mascot=new GameObject("ResultFace",typeof(RectTransform));
            mascot.transform.SetParent(bg.transform,false);
            var mr=mascot.GetComponent<RectTransform>();
            mr.anchorMin=new Vector2(.34f,.62f); mr.anchorMax=new Vector2(.66f,.80f); mr.offsetMin=mr.offsetMax=Vector2.zero;
            var face=mascot.AddComponent<CharacterFaceView>(); face.Build(gameplay.ActiveCharacter,save.reducedMotion); face.SetVisible(true);

            Label(bg.transform,solved?"PUZZLE SOLVED!":"TRY AGAIN",38,TextAnchor.MiddleCenter,text,new Vector2(.08f,.52f),new Vector2(.92f,.60f));
            Label(bg.transform,solved
                ? $"{CharacterCatalog.ResourceIcon(gameplay.ActiveCharacter)} +{reward.Treats} {CharacterCatalog.ResourceName(gameplay.ActiveCharacter)}\n🪙 +{reward.Coins} coins"
                : $"No {CharacterCatalog.ResourceName(gameplay.ActiveCharacter)} left",25,TextAnchor.MiddleCenter,new Color(.40f,.35f,.31f),new Vector2(.10f,.41f),new Vector2(.90f,.51f));

            if(solved)
            {
                Button(bg.transform,gameplay.LevelId<500?"NEXT PUZZLE":"ALL PUZZLES COMPLETE",34,new Color(.55f,.78f,.48f),()=>{
                    gameplay.Advance();
                    int next=LevelCatalog.NextLevel(gameplay.LevelId);
                    if(next>0)StartLevel(next); else ShowHome();
                },new Vector2(.10f,.27f),new Vector2(.90f,.36f));
            }
            else
            {
                Button(bg.transform,"RETRY PUZZLE",32,new Color(.55f,.78f,.48f),()=>StartLevel(gameplay.LevelId),new Vector2(.10f,.28f),new Vector2(.90f,.37f));
                Button(bg.transform,"HOME",22,card,ShowHome,new Vector2(.28f,.18f),new Vector2(.72f,.24f));
            }
        }

        private void PauseGame()
        {
            gameplay.Pause();
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            Label(bg.transform,"PAUSED",40,TextAnchor.MiddleCenter,text,new Vector2(.08f,.68f),new Vector2(.92f,.76f));
            Button(bg.transform,"RESUME",32,new Color(.55f,.78f,.48f),()=>{gameplay.Resume();BuildGameplay();},new Vector2(.10f,.52f),new Vector2(.90f,.61f));
            Button(bg.transform,"RESTART PUZZLE",28,card,()=>{gameplay.Restart();BuildGameplay();},new Vector2(.10f,.41f),new Vector2(.90f,.49f));
            Button(bg.transform,"HOME",24,card,()=>{gameplay.Exit();ShowHome();},new Vector2(.22f,.28f),new Vector2(.78f,.35f));
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