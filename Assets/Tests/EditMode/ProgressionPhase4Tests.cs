using System.Collections.Generic;
using CapybaraGame.Core;
using CapybaraGame.Levels;
using CapybaraGame.Progression;
using CapybaraGame.Gameplay;
using NUnit.Framework;

public sealed class ProgressionPhase4Tests
{
    private LocalSave NewSave()
    {
        return new LocalSave { unlockedLevel = 1, completedLevels = new List<int>(), coins = 0, treats = 0 };
    }

    [Test]
    public void LevelOneStartsUnlockedAndLevelTwoLocked()
    {
        var save = NewSave();
        Assert.IsTrue(ProgressionModel.IsUnlocked(save, 1));
        Assert.IsFalse(ProgressionModel.IsUnlocked(save, 2));
    }

    [Test]
    public void CompletingLevelUnlocksNextLevel()
    {
        var save = NewSave();
        ProgressionModel.ApplyCompletion(save, 1, RewardCalculator.CalculateCompletion(3, LevelRewardConfig.Default));
        Assert.IsTrue(ProgressionModel.IsCompleted(save, 1));
        Assert.IsTrue(ProgressionModel.IsUnlocked(save, 2));
    }

    [Test]
    public void LockedLevelCannotBecomeCompletedThroughNormalization()
    {
        var save = NewSave();
        save.completedLevels.Add(5);
        ProgressionModel.Normalize(save);
        Assert.IsTrue(ProgressionModel.IsCompleted(save, 5));
        Assert.AreEqual(1, save.unlockedLevel);
    }

    [Test]
    public void ReplayingCompletedLevelDoesNotDuplicateReward()
    {
        var save = NewSave();
        var reward = RewardCalculator.CalculateCompletion(3, LevelRewardConfig.Default);
        ProgressionModel.ApplyCompletion(save, 1, reward);
        int coins = save.coins;
        int treats = save.treats;
        ProgressionModel.ApplyCompletion(save, 1, reward);
        Assert.AreEqual(coins, save.coins);
        Assert.AreEqual(treats, save.treats);
    }

    [Test]
    public void LevelCatalogProvidesDeterministicDifficultyCurve()
    {
        Assert.AreEqual(4, LevelCatalog.Get(1).BoardSize);
        Assert.AreEqual(PuzzleDifficultyBand.Easy, LevelCatalog.Get(1).Difficulty);
        Assert.AreEqual(5, LevelCatalog.Get(51).BoardSize);
        Assert.AreEqual(PuzzleDifficultyBand.Medium, LevelCatalog.Get(51).Difficulty);
        Assert.AreEqual(6, LevelCatalog.Get(101).BoardSize);
        Assert.AreEqual(7, LevelCatalog.Get(401).BoardSize);
        Assert.IsTrue(LevelCatalog.Get(10).IsHardChallenge);
    }

    [Test]
    public void InvalidLevelIsRejected()
    {
        var save = NewSave();
        Assert.IsFalse(ProgressionModel.IsUnlocked(save, 0));
        Assert.IsFalse(ProgressionModel.IsUnlocked(save, LevelCatalog.MaxLevel + 1));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => LevelCatalog.Get(0));
    }
}