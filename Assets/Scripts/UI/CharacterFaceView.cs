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
        private Image eyeL;
        private Image eyeR;
        private Image earL;
        private Image earR;
        private RectTransform headTransform;
        private RectTransform leftCheekTransform;
        private RectTransform rightCheekTransform;
        private Coroutine idleRoutine;

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
            bool changed = !built || character != id || reducedMotion != reduced;
            character = id;
            reducedMotion = reduced;
            if (!forceRebuild && built && !changed) return;
            Clear();
            CreateFace();
            built = true;
            if (!reducedMotion)
            {
                if (idleRoutine != null) StopCoroutine(idleRoutine);
                idleRoutine = StartCoroutine(IdleRoutine());
            }
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
            eyeL = null; eyeR = null; earL = null; earR = null;
            headTransform = null; leftCheekTransform = null; rightCheekTransform = null;
            switch (character)
            {
                case CharacterId.Cat:
                    head = new Color(.94f,.72f,.54f); ear = new Color(.80f,.53f,.42f); inner = new Color(1f,.82f,.72f);
                    earL = Part("EarL", ear, new Vector2(.25f,.78f), new Vector2(26,26), -18f);
                    earR = Part("EarR", ear, new Vector2(.75f,.78f), new Vector2(26,26), 18f);
                    break;
                case CharacterId.Dog:
                    head = new Color(.78f,.59f,.43f); ear = new Color(.55f,.39f,.28f); inner = new Color(.93f,.76f,.60f);
                    earL = Part("EarL", ear, new Vector2(.18f,.60f), new Vector2(34,48), -12f);
                    earR = Part("EarR", ear, new Vector2(.82f,.60f), new Vector2(34,48), 12f);
                    break;
                case CharacterId.Penguin:
                    head = new Color(.20f,.27f,.31f); ear = head; inner = new Color(.97f,.96f,.91f);
                    break;
                case CharacterId.Panda:
                    head = new Color(.94f,.94f,.91f); ear = new Color(.16f,.15f,.14f); inner = new Color(.98f,.98f,.95f);
                    earL = Part("EarL", ear, new Vector2(.23f,.82f), new Vector2(28,28));
                    earR = Part("EarR", ear, new Vector2(.77f,.82f), new Vector2(28,28));
                    break;
                default:
                    head = new Color(.69f,.57f,.43f); ear = new Color(.57f,.45f,.33f); inner = new Color(.82f,.70f,.55f);
                    earL = Part("EarL", ear, new Vector2(.24f,.80f), new Vector2(22,22));
                    earR = Part("EarR", ear, new Vector2(.76f,.80f), new Vector2(22,22));
                    break;
            }

            var headImage = Part("Head", head, new Vector2(.5f,.5f), new Vector2(82,82));
            headTransform = headImage.rectTransform;
            if (character == CharacterId.Penguin) Part("FacePatch", inner, new Vector2(.5f,.48f), new Vector2(62,64));
            if (character == CharacterId.Panda)
            {
                Part("PatchL", ear, new Vector2(.37f,.57f), new Vector2(25,31), -20f);
                Part("PatchR", ear, new Vector2(.63f,.57f), new Vector2(25,31), 20f);
            }

            Color eye = new Color(.10f,.09f,.08f);
            eyeL = Part("EyeL", eye, new Vector2(.38f,.59f), new Vector2(13,17));
            eyeR = Part("EyeR", eye, new Vector2(.62f,.59f), new Vector2(13,17));
            Part("EyeGlintL", Color.white, new Vector2(.36f,.62f), new Vector2(4,4));
            Part("EyeGlintR", Color.white, new Vector2(.60f,.62f), new Vector2(4,4));
            if (character == CharacterId.Penguin) Part("Beak", new Color(1f,.63f,.27f), new Vector2(.5f,.44f), new Vector2(15,10));
            else Part("Nose", new Color(.24f,.14f,.11f), new Vector2(.5f,.45f), new Vector2(9,7));
            var blushL = Part("BlushL", new Color(1f,.50f,.52f,.55f), new Vector2(.28f,.45f), new Vector2(16,9));
            var blushR = Part("BlushR", new Color(1f,.50f,.52f,.55f), new Vector2(.72f,.45f), new Vector2(16,9));
            leftCheekTransform = blushL.rectTransform;
            rightCheekTransform = blushR.rectTransform;
            Part("Mouth", new Color(.24f,.14f,.13f), new Vector2(.5f,.37f), new Vector2(13,5));
        }


        private System.Collections.IEnumerator IdleRoutine()
        {
            float phase = Random.Range(0f, 6.28f);
            while (built && !reducedMotion)
            {
                float duration = Random.Range(2.4f, 3.8f);
                float elapsed = 0f;
                var root = transform as RectTransform;
                Vector2 start = root != null ? root.anchoredPosition : Vector2.zero;
                float bobAmount = character == CharacterId.Capybara ? 1.8f : 1.2f;
                while (elapsed < duration)
                {
                    elapsed += Time.unscaledDeltaTime;
                    phase += Time.unscaledDeltaTime * 2.0f;
                    float bob = Mathf.Sin(phase) * bobAmount;
                    if (root != null) root.anchoredPosition = start + new Vector2(0f, bob);
                    if (headTransform != null)
                        headTransform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(phase * .55f) * .7f);
                    if (leftCheekTransform != null && rightCheekTransform != null)
                    {
                        float cheek = 1f + Mathf.Sin(phase) * .025f;
                        leftCheekTransform.localScale = Vector3.one * cheek;
                        rightCheekTransform.localScale = Vector3.one * cheek;
                    }
                    yield return null;
                }
                yield return BlinkRoutine();
                if (Random.value < .22f) yield return BlinkRoutine();
                if (Random.value < .35f && earL != null && earR != null) yield return EarWiggleRoutine();
            }
        }

        private System.Collections.IEnumerator BlinkRoutine()
        {
            if (eyeL == null || eyeR == null) yield break;
            Vector2 leftSize = eyeL.rectTransform.sizeDelta;
            Vector2 rightSize = eyeR.rectTransform.sizeDelta;
            float t = 0f;
            while (t < .07f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / .07f);
                eyeL.rectTransform.sizeDelta = new Vector2(leftSize.x, Mathf.Lerp(leftSize.y, 2.5f, p));
                eyeR.rectTransform.sizeDelta = new Vector2(rightSize.x, Mathf.Lerp(rightSize.y, 2.5f, p));
                yield return null;
            }
            t = 0f;
            while (t < .09f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / .09f);
                eyeL.rectTransform.sizeDelta = new Vector2(leftSize.x, Mathf.Lerp(2.5f, leftSize.y, p));
                eyeR.rectTransform.sizeDelta = new Vector2(rightSize.x, Mathf.Lerp(2.5f, rightSize.y, p));
                yield return null;
            }
        }

        private System.Collections.IEnumerator EarWiggleRoutine()
        {
            if (earL == null || earR == null) yield break;
            float leftStart = earL.rectTransform.localEulerAngles.z;
            float rightStart = earR.rectTransform.localEulerAngles.z;
            if (leftStart > 180f) leftStart -= 360f;
            if (rightStart > 180f) rightStart -= 360f;
            float t = 0f;
            while (t < .28f)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / .28f);
                float wiggle = Mathf.Sin(p * Mathf.PI * 2f) * 5f * (1f - p);
                earL.rectTransform.localRotation = Quaternion.Euler(0f, 0f, leftStart + wiggle);
                earR.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rightStart - wiggle);
                yield return null;
            }
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