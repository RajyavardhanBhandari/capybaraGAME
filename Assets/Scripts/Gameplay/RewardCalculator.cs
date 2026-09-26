namespace CapybaraGame.Gameplay
{
    public readonly struct RewardResult
    {
        public readonly int Treats;
        public readonly int Coins;

        public RewardResult(int treats, int coins)
        {
            Treats = treats;
            Coins = coins;
        }
    }

    public sealed class LevelRewardConfig
    {
        public int completionCoins = 150;
        public int perfectBonusCoins = 0;

        public static LevelRewardConfig Default => new LevelRewardConfig();
    }

    public static class RewardCalculator
    {
        public static RewardResult CalculateCompletion(int remainingLives, LevelRewardConfig config)
        {
            if (config == null) config = LevelRewardConfig.Default;
            int lives = remainingLives < 0 ? 0 : remainingLives > 3 ? 3 : remainingLives;
            return new RewardResult(lives, config.completionCoins + (lives == 3 ? config.perfectBonusCoins : 0));
        }

        public static RewardResult CalculateFailure() => new RewardResult(0, 0);
    }
}