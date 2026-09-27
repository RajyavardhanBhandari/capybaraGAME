using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

namespace CapybaraGame.UI
{
    public sealed class UIInteractionFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public bool reducedMotion;
        private RectTransform rect;
        private Vector3 baseScale;

        private void Awake()
        {
            rect = transform as RectTransform;
            baseScale = rect != null ? rect.localScale : Vector3.one;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (reducedMotion || rect == null) return;
            StopAllCoroutines();
            StartCoroutine(ScaleTo(baseScale * .965f, .07f));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (reducedMotion || rect == null) return;
            StopAllCoroutines();
            StartCoroutine(ScaleTo(baseScale, .10f));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (reducedMotion || rect == null) return;
            StopAllCoroutines();
            StartCoroutine(ScaleTo(baseScale, .08f));
        }

        private IEnumerator ScaleTo(Vector3 target, float duration)
        {
            Vector3 start = rect.localScale;
            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                rect.localScale = Vector3.Lerp(start, target, Mathf.Clamp01(t / duration));
                yield return null;
            }
            rect.localScale = target;
        }
    }
}