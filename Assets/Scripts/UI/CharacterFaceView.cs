using CapybaraGame.Core;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CapybaraGame.UI
{
    public sealed class CharacterFaceView : MonoBehaviour
    {
        private static Sprite circleSprite;
        private readonly List<GameObject> parts = new List<GameObject>();
        private CharacterId character;
        private bool reducedMotion;
        private bool built;

        private static Sprite CircleSprite
        {
            get
            {
                if (circleSprite != null) return circleSprite;
                const int size = 128;
                var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
                texture.filterMode = FilterMode.Bilinear;
                var pixels = new Color[size * size];
                float center = (size - 1) * .5f;
                float radius = size * .5f - 1f;
                for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - center, dy = y - center;
                    float d = Mathf.Sqrt(dx * dx + dy * dy);
                    float a = Mathf.Clamp01(radius + 1f - d);
                    pixels[y * size + x] = new Color(1f, 1f, 1f, a);
                }
                texture.SetPixels(pixels);
                texture.Apply();
                circleSprite = Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
                return circleSprite;
            }
        }

        public void Build(CharacterId id, bool reduced) => Configure(id, reduced, true);

        public void Configure(CharacterId id, bool reduced, bool forceRebuild = false)
        {
            character = id;
            reducedMotion = reduced;
            if (!forceRebuild && built) return;
            Clear();
            CreateFace();
            built = true;
        }

        public void SetVisible(bool visible) => gameObject.SetActive(visible);

        public void PlayPlacement()
        {
            if (reducedMotion) return;
            StopAllCoroutines();
            StartCoroutine(PopRoutine());
        }

        public void PlayError()
        {
            if (reducedMotion) return;
            StopAllCoroutines();
            StartCoroutine(ShakeRoutine());
        }

        private void Clear()
        {
            for (int i = parts.Count - 1; i >= 0; i--)
                if (parts[i] != null) Destroy(parts[i]);
            parts.Clear();
        }

        private Image Part(string name, Color color, Vector2 anchor, Vector2 size, float rotation = 0f)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(transform, false);
            var r = go.GetComponent<RectTransform>();
            r.anchorMin = r.anchorMax = anchor;
            r.pivot = new Vector2(.5f, .5f);
            r.sizeDelta = size;
            r.localRotation = Quaternion.Euler(0, 0, rotation);
            var image = go.GetComponent<Image>();
            image.sprite = CircleSprite;
            image.color = color;
            image.raycastTarget = false;
            parts.Add(go);
            return image;
        }

        private void CreateFace()
        {
            Color head, ear, inner;
            switch (character)
            {
                case CharacterId.Cat:
                    head = new Color(.94f,.72f,.54f); ear = new Color(.80f,.53f,.42f); inner = new Color(1f,.82f,.72f);
                    Part("EarL", ear, new Vector2(.25f,.78f), new Vector2(26,26), -18f);
                    Part("EarR", ear, new Vector2(.75f,.78f), new Vector2(26,26), 18f);
                    break;
                case CharacterId.Dog:
                    head = new Color(.78f,.59f,.43f); ear = new Color(.55f,.39f,.28f); inner = new Color(.93f,.76f,.60f);
                    Part("EarL", ear, new Vector2(.18f,.60f), new Vector2(34,48), -12f);
                    Part("EarR", ear, new Vector2(.82f,.60f), new Vector2(34,48), 12f);
                    break;
                case CharacterId.Penguin:
                    head = new Color(.20f,.27f,.31f); ear = head; inner = new Color(.97f,.96f,.91f);
                    break;
                case CharacterId.Panda:
                    head = new Color(.94f,.94f,.91f); ear = new Color(.16f,.15f,.14f); inner = new Color(.98f,.98f,.95f);
                    Part("EarL", ear, new Vector2(.23f,.82f), new Vector2(28,28));
                    Part("EarR", ear, new Vector2(.77f,.82f), new Vector2(28,28));
                    break;
                default:
                    head = new Color(.69f,.57f,.43f); ear = new Color(.57f,.45f,.33f); inner = new Color(.82f,.70f,.55f);
                    Part("EarL", ear, new Vector2(.24f,.80f), new Vector2(22,22));
                    Part("EarR", ear, new Vector2(.76f,.80f), new Vector2(22,22));
                    break;
            }

            Part("Head", head, new Vector2(.5f,.5f), new Vector2(82,82));
            if (character == CharacterId.Penguin) Part("FacePatch", inner, new Vector2(.5f,.48f), new Vector2(62,64));
            if (character == CharacterId.Panda)
            {
                Part("PatchL", ear, new Vector2(.37f,.57f), new Vector2(25,31), -20f);
                Part("PatchR", ear, new Vector2(.63f,.57f), new Vector2(25,31), 20f);
            }

            Color eye = new Color(.10f,.09f,.08f);
            Part("EyeL", eye, new Vector2(.38f,.59f), new Vector2(13,17));
            Part("EyeR", eye, new Vector2(.62f,.59f), new Vector2(13,17));
            Part("EyeGlintL", Color.white, new Vector2(.36f,.62f), new Vector2(4,4));
            Part("EyeGlintR", Color.white, new Vector2(.60f,.62f), new Vector2(4,4));
            if (character == CharacterId.Penguin) Part("Beak", new Color(1f,.63f,.27f), new Vector2(.5f,.44f), new Vector2(15,10));
            else Part("Nose", new Color(.24f,.14f,.11f), new Vector2(.5f,.45f), new Vector2(9,7));
            Part("BlushL", new Color(1f,.50f,.52f,.55f), new Vector2(.28f,.45f), new Vector2(16,9));
            Part("BlushR", new Color(1f,.50f,.52f,.55f), new Vector2(.72f,.45f), new Vector2(16,9));
            Part("Mouth", new Color(.24f,.14f,.13f), new Vector2(.5f,.37f), new Vector2(13,5));
        }

        private System.Collections.IEnumerator PopRoutine()
        {
            var r = transform as RectTransform;
            Vector3 start = r.localScale;
            r.localScale = start * .72f;
            float t = 0f;
            while (t < .22f)
            {
                t += Time.unscaledDeltaTime;
                r.localScale = start * Mathf.Lerp(.72f, 1.06f, Mathf.Clamp01(t / .22f));
                yield return null;
            }
            r.localScale = start;
        }

        private System.Collections.IEnumerator ShakeRoutine()
        {
            var r = transform as RectTransform;
            Vector2 start = r.anchoredPosition;
            float t = 0f;
            while (t < .18f)
            {
                t += Time.unscaledDeltaTime;
                float p = 1f - Mathf.Clamp01(t / .18f);
                r.anchoredPosition = start + new Vector2(Mathf.Sin(t * 100f) * 5f * p, 0f);
                yield return null;
            }
            r.anchoredPosition = start;
        }
    }
}