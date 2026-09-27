using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace CapybaraGame.UI
{
    /// <summary>Soft screen entrance motion used by every full-screen runtime panel.</summary>
    public sealed class ScreenPanelAnimator : MonoBehaviour
    {
        private CanvasGroup canvasGroup;
        private RectTransform rect;

        public void Play(bool reducedMotion)
        {
            if (canvasGroup == null) canvasGroup = gameObject.GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
            if (rect == null) rect = transform as RectTransform;

            StopAllCoroutines();
            if (reducedMotion)
            {
                canvasGroup.alpha = 1f;
                if (rect != null) rect.localScale = Vector3.one;
                return;
            }

            StartCoroutine(AnimateIn());
        }

        private IEnumerator AnimateIn()
        {
            canvasGroup.alpha = 0f;
            if (rect != null) rect.localScale = Vector3.one * .985f;

            float t = 0f;
            const float duration = .22f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float p = Mathf.Clamp01(t / duration);
                float eased = 1f - Mathf.Pow(1f - p, 3f);
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, eased);
                if (rect != null)
                {
                    float scale = Mathf.Lerp(.985f, 1.0f, eased);
                    rect.localScale = Vector3.one * scale;
                }
                yield return null;
            }

            canvasGroup.alpha = 1f;
            if (rect != null) rect.localScale = Vector3.one;
        }
    }
}