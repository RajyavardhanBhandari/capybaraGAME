using System.Collections.Generic;
using CapybaraGame.Core;

namespace CapybaraGame.Puzzle
{
    public static class PuzzleValidator
    {
        public static bool IsPlacementValid(PuzzleDefinition puzzle, int[] placed, int row, int col)
        {
            if (row < 0 || row >= puzzle.rows || col < 0 || col >= puzzle.columns) return false;
            int index = row * puzzle.columns + col;
            if (placed[index] != -1) return false;

            int region = puzzle.RegionAt(row, col);

            for (int r = 0; r < puzzle.rows; r++)
            {
                for (int c = 0; c < puzzle.columns; c++)
                {
                    int existing = placed[r * puzzle.columns + c];
                    if (existing == -1) continue;

                    if (r == row || c == col) return false;
                    if (puzzle.RegionAt(r, c) == region) return false;

                    if (System.Math.Abs(r - row) <= 1 && System.Math.Abs(c - col) <= 1)
                        return false;
                }
            }

            return true;
        }

        public static bool IsSolved(PuzzleDefinition puzzle, int[] placed)
        {
            int count = 0;
            var regions = new HashSet<int>();
            var rows = new HashSet<int>();
            var cols = new HashSet<int>();

            for (int r = 0; r < puzzle.rows; r++)
            {
                for (int c = 0; c < puzzle.columns; c++)
                {
                    int idx = r * puzzle.columns + c;
                    if (placed[idx] == -1) continue;

                    count++;
                    if (!regions.Add(puzzle.RegionAt(r, c))) return false;
                    if (!rows.Add(r)) return false;
                    if (!cols.Add(c)) return false;

                    for (int rr = r - 1; rr <= r + 1; rr++)
                    {
                        for (int cc = c - 1; cc <= c + 1; cc++)
                        {
                            if (rr < 0 || rr >= puzzle.rows || cc < 0 || cc >= puzzle.columns) continue;
                            if (rr == r && cc == c) continue;
                            if (placed[rr * puzzle.columns + cc] != -1) return false;
                        }
                    }
                }
            }

            return count == puzzle.rows &&
                   regions.Count == puzzle.rows &&
                   rows.Count == puzzle.rows &&
                   cols.Count == puzzle.columns;
        }
    }
}
