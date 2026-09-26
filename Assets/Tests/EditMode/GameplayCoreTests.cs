using CapybaraGame.Core;
using CapybaraGame.Gameplay;
using NUnit.Framework;

public sealed class GameplayCoreTests
{
    private static PuzzleDefinition FourByFour()
    {
        return new PuzzleDefinition
        {
            id = "TEST-4",
            rows = 4,
            columns = 4,
            regionCount = 4,
            regions = new[] { 0, 0, 1, 1, 0, 0, 1, 1, 2, 2, 3, 3, 2, 2, 3, 3 },
            solution = new[] { 1, 3, 0, 2 },
            initialState = new int[16]
        };
    }

    [Test]
    public void NewPuzzleStartsWithThreeLives()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        Assert.AreEqual(3, controller.State.livesRemaining);
        Assert.AreEqual(GameplayState.Playing, controller.CurrentState);
    }

    [Test]
    public void CorrectMovePlacesCharacterWithoutLosingLife()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        controller.TapCell(0, 1);
        Assert.AreEqual(0, controller.State.placed[1]);
        Assert.AreEqual(3, controller.State.livesRemaining);
    }

    [Test]
    public void IncorrectMoveConsumesExactlyOneLifeAndDoesNotPlace()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        controller.TapCell(0, 0);
        Assert.AreEqual(2, controller.State.livesRemaining);
        Assert.AreEqual(-1, controller.State.placed[0]);
        Assert.AreEqual(GameplayState.Playing, controller.CurrentState);
    }

    [Test]
    public void OccupiedCellRemovesWithoutLosingLife()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        controller.TapCell(0, 1);
        controller.TapCell(0, 1);
        Assert.AreEqual(-1, controller.State.placed[1]);
        Assert.AreEqual(3, controller.State.livesRemaining);
    }

    [Test]
    public void ThreeInvalidMovesFailPuzzleAndGiveZeroReward()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        controller.TapCell(0, 0);
        controller.TapCell(0, 0);
        controller.TapCell(0, 0);
        Assert.AreEqual(GameplayState.Failed, controller.CurrentState);
        Assert.AreEqual(0, controller.State.livesRemaining);
        Assert.AreEqual(0, RewardCalculator.CalculateFailure().Treats);
        Assert.AreEqual(0, RewardCalculator.CalculateFailure().Coins);
    }

    [Test]
    public void SolvingUsesRemainingLivesAsTreatReward()
    {
        var controller = new GameplayController();
        RewardResult reward = default;
        controller.Completed += r => reward = r;
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        var puzzle = controller.Puzzle;
        for (int row = 0; row < 4; row++) controller.TapCell(row, puzzle.solution[row]);
        Assert.AreEqual(GameplayState.Completed, controller.CurrentState);
        Assert.AreEqual(3, reward.Treats);
        Assert.AreEqual(150, reward.Coins);
    }

    [Test]
    public void RewardCalculatorUsesRemainingLives()
    {
        Assert.AreEqual(3, RewardCalculator.CalculateCompletion(3, LevelRewardConfig.Default).Treats);
        Assert.AreEqual(2, RewardCalculator.CalculateCompletion(2, LevelRewardConfig.Default).Treats);
        Assert.AreEqual(1, RewardCalculator.CalculateCompletion(1, LevelRewardConfig.Default).Treats);
        Assert.AreEqual(0, RewardCalculator.CalculateFailure().Treats);
    }


    [Test]
    public void PauseStopsGameplayAndResumeRestoresIt()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        controller.Pause();
        Assert.AreEqual(GameplayState.Paused, controller.CurrentState);
        Assert.AreEqual(0f, UnityEngine.Time.timeScale);
        controller.Resume();
        Assert.AreEqual(GameplayState.Playing, controller.CurrentState);
        Assert.AreEqual(1f, UnityEngine.Time.timeScale);
    }

    [Test]
    public void RestartResetsPuzzleAndLives()
    {
        var controller = new GameplayController();
        controller.LoadPuzzle(FourByFour(), 1, CharacterId.Capybara);
        controller.TapCell(0, 1);
        controller.TapCell(0, 0);
        controller.Restart();
        Assert.AreEqual(3, controller.State.livesRemaining);
        Assert.AreEqual(-1, controller.State.placed[1]);
        Assert.AreEqual(GameplayState.Playing, controller.CurrentState);
    }

    [Test]
    public void PuzzleStateUsesDynamicCellCount()
    {
        var puzzle = FourByFour();
        var controller = new GameplayController();
        controller.LoadPuzzle(puzzle, 1, CharacterId.Capybara);
        Assert.AreEqual(16, controller.State.placed.Length);
    }
}