#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace CapybaraGame.Editor
{
    public sealed class Phase3DebugWindow : EditorWindow
    {
        private int level = 1;
        private int coins = 750;

        [MenuItem("Capybara/Phase 3 Debug")]
        private static void Open() => GetWindow<Phase3DebugWindow>("Phase 3 Debug");

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Core Gameplay Debug", EditorStyles.boldLabel);
            level = EditorGUILayout.IntSlider("Jump to level", level, 1, 500);
            coins = EditorGUILayout.IntField("Coins", coins);

            if (GUILayout.Button("Jump to Level"))
                FindFirstObjectByType<GameRoot>()?.DebugStartLevel(level);

            if (GUILayout.Button("Set Coins"))
                FindFirstObjectByType<GameRoot>()?.DebugSetCoins(coins);

            if (GUILayout.Button("Reset Save"))
            {
                if (EditorUtility.DisplayDialog("Reset Save", "Delete local Capybara progression?", "Reset", "Cancel"))
                    FindFirstObjectByType<GameRoot>()?.DebugResetSave();
            }
        }
    }
}
#endif
