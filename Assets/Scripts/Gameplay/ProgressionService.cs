using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Services;

namespace CapybaraGame.Gameplay
{
    public static class ProgressionService
    {
        public static bool IsUnlocked(LocalSave save, int level)
            => save != null && level >= 1 && level <= save.unlockedLevel;

        public static bool HasCompleted(LocalSave save, int level)
            => save != null && save.completedLevels != null && save.completedLevels.Contains(level);

        public static void ApplyCompletion(LocalSave save, int level, RewardResult reward)
        {
            if (save == null) return;
            if (save.completedLevels == null) save.completedLevels = new List<int>();
            if (!save.completedLevels.Contains(level)) save.completedLevels.Add(level);
            if (level >= save.unlockedLevel && level < Puzzle.PuzzleRepository.LaunchLevelCount)
                save.unlockedLevel = level + 1;
            save.coins += reward.Coins;
            save.treats += reward.Treats;
            SaveService.Save(save);
        }
    }
}