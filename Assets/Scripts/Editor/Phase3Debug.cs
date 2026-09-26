#if UNITY_EDITOR || DEVELOPMENT_BUILD
using CapybaraGame.Characters;
using CapybaraGame.Services;
using UnityEngine;

namespace CapybaraGame
{
    public sealed partial class GameRoot
    {
        public void DebugStartLevel(int level)
        {
            level = Mathf.Clamp(level, 1, 500);
            save.unlockedLevel = Mathf.Max(save.unlockedLevel, level);
            SaveService.Save(save);
            StartLevel(level);
        }

        public void DebugSetCoins(int amount)
        {
            save.coins = Mathf.Max(0, amount);
            SaveService.Save(save);
        }

        public void DebugResetSave()
        {
            SaveService.Reset();
            save = SaveService.Load();
            characters = new CharacterSystem(save);
            ShowHome();
        }
    }
}
#endif