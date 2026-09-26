using CapybaraGame.Core;
using CapybaraGame.Progression;

namespace CapybaraGame.Gameplay
{
    // Compatibility facade retained for existing callers. Phase 4 owns progression in ProgressionModel.
    public static class ProgressionService
    {
        public static bool IsUnlocked(LocalSave save, int level) => ProgressionModel.IsUnlocked(save, level);
        public static bool HasCompleted(LocalSave save, int level) => ProgressionModel.IsCompleted(save, level);
        public static void ApplyCompletion(LocalSave save, int level, RewardResult reward) => ProgressionModel.ApplyCompletion(save, level, reward);
    }
}