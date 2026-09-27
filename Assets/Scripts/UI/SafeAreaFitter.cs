using UnityEngine;

namespace CapybaraGame.UI
{
    /// <summary>Keeps runtime UI inside device safe areas while preserving the 1080x1920 canvas.</summary>
    public sealed class SafeAreaFitter : MonoBehaviour
    {
        private RectTransform rect;
        private Rect lastSafeArea;
        private Vector2Int lastScreenSize;

        private void Awake()
        {
            rect = transform as RectTransform;
            Apply();
        }

        private void OnEnable() => Apply();

        private void Update()
        {
            if (Screen.safeArea != lastSafeArea ||
                new Vector2Int(Screen.width, Screen.height) != lastScreenSize)
                Apply();
        }

        private void Apply()
        {
            if (rect == null) return;

            Rect safe = Screen.safeArea;
            lastSafeArea = safe;
            lastScreenSize = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0) return;

            Vector2 anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            Vector2 anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);

            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }
    }
}