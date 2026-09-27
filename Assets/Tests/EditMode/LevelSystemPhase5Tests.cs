using System.Collections.Generic;
using NUnit.Framework;
using CapybaraGame.Core;
using CapybaraGame.Levels;
using CapybaraGame.Puzzle;

public sealed class LevelSystemPhase5Tests
{
    [Test]
    public void LevelOneAndFiveHundredExist()
    {
        Assert.AreEqual(1, LevelCatalog.Get(1).Id);
        Assert.AreEqual(500, LevelCatalog.Get(500).Id);
    }

    [Test]
    public void LevelIdsAreStableAndInvalidIdsFail()
    {
        Assert.AreEqual(127, LevelCatalog.Get(127).Id);
        Assert.Throws<System.ArgumentOutOfRangeException>(() => LevelCatalog.Get(0));
        Assert.Throws<System.ArgumentOutOfRangeException>(() => LevelCatalog.Get(501));
    }

    [Test]
    public void ExactlyFiftyHardChallengesExist()
    {
        int count = 0;
        for (int level = 1; level <= 500; level++)
            if (LevelCatalog.Get(level).IsHardChallenge) count++;

        Assert.AreEqual(50, count);
        for (int level = 1; level <= 500; level++)
            Assert.AreEqual(level % 10 == 0, LevelCatalog.Get(level).IsHardChallenge);
    }

    [Test]
    public void HardChallengeIndicesAreOneThroughFifty()
    {
        for (int index = 1; index <= 50; index++)
            Assert.AreEqual(index, LevelCatalog.Get(index * 10).HardChallengeIndex);

        for (int level = 1; level <= 500; level++)
            if (level % 10 != 0)
                Assert.AreEqual(0, LevelCatalog.Get(level).HardChallengeIndex);
    }

    [Test]
    public void ProgressionBandsCoverTheLaunchSet()
    {
        Assert.AreEqual(1, LevelCatalog.Get(1).ProgressionBand);
        Assert.AreEqual(1, LevelCatalog.Get(100).ProgressionBand);
        Assert.AreEqual(2, LevelCatalog.Get(101).ProgressionBand);
        Assert.AreEqual(3, LevelCatalog.Get(201).ProgressionBand);
        Assert.AreEqual(4, LevelCatalog.Get(301).ProgressionBand);
        Assert.AreEqual(5, LevelCatalog.Get(401).ProgressionBand);
        Assert.AreEqual(5, LevelCatalog.Get(500).ProgressionBand);
    }

    [Test]
    public void LevelSelectionIsDeterministic()
    {
        var a = LevelCatalog.Get(273);
        LevelCatalog.ClearCache();
        var b = LevelCatalog.Get(273);

        Assert.AreEqual(a.Id, b.Id);
        Assert.AreEqual(a.Seed, b.Seed);
        Assert.AreEqual(a.BoardSize, b.BoardSize);
        Assert.AreEqual(a.Style, b.Style);
        Assert.AreEqual(a.Difficulty, b.Difficulty);
        Assert.AreEqual(a.IsHardChallenge, b.IsHardChallenge);
    }

    [Test]
    public void LevelDatabaseRejectsMissingOrDuplicateLevels()
    {
        var db = new LevelDatabase();
        db.levels.Add(LevelCatalog.Get(1));
        db.levels.Add(LevelCatalog.Get(2));

        Assert.IsFalse(db.IsComplete(3));

        db.levels.Add(LevelCatalog.Get(2));
        Assert.IsFalse(db.IsComplete(3));
    }

    [Test]
    public void StructuralFingerprintDistanceIsZeroForClone()
    {
        var puzzle = PuzzleGenerator.Generate(new PuzzleGenerationConfig
        {
            rows = 4, columns = 4, regionCount = 4, seed = 9182, maxAttempts = 2000
        }).Puzzle;

        Assert.IsNotNull(puzzle);
        Assert.AreEqual(0, PuzzleFingerprint.StructuralDistance(puzzle, puzzle.Clone()));
    }

    [Test]
    public void ProductionLevelCanBeLoadedThroughLevelSystem()
    {
        var puzzle = ProductionPuzzleRepository.Get(127);
        Assert.IsNotNull(puzzle);
        Assert.AreEqual(127, LevelCatalog.Get(127).Id);
        Assert.IsTrue(PuzzleValidator.IsRegionMapValid(puzzle));
        Assert.AreEqual(1, PuzzleSolver.Solve(puzzle, 2).SolutionCount);
    }
}