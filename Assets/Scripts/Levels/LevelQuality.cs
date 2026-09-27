using CapybaraGame.Puzzle;

namespace CapybaraGame.Levels
{
    public sealed class LevelQuality
    {
        public DifficultyMetrics Metrics;
        public bool IsBreather;
        public bool IsHardChallenge;
        public bool IsTeaching;
        public bool DailyReady;
        public float FingerprintDistance;

        public string ExperienceLabel
        {
            get
            {
                if (IsTeaching) return "TEACHING";
                if (IsBreather) return "BREATHER";
                if (IsHardChallenge) return "HARD CHALLENGE";
                if (Metrics != null && Metrics.DeductionDepth > .70f) return "DEEP LOGIC";
                if (Metrics != null && Metrics.TopologyComplexity > .60f) return "REGION PUZZLE";
                return "BALANCED";
            }
        }
    }
}