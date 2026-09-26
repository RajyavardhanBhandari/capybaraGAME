using System;
using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Levels;

namespace CapybaraGame.Progression
{
    [Serializable]
    public sealed class LevelProgress
    {
        public int LevelId;
        public bool Completed;
        public int BestTreats;
        public int BestCoins;
        public int Attempts;

        public LevelProgress Clone() => new LevelProgress
        {
            LevelId = LevelId, Completed = Completed, BestTreats = BestTreats,
            BestCoins = BestCoins, Attempts = Attempts
        };
    }

    public static class ProgressionModel
    {
        public static bool IsUnlocked(LocalSave save, int level)
            => save != null && level >= 1 && level <= LevelCatalog.MaxLevel && level <= save.unlockedLevel;

        public static bool IsCompleted(LocalSave save, int level)
            => save != null && save.completedLevels != null && save.completedLevels.Contains(level);

        public static int GetNextAvailableLevel(LocalSave save)
            => save == null ? 1 : Math.Max(1, Math.Min(LevelCatalog.MaxLevel, save.unlockedLevel));

        public static int GetCompletedCount(LocalSave save)
            => save == null || save.completedLevels == null ? 0 : save.completedLevels.Count;

        public static void Normalize(LocalSave save)
        {
            if (save == null) return;
            save.unlockedLevel = Math.Max(1, Math.Min(LevelCatalog.MaxLevel, save.unlockedLevel));
            if (save.completedLevels == null) save.completedLevels = new List<int>();
            var valid = new HashSet<int>();
            for (int i = 0; i < save.completedLevels.Count; i++)
            {
                int level = save.completedLevels[i];
                if (level >= 1 && level <= LevelCatalog.MaxLevel) valid.Add(level);
            }
            save.completedLevels.Clear();
            foreach (int level in valid) save.completedLevels.Add(level);
            save.completedLevels.Sort();
            int highestCompleted = 0;
            for (int i = 1; i <= LevelCatalog.MaxLevel; i++)
            {
                if (!valid.Contains(i)) break;
                highestCompleted = i;
            }
            save.unlockedLevel = Math.Max(save.unlockedLevel, Math.Min(LevelCatalog.MaxLevel, highestCompleted + 1));
        }

        public static void ApplyCompletion(LocalSave save, int level, RewardResult reward)
        {
            if (save == null || level < 1 || level > LevelCatalog.MaxLevel) return;
            if (save.completedLevels == null) save.completedLevels = new List<int>();
            bool firstCompletion = !save.completedLevels.Contains(level);
            if (firstCompletion) save.completedLevels.Add(level);

            if (level == save.unlockedLevel && level < LevelCatalog.MaxLevel)
                save.unlockedLevel = level + 1;

            // Rewards are granted once per level. Replays never duplicate permanent currency.
            if (firstCompletion)
            {
                save.coins += reward.Coins;
                save.treats += reward.Treats;
            }
            Normalize(save);
        }
    }
}