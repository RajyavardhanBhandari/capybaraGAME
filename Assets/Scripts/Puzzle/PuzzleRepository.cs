using CapybaraGame.Core;
using CapybaraGame.Puzzle;
using NUnit.Framework;

public class PuzzleValidatorTests
{
    private PuzzleDefinition Puzzle()
    {
        var p = PuzzleRepository.Get(1);
        Assert.IsTrue(PuzzleValidator.IsSolved(p, BuildSolution(p)));
        return p;
    }

    private int[] BuildSolution(PuzzleDefinition p)
    {
        var placed = new int[100];
        for (int i=0;i<100;i++) placed[i] = -1;
        for (int r=0;r<10;r++) placed[r*10+p.solution[r]] = 0;
        return placed;
    }

    [Test] public void GeneratedPuzzleHasValidSolution() => Assert.IsTrue(PuzzleValidator.IsSolved(Puzzle(), BuildSolution(Puzzle())));

    [Test]
    public void DuplicateRowRejected()
    {
        var p = Puzzle();
        var placed = new int[100]; for(int i=0;i<100;i++) placed[i]=-1;
        placed[0] = 0;
        Assert.IsFalse(PuzzleValidator.IsPlacementValid(p, placed, 0, 2));
    }

    [Test]
    public void DiagonalTouchRejected()
    {
        var p = Puzzle();
        var placed = new int[100]; for(int i=0;i<100;i++) placed[i]=-1;
        placed[0] = 0;
        Assert.IsFalse(PuzzleValidator.IsPlacementValid(p, placed, 1, 1));
    }

    [Test]
    public void ValidSolutionContainsTenCharacters()
    {
        var p = Puzzle();
        int count=0;
        var placed=BuildSolution(p);
        foreach(var v in placed) if(v!=-1) count++;
        Assert.AreEqual(10,count);
    }
}
