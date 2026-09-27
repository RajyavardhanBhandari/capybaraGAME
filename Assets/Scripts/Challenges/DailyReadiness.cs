using CapybaraGame.Puzzle;

namespace CapybaraGame.Challenges
{
    public sealed class DailyReadiness
    {
        public bool Ready;
        public int LevelId;
        public string Fingerprint;
        public float Difficulty;
        public string Reason;
    }

    public static class DailyReadinessService
    {
        public static DailyReadiness Evaluate(string dayId)
        {
            int level = ChallengeService.DailyLevelId(dayId);
            var puzzle = ProductionPuzzleRepository.Get(level);
            if (puzzle == null)
                return new DailyReadiness { Ready = false, LevelId = level, Reason = "Puzzle unavailable." };

            var metrics = PuzzleDifficultyEvaluator.Evaluate(puzzle);
            bool ready = metrics.Band != Core.PuzzleDifficultyBand.Hard || puzzle.isHardChallenge;
            return new DailyReadiness
            {
                Ready = ready,
                LevelId = level,
                Fingerprint = PuzzleFingerprint.Compute(puzzle),
                Difficulty = metrics.Score,
                Reason = ready ? "Ready for the shared daily puzzle." : "Difficulty requires regeneration."
            };
        }
    }
}