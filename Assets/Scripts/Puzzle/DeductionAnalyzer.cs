using System;
using System.Collections.Generic;
using CapybaraGame.Core;

namespace CapybaraGame.Puzzle
{
    public enum DeductionKind { Region, Row, Column, Adjacency, Chain }

    public sealed class DeductionStep
    {
        public int Row;
        public int Column;
        public DeductionKind Kind;
        public int CandidateCount;
        public string Message;
    }

    /// <summary>Explains the next deterministic consequence of the current board without changing puzzle rules.</summary>
    public static class DeductionAnalyzer
    {
        public static List<DeductionStep> Analyze(PuzzleDefinition puzzle, PuzzleState state, int maxSteps = 3)
        {
            var result = new List<DeductionStep>();
            if (puzzle == null || state == null || puzzle.solution == null) return result;

            for (int r = 0; r < puzzle.rows && result.Count < maxSteps; r++)
            {
                int solutionColumn = puzzle.solution[r];
                int index = r * puzzle.columns + solutionColumn;
                if (state.placed[index] != -1) continue;

                int candidates = 0;
                for (int c = 0; c < puzzle.columns; c++)
                    if (state.placed[r * puzzle.columns + c] == -1 &&
                        PuzzleValidator.IsPlacementValid(puzzle, state.placed, r, c))
                        candidates++;

                if (candidates == 1)
                {
                    result.Add(new DeductionStep
                    {
                        Row = r,
                        Column = solutionColumn,
                        Kind = DeductionKind.Chain,
                        CandidateCount = 1,
                        Message = "Only one cell in this row can hold the character."
                    });
                }
            }

            if (result.Count == 0)
            {
                for (int r = 0; r < puzzle.rows && result.Count < maxSteps; r++)
                for (int c = 0; c < puzzle.columns && result.Count < maxSteps; c++)
                {
                    int i = r * puzzle.columns + c;
                    if (state.placed[i] != -1) continue;
                    if (puzzle.solution[r] != c) continue;

                    int region = puzzle.RegionAt(r, c);
                    bool regionOccupied = false;
                    for (int rr = 0; rr < puzzle.rows && !regionOccupied; rr++)
                    for (int cc = 0; cc < puzzle.columns; cc++)
                        if (state.placed[rr * puzzle.columns + cc] != -1 &&
                            puzzle.RegionAt(rr, cc) == region) { regionOccupied = true; break; }

                    if (!regionOccupied)
                    {
                        result.Add(new DeductionStep
                        {
                            Row = r, Column = c, Kind = DeductionKind.Region,
                            CandidateCount = 1,
                            Message = "This region still needs its character."
                        });
                        break;
                    }
                }
            }

            return result;
        }
    }
}