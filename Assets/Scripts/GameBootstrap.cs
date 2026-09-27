using CapybaraGame.Audio;
using CapybaraGame.Challenges;
using CapybaraGame.Analytics;
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
        private RectTransform safeAreaRoot;
        private Text resourceText;
        private Text deductionText;
        private Font font;
        private Sprite roundedSprite;
        private readonly System.Collections.Generic.List<GameObject> spawned = new System.Collections.Generic.List<GameObject>();
        private bool pendingHint;
        private bool pendingReveal;
        private bool dailyMode;
        private bool goldenMode;
        private const int BerryReviveCost = 500;
        private RewardedAdService rewardedAds;
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
            rewardedAds = new RewardedAdService();

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
            camera.backgroundColor = background;
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

            safeAreaRoot = new GameObject("SafeArea", typeof(RectTransform)).GetComponent<RectTransform>();
            safeAreaRoot.SetParent(canvas.transform, false);
            safeAreaRoot.anchorMin = Vector2.zero;
            safeAreaRoot.anchorMax = Vector2.one;
            safeAreaRoot.offsetMin = safeAreaRoot.offsetMax = Vector2.zero;
            safeAreaRoot.gameObject.AddComponent<SafeAreaFitter>();

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
            if (min == Vector2.zero && max == Vector2.one)
                go.AddComponent<ScreenPanelAnimator>().Play(save != null && save.reducedMotion);
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
            var feel=go.AddComponent<UIInteractionFeedback>();
            feel.reducedMotion=save != null && save.reducedMotion;
            button.colors=new ColorBlock{normalColor=fill,highlightedColor=Color.Lerp(fill,Color.white,.12f),pressedColor=Color.Lerp(fill,Color.black,.08f),selectedColor=fill,disabledColor=new Color(fill.r,fill.g,fill.b,.45f),colorMultiplier=1f};
            button.navigation=new Navigation{mode=Navigation.Mode.None};
            if(action!=null)button.onClick.AddListener(() => {
                audioService?.Play(SfxType.Button);
                haptics?.Play(HapticType.Tiny);
                action();
            });

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

            // Header
            Label(bg.transform,"CAPY",38,TextAnchor.MiddleLeft,text,new Vector2(.08f,.91f),new Vector2(.45f,.97f));
            var coinPill=Panel(bg.transform,new Color(1f,.985f,.965f),new Vector2(.57f,.915f),new Vector2(.92f,.965f));
            Label(coinPill.transform,"●  "+save.coins,20,TextAnchor.MiddleCenter,new Color(.68f,.47f,.12f),Vector2.zero,Vector2.one);
            Button(bg.transform,"⚙",22,card,ShowSettings,new Vector2(.06f,.84f),new Vector2(.15f,.90f));

            // Hero
            var hero=Panel(bg.transform,new Color(1f,.985f,.965f),new Vector2(.08f,.57f),new Vector2(.92f,.84f));
            Label(hero.transform,"YOUR NEXT PUZZLE",17,TextAnchor.MiddleCenter,new Color(.48f,.42f,.38f),new Vector2(.08f,.83f),new Vector2(.92f,.93f));
            Label(hero.transform,$"LEVEL {save.unlockedLevel}",38,TextAnchor.MiddleCenter,text,new Vector2(.08f,.68f),new Vector2(.92f,.82f));
            var mascot=new GameObject("CapybaraMascot",typeof(RectTransform));
            mascot.transform.SetParent(hero.transform,false);
            var mr=mascot.GetComponent<RectTransform>();
            mr.anchorMin=new Vector2(.34f,.08f); mr.anchorMax=new Vector2(.66f,.70f); mr.offsetMin=mr.offsetMax=Vector2.zero;
            var face=mascot.AddComponent<CharacterFaceView>(); face.Build(CharacterId.Capybara,save.reducedMotion); face.SetVisible(true);

            Button(bg.transform,"PLAY",34,new Color(.55f,.78f,.48f),()=>StartLevel(save.unlockedLevel),new Vector2(.10f,.46f),new Vector2(.90f,.54f));

            // Secondary actions
            Button(bg.transform,"STORE",18,card,ShowStore,new Vector2(.06f,.38f),new Vector2(.30f,.44f));
            Button(bg.transform,"HOW TO PLAY",18,card,ShowRules,new Vector2(.35f,.38f),new Vector2(.65f,.44f));
            Button(bg.transform,"DAILY",18,card,StartDaily,new Vector2(.70f,.38f),new Vector2(.94f,.44f));
            Button(bg.transform,"GOLDEN",18,card,StartGolden,new Vector2(.06f,.30f),new Vector2(.30f,.36f));
            Button(bg.transform,"WEEKLY",18,card,ShowWeekly,new Vector2(.35f,.30f),new Vector2(.65f,.36f));
            Button(bg.transform,"SETTINGS",18,card,ShowSettings,new Vector2(.70f,.30f),new Vector2(.94f,.36f));

            // Progress snapshot
            var stats=Panel(bg.transform,new Color(1f,.985f,.965f),new Vector2(.10f,.20f),new Vector2(.90f,.34f));
            Label(stats.transform,"PROGRESS",15,TextAnchor.MiddleLeft,new Color(.50f,.44f,.40f),new Vector2(.06f,.58f),new Vector2(.45f,.92f));
            Label(stats.transform,$"{save.completedLevels.Count} puzzles solved",19,TextAnchor.MiddleLeft,text,new Vector2(.06f,.10f),new Vector2(.62f,.60f));
            Label(stats.transform,$"{save.treats} treats",19,TextAnchor.MiddleRight,new Color(.68f,.47f,.12f),new Vector2(.62f,.10f),new Vector2(.94f,.60f));

            Label(bg.transform,"Simple puzzle. Deep little world.",18,TextAnchor.MiddleCenter,new Color(.50f,.45f,.41f),new Vector2(.10f,.13f),new Vector2(.90f,.18f));
        }

        private void ShowStore()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            Label(bg.transform,"STORE",46,TextAnchor.MiddleCenter,text,new Vector2(.08f,.89f),new Vector2(.92f,.96f));
            Label(bg.transform,"Characters",25,TextAnchor.MiddleLeft,text,new Vector2(.08f,.82f),new Vector2(.92f,.87f));

            var ids=CharacterCatalog.All;
            for(int i=0;i<ids.Length;i++)
            {
                var id=ids[i];
                float x0=.07f+i*.185f, x1=x0+.17f;
                var cardObject=Panel(bg.transform,card,new Vector2(x0,.60f),new Vector2(x1,.80f));
                var faceObject=new GameObject("StoreFace",typeof(RectTransform));
                faceObject.transform.SetParent(cardObject.transform,false);
                var fr=faceObject.GetComponent<RectTransform>();
                fr.anchorMin=new Vector2(.18f,.35f); fr.anchorMax=new Vector2(.82f,.95f); fr.offsetMin=fr.offsetMax=Vector2.zero;
                var face=faceObject.AddComponent<CharacterFaceView>();
                face.Build(id,save.reducedMotion); face.SetVisible(true);

                bool owned=characters.IsOwned(id,save);
                int price=CharacterStorePrice(id);
                string title=owned ? (characters.Active==id ? "EQUIPPED" : "USE") : price.ToString();
                Color fill=owned ? new Color(.84f,.92f,.78f) : new Color(.96f,.84f,.54f);
                Button(cardObject.transform,title,16,fill,()=>{
                    if(characters.IsOwned(id,save)) { characters.Select(id,save); ShowStore(); }
                    else if(save.coins>=price) { save.coins-=price; save.ownedCharacters.Add((int)id); characters.Select(id,save); SaveService.Save(save); ShowStore(); }
                },new Vector2(.08f,.05f),new Vector2(.92f,.30f));
            }

            Label(bg.transform,"Aids",25,TextAnchor.MiddleLeft,text,new Vector2(.08f,.53f),new Vector2(.92f,.58f));
            Label(bg.transform,$"Hint  ·  100 coins  ·  OWNED {save.hintAids}",21,TextAnchor.MiddleLeft,new Color(.40f,.35f,.31f),new Vector2(.10f,.45f),new Vector2(.72f,.50f));
            Button(bg.transform,"BUY 1",19,new Color(.84f,.92f,.78f),()=>{if(save.coins>=100){save.coins-=100;save.hintAids++;SaveService.Save(save);ShowStore();}},new Vector2(.75f,.45f),new Vector2(.90f,.50f));
            Label(bg.transform,$"Reveal  ·  175 coins  ·  OWNED {save.revealAids}",21,TextAnchor.MiddleLeft,new Color(.40f,.35f,.31f),new Vector2(.10f,.37f),new Vector2(.72f,.42f));
            Button(bg.transform,"BUY 1",19,new Color(.84f,.92f,.78f),()=>{if(save.coins>=175){save.coins-=175;save.revealAids++;SaveService.Save(save);ShowStore();}},new Vector2(.75f,.37f),new Vector2(.90f,.42f));
            Label(bg.transform,"Aids are purchased here before a puzzle. Nothing can be bought during active play.",17,TextAnchor.MiddleCenter,new Color(.50f,.45f,.41f),new Vector2(.08f,.28f),new Vector2(.92f,.34f));
            Label(bg.transform,$"COINS {save.coins}",22,TextAnchor.MiddleCenter,text,new Vector2(.25f,.20f),new Vector2(.75f,.26f));
            Button(bg.transform,"BACK",26,card,ShowHome,new Vector2(.18f,.09f),new Vector2(.82f,.16f));
        }

        private int CharacterStorePrice(CharacterId id)
        {
            switch(id)
            {
                case CharacterId.Cat: return 750;
                case CharacterId.Dog: return 1500;
                case CharacterId.Penguin: return 2000;
                case CharacterId.Panda: return 3000;
                default: return 0;
            }
        }

        private void ShowLevelReady()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);

            Button(bg.transform,"‹",34,card,ShowHome,new Vector2(.06f,.91f),new Vector2(.15f,.97f));
            var levelDefinition = LevelCatalog.Get(gameplay.LevelId);
            string readyTitle = dailyMode ? "DAILY" : goldenMode ? "GOLDEN CHALLENGE" : levelDefinition.IsHardChallenge ? "HARD CHALLENGE" : levelDefinition.IsBreather ? "BREATHER LEVEL" : "LEVEL "+gameplay.LevelId;
            Label(bg.transform,readyTitle,30,TextAnchor.MiddleCenter,text,new Vector2(.20f,.92f),new Vector2(.80f,.97f));
            var coinPill=Panel(bg.transform,new Color(1f,.985f,.965f),new Vector2(.70f,.855f),new Vector2(.92f,.90f));
            Label(coinPill.transform,"●  "+save.coins,17,TextAnchor.MiddleCenter,new Color(.68f,.47f,.12f),Vector2.zero,Vector2.one);

            var hero=Panel(bg.transform,new Color(1f,.985f,.965f),new Vector2(.08f,.57f),new Vector2(.92f,.85f));
            var faceObject=new GameObject("ReadyFace",typeof(RectTransform));
            faceObject.transform.SetParent(hero.transform,false);
            var fr=faceObject.GetComponent<RectTransform>();
            fr.anchorMin=new Vector2(.36f,.20f); fr.anchorMax=new Vector2(.64f,.82f); fr.offsetMin=fr.offsetMax=Vector2.zero;
            var face=faceObject.AddComponent<CharacterFaceView>(); face.Build(gameplay.ActiveCharacter,save.reducedMotion); face.SetVisible(true);
            Label(hero.transform,$"{CharacterCatalog.Name(gameplay.ActiveCharacter)}'s turn",18,TextAnchor.MiddleCenter,new Color(.48f,.42f,.38f),new Vector2(.08f,.04f),new Vector2(.92f,.18f));

            var rules=Panel(bg.transform,card,new Vector2(.08f,.38f),new Vector2(.92f,.54f));
            Label(rules.transform,"3 "+CharacterCatalog.ResourceName(gameplay.ActiveCharacter)+"  •  1 per region  •  1 per row  •  1 per column",17,TextAnchor.MiddleCenter,text,new Vector2(.05f,.52f),new Vector2(.95f,.92f));
            Label(rules.transform,"Characters cannot touch, including diagonally.",16,TextAnchor.MiddleCenter,new Color(.48f,.42f,.38f),new Vector2(.05f,.12f),new Vector2(.95f,.52f));
            if (levelDefinition.IsTeaching)
                Label(bg.transform,"LEARN: mark impossible cells with X, then place when only one candidate remains.",15,TextAnchor.MiddleCenter,new Color(.42f,.52f,.38f),new Vector2(.08f,.32f),new Vector2(.92f,.37f));
            else if (levelDefinition.IsBreather)
                Label(bg.transform,"BREATHER: a lower-pressure puzzle before the next difficulty step.",15,TextAnchor.MiddleCenter,new Color(.42f,.52f,.38f),new Vector2(.08f,.32f),new Vector2(.92f,.37f));

            if(!dailyMode && !goldenMode && (save.hintAids>0 || save.revealAids>0))
            {
                Label(bg.transform,"READY YOUR AIDS",15,TextAnchor.MiddleCenter,new Color(.50f,.44f,.40f),new Vector2(.10f,.32f),new Vector2(.90f,.37f));
                Button(bg.transform,pendingHint?"✓ HINT READY":"HINT  ·  "+save.hintAids,18,pendingHint?new Color(.55f,.78f,.48f):card,()=>{
                    if(!pendingHint && save.hintAids>0){save.hintAids--;pendingHint=true;SaveService.Save(save);ShowLevelReady();}
                },new Vector2(.10f,.25f),new Vector2(.47f,.31f));
                Button(bg.transform,pendingReveal?"✓ REVEAL READY":"REVEAL  ·  "+save.revealAids,18,pendingReveal?new Color(.55f,.78f,.48f):card,()=>{
                    if(!pendingReveal && save.revealAids>0){save.revealAids--;pendingReveal=true;SaveService.Save(save);ShowLevelReady();}
                },new Vector2(.53f,.25f),new Vector2(.90f,.31f));
            }

            Button(bg.transform,"PLAY PUZZLE",30,new Color(.55f,.78f,.48f),BeginLevel,new Vector2(.10f,.15f),new Vector2(.90f,.23f));
            Button(bg.transform,"STORE",18,card,ShowStore,new Vector2(.10f,.08f),new Vector2(.43f,.13f));
            Button(bg.transform,"BACK",18,card,ShowHome,new Vector2(.57f,.08f),new Vector2(.90f,.13f));
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

        private void StartDaily()
        {
            string day = ChallengeService.TodayIdUtc();
            if (ChallengeService.HasDailyResult(day)) { ShowDailyStatus(day); return; }
            var readiness = DailyReadinessService.Evaluate(day);
            if (!readiness.Ready)
            {
                ShowDailyStatus(day);
                return;
            }
            dailyMode = true; goldenMode = false; pendingHint = false; pendingReveal = false;
            gameplay.LoadLevel(ChallengeService.DailyLevelId(day), characters.Active);
            AnalyticsService.Track("daily_started", day);
            ShowLevelReady();
        }

        private void StartGolden()
        {
            string day = ChallengeService.TodayIdUtc();
            if (!ChallengeService.TryClaimGoldenAttempt(day)) { ShowGoldenStatus(); return; }
            dailyMode = false; goldenMode = true; pendingHint = false; pendingReveal = false;
            gameplay.LoadLevel(ChallengeService.DailyLevelId(day), characters.Active, 1);
            AnalyticsService.Track("golden_started", day);
            ShowLevelReady();
        }

        private void ShowDailyStatus(string day)
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            var result=ChallengeService.GetDailyResult(day);
            Label(bg.transform,"DAILY",48,TextAnchor.MiddleCenter,text,new Vector2(.08f,.78f),new Vector2(.92f,.88f));
            Label(bg.transform,result==null?"No result yet":$"TODAY'S SCORE  {result.treatScore}/3",28,TextAnchor.MiddleCenter,text,new Vector2(.08f,.58f),new Vector2(.92f,.68f));
            Label(bg.transform,"The Daily accepts one eligible completed attempt.",18,TextAnchor.MiddleCenter,new Color(.50f,.45f,.41f),new Vector2(.08f,.50f),new Vector2(.92f,.56f));
            Button(bg.transform,"BACK",28,card,ShowHome,new Vector2(.18f,.12f),new Vector2(.82f,.20f));
        }

        private void ShowGoldenStatus()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            Label(bg.transform,"GOLDEN CHALLENGE",40,TextAnchor.MiddleCenter,text,new Vector2(.06f,.78f),new Vector2(.94f,.88f));
            Label(bg.transform,"Today's Golden attempt is already used.",22,TextAnchor.MiddleCenter,text,new Vector2(.08f,.60f),new Vector2(.92f,.68f));
            Label(bg.transform,$"Golden Treats  {ChallengeService.GoldenTreats(characters.Active)}",20,TextAnchor.MiddleCenter,new Color(.68f,.47f,.12f),new Vector2(.08f,.52f),new Vector2(.92f,.58f));
            Button(bg.transform,"BACK",28,card,ShowHome,new Vector2(.18f,.12f),new Vector2(.82f,.20f));
        }

        private void ShowWeekly()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            var score=WeeklyLeaderboardService.GetLocalScore();
            Label(bg.transform,"WEEKLY",48,TextAnchor.MiddleCenter,text,new Vector2(.08f,.78f),new Vector2(.92f,.88f));
            Label(bg.transform,$"{score.treatScore} TREATS",34,TextAnchor.MiddleCenter,new Color(.68f,.47f,.12f),new Vector2(.08f,.61f),new Vector2(.92f,.70f));
            Label(bg.transform,$"{score.perfectDays} PERFECT DAYS",20,TextAnchor.MiddleCenter,text,new Vector2(.08f,.53f),new Vector2(.92f,.59f));
            Label(bg.transform,"Your local weekly score. Server leaderboard integration is a release backend task.",16,TextAnchor.MiddleCenter,new Color(.50f,.45f,.41f),new Vector2(.10f,.40f),new Vector2(.90f,.50f));
            Button(bg.transform,"BACK",28,card,ShowHome,new Vector2(.18f,.12f),new Vector2(.82f,.20f));
        }

        private void StartLevel(int level)
        {
            if (!ProgressionModel.IsUnlocked(save, level)) level = save.unlockedLevel;
            pendingHint=false;
            pendingReveal=false;
            dailyMode = false; goldenMode = false;
            save.currentLevel=level;
            SaveService.Save(save);
            gameplay.LoadLevel(level,characters.Active);
            ShowLevelReady();
        }

        private void BeginLevel()
        {
            if(gameplay.Puzzle==null)return;
            BuildGameplay();
            if(pendingHint && gameplay.TryGetSolutionCell(out var hintRow,out var hintCol))
            {
                board.SetSelected(hintRow,hintCol);
                board.Refresh(gameplay.State);
            }
            if(!dailyMode && !goldenMode && pendingReveal && gameplay.TryGetSolutionCell(out var revealRow,out var revealCol))
            {
                gameplay.TapCell(revealRow,revealCol);
                if(gameplay.CurrentState==GameplayState.Playing)RefreshGameplay();
            }
            pendingHint=false;
            pendingReveal=false;
        }

        private void BuildGameplay()
        {
            Clear();
            var bg=Panel(canvas.transform,background,Vector2.zero,Vector2.one);
            var puzzle=gameplay.Puzzle;

            // Compact premium HUD
            Button(bg.transform,"‹",32,card,PauseGame,new Vector2(.05f,.92f),new Vector2(.14f,.975f));
            var levelCard=Panel(bg.transform,card,new Vector2(.19f,.915f),new Vector2(.48f,.975f));
            Label(levelCard.transform,"LEVEL "+gameplay.LevelId,21,TextAnchor.MiddleCenter,text,Vector2.zero,Vector2.one);
            var resourceCard=Panel(bg.transform,card,new Vector2(.52f,.915f),new Vector2(.95f,.975f));
            resourceText = Label(resourceCard.transform,CharacterCatalog.ResourceIcon(gameplay.ActiveCharacter)+"  "+gameplay.State.livesRemaining+" "+CharacterCatalog.ResourceName(gameplay.ActiveCharacter),19,TextAnchor.MiddleCenter,new Color(.55f,.38f,.16f),Vector2.zero,Vector2.one);

            // Character identity strip
            var identity=Panel(bg.transform,new Color(1f,.985f,.965f),new Vector2(.07f,.855f),new Vector2(.93f,.905f));
            Label(identity.transform,CharacterCatalog.Name(gameplay.ActiveCharacter).ToUpper()+"  ·  "+(puzzle.isHardChallenge ? "HARD CHALLENGE" : puzzle.difficultyBand.ToUpper()),16,TextAnchor.MiddleCenter,new Color(.48f,.42f,.38f),Vector2.zero,Vector2.one);

            // Rule cards
            var rules=Panel(bg.transform,card,new Vector2(.07f,.775f),new Vector2(.93f,.845f));
            Label(rules.transform,"1 / REGION     1 / ROW     1 / COLUMN     NO TOUCHING",15,TextAnchor.MiddleCenter,text,new Vector2(.03f,.35f),new Vector2(.97f,.90f));
            Label(rules.transform,levelDefinition.IsTeaching ? "Single tap = mark X   •   Double tap = place" : "Single tap = X   •   Double tap = place",14,TextAnchor.MiddleCenter,new Color(.50f,.44f,.40f),new Vector2(.03f,.02f),new Vector2(.97f,.40f));

            // Board dominates the screen
            var boardCard=Panel(bg.transform,new Color(1f,.995f,.985f),new Vector2(.055f,.275f),new Vector2(.945f,.765f));
            var boardObject=new GameObject("Board",typeof(RectTransform));
            boardObject.transform.SetParent(boardCard.transform,false);
            var br=boardObject.GetComponent<RectTransform>();
            br.anchorMin=new Vector2(.02f,.02f); br.anchorMax=new Vector2(.98f,.98f); br.offsetMin=br.offsetMax=Vector2.zero;
            board=boardObject.AddComponent<BoardView>();
            board.Build(puzzle,regionColors,gameplay.ActiveCharacter,save.reducedMotion,OnCellMarked,OnCellDoubleTapped);
            board.Refresh(gameplay.State);

            // Bottom action guidance
            var guide=Panel(bg.transform,card,new Vector2(.08f,.19f),new Vector2(.92f,.245f));
            Label(guide.transform,"X  MARK CANDIDATE",15,TextAnchor.MiddleLeft,new Color(.48f,.42f,.38f),new Vector2(.04f,.05f),new Vector2(.48f,.95f));
            Label(guide.transform,"DOUBLE-TAP  PLACE",15,TextAnchor.MiddleRight,new Color(.48f,.42f,.38f),new Vector2(.52f,.05f),new Vector2(.96f,.95f));
            var deduction = DeductionAnalyzer.Analyze(puzzle, gameplay.State, 1);
            string deductionMessage = deduction.Count > 0 ? "DEDUCTION  ·  "+deduction[0].Message : "KEEP ELIMINATING ROWS, COLUMNS, REGIONS AND NEIGHBORS";
            deductionText = Label(bg.transform,deductionMessage,13,TextAnchor.MiddleCenter,new Color(.42f,.47f,.40f),new Vector2(.08f,.15f),new Vector2(.92f,.185f));

            Label(bg.transform,"AIDS PREPARED BEFORE PLAY  ·  NO SHOPPING DURING PUZZLE",13,TextAnchor.MiddleCenter,new Color(.55f,.50f,.46f),new Vector2(.08f,.10f),new Vector2(.92f,.15f));
            var coinPill=Panel(bg.transform,card,new Vector2(.32f,.045f),new Vector2(.68f,.09f));
            Label(coinPill.transform,"●  "+save.coins,16,TextAnchor.MiddleCenter,new Color(.68f,.47f,.12f),Vector2.zero,Vector2.one);
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
            if (resourceText != null)
                resourceText.text = CharacterCatalog.ResourceIcon(gameplay.ActiveCharacter)+"  "+gameplay.State.livesRemaining+" "+CharacterCatalog.ResourceName(gameplay.ActiveCharacter);
            if (deductionText != null)
            {
                var deductions = DeductionAnalyzer.Analyze(gameplay.Puzzle, gameplay.State, 1);
                deductionText.text = deductions.Count > 0 ? "DEDUCTION  ·  "+deductions[0].Message : "KEEP ELIMINATING ROWS, COLUMNS, REGIONS AND NEIGHBORS";
            }
        }

        private void OnCompleted(RewardResult reward)
        {
            audioService.Play(SfxType.Completion);
            audioService.Play(SfxType.Reward);
            if (gameplay.State != null && gameplay.State.livesRemaining >= GameplayController.MaxLives)
                audioService.Play(SfxType.PerfectCompletion);
            haptics.Play(HapticType.Celebration);
            if (goldenMode)
            {
                ChallengeService.AddGoldenTreats(gameplay.ActiveCharacter, 5);
                AnalyticsService.Track("golden_completed", gameplay.LevelId.ToString());
            }
            else if (dailyMode)
            {
                ChallengeService.TryRecordDaily(ChallengeService.TodayIdUtc(), gameplay.LevelId, gameplay.State.livesRemaining, Mathf.Clamp(gameplay.State.livesRemaining,0,3));
                AnalyticsService.Track("daily_completed", gameplay.State.livesRemaining.ToString());
            }
            else
            {
                ProgressionModel.ApplyCompletion(save, gameplay.LevelId, reward);
            }
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

            var top=Panel(bg.transform,card,new Vector2(.08f,.88f),new Vector2(.92f,.96f));
            Label(top.transform,"LEVEL "+gameplay.LevelId,22,TextAnchor.MiddleCenter,text,Vector2.zero,Vector2.one);

            var hero=Panel(bg.transform,solved?new Color(.92f,.96f,.87f):new Color(1f,.93f,.89f),new Vector2(.08f,.53f),new Vector2(.92f,.84f));
            var mascot=new GameObject("ResultFace",typeof(RectTransform));
            mascot.transform.SetParent(hero.transform,false);
            var mr=mascot.GetComponent<RectTransform>();
            mr.anchorMin=new Vector2(.34f,.20f); mr.anchorMax=new Vector2(.66f,.88f); mr.offsetMin=mr.offsetMax=Vector2.zero;
            var face=mascot.AddComponent<CharacterFaceView>(); face.Build(gameplay.ActiveCharacter,save.reducedMotion); face.SetVisible(true); face.PlayEmotion(solved ? "success" : "failure");
            bool perfect = solved && gameplay.State.livesRemaining >= GameplayController.MaxLives;
            Label(hero.transform,perfect?"PERFECT SOLVE!":solved?"PUZZLE SOLVED!":"OUT OF "+CharacterCatalog.ResourceName(gameplay.ActiveCharacter).ToUpper(),26,TextAnchor.MiddleCenter,text,new Vector2(.05f,.05f),new Vector2(.95f,.22f));

            var rewardCard=Panel(bg.transform,card,new Vector2(.10f,.36f),new Vector2(.90f,.48f));
            if(solved)
            {
                Label(rewardCard.transform,"+"+reward.Treats+" TREATS",22,TextAnchor.MiddleLeft,new Color(.68f,.47f,.12f),new Vector2(.06f,.45f),new Vector2(.52f,.90f));
                Label(rewardCard.transform,"+"+reward.Coins+" COINS",22,TextAnchor.MiddleRight,new Color(.45f,.68f,.28f),new Vector2(.48f,.45f),new Vector2(.94f,.90f));
                Label(rewardCard.transform,(perfect?"NO MISTAKES  ·  ":"")+gameplay.State.livesRemaining+" "+CharacterCatalog.ResourceName(gameplay.ActiveCharacter)+" remaining",16,TextAnchor.MiddleCenter,new Color(.50f,.44f,.40f),new Vector2(.06f,.05f),new Vector2(.94f,.45f));
                Button(bg.transform,(dailyMode||goldenMode)?"BACK TO HOME":gameplay.LevelId<500?"NEXT LEVEL":"ALL LEVELS COMPLETE",28,new Color(.55f,.78f,.48f),()=>{
                    gameplay.Advance();
                    if(dailyMode||goldenMode){dailyMode=false;goldenMode=false;ShowHome();return;}
                    int next=LevelCatalog.NextLevel(gameplay.LevelId);
                    audioService.Play(SfxType.NextLevel);
                    if(next>0)StartLevel(next); else ShowHome();
                },new Vector2(.10f,.23f),new Vector2(.90f,.31f));
            }
            else
            {
                Label(rewardCard.transform,"No treats earned this attempt.",17,TextAnchor.MiddleCenter,new Color(.55f,.45f,.42f),new Vector2(.05f,.52f),new Vector2(.95f,.90f));
                Label(rewardCard.transform,"Choose one recovery, then return to the puzzle.",15,TextAnchor.MiddleCenter,new Color(.50f,.44f,.40f),new Vector2(.05f,.08f),new Vector2(.95f,.50f));
                if (dailyMode || goldenMode || !gameplay.RecoveryUsed)
                {
                    Button(bg.transform,(dailyMode||goldenMode)?"BACK TO HOME":"WATCH AD  +1 "+CharacterCatalog.ResourceName(gameplay.ActiveCharacter).ToUpper(),19,new Color(.55f,.78f,.48f),()=>{if(dailyMode||goldenMode)ShowHome();else TryRewardedBerry();},new Vector2(.08f,.14f),new Vector2(.48f,.21f));
                    Button(bg.transform,dailyMode||goldenMode?"BACK TO HOME":"BUY 1  ·  "+BerryReviveCost,19,new Color(.96f,.84f,.54f),()=>{if(dailyMode||goldenMode)ShowHome();else BuyBerryRevive();},new Vector2(.52f,.14f),new Vector2(.92f,.21f));
                }
                Button(bg.transform,(dailyMode||goldenMode)?"BACK TO HOME":"RETRY",20,card,()=>{if(dailyMode||goldenMode)ShowHome();else StartLevel(gameplay.LevelId);},new Vector2(.28f,.07f),new Vector2(.72f,.12f));
            }
        }

        private void TryRewardedBerry()
        {
            if(rewardedAds==null || !rewardedAds.IsReady)return;
            rewardedAds.ShowRewarded(success=>{
                if(!success)return;
                if(gameplay.ReviveWithBerry())BuildGameplay();
            });
        }

        private void BuyBerryRevive()
        {
            if(save.coins<BerryReviveCost)return;
            save.coins-=BerryReviveCost;
            SaveService.Save(save);
            if(gameplay.ReviveWithBerry())BuildGameplay();
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

    }
}
