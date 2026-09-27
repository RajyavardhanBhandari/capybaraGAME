using System;
using System.Security.Cryptography;
using System.Text;
using CapybaraGame.Core;

namespace CapybaraGame.Puzzle
{
    public static class PuzzleFingerprint
    {
        public static string Compute(PuzzleDefinition p)
        {
            var sizes = Sizes(p);
            Array.Sort(sizes);
            int boundary = BoundaryCount(p);
            int symmetry = SymmetryScore(p);
            string raw = p.rows + "x" + p.columns + "|" +
                         string.Join(",", sizes) + "|" + boundary + "|" +
                         symmetry + "|" + string.Join(",", p.solution);

            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(raw));
                var s = new StringBuilder(16);
                for (int i = 0; i < 8; i++) s.Append(bytes[i].ToString("x2"));
                return s.ToString();
            }
        }

        public static int StructuralDistance(PuzzleDefinition a, PuzzleDefinition b)
        {
            if (a == null || b == null) return int.MaxValue;
            if (a.rows != b.rows || a.columns != b.columns || a.regionCount != b.regionCount)
                return 1000;

            var x = Sizes(a);
            var y = Sizes(b);
            Array.Sort(x);
            Array.Sort(y);

            int distance = 0;
            for (int i = 0; i < x.Length; i++)
                distance += Math.Abs(x[i] - y[i]);

            distance += Math.Abs(BoundaryCount(a) - BoundaryCount(b));
            distance += Math.Abs(SymmetryScore(a) - SymmetryScore(b));

            int solutionDistance = 0;
            for (int r = 0; r < a.rows; r++)
                if (a.solution[r] != b.solution[r]) solutionDistance++;
            distance += solutionDistance / 2;

            return distance;
        }

        public static bool StructurallySimilar(PuzzleDefinition a, PuzzleDefinition b, int threshold = 2)
            => StructuralDistance(a, b) <= threshold;

        private static int[] Sizes(PuzzleDefinition p)
        {
            var a = new int[p.regionCount];
            foreach (int z in p.regions) a[z]++;
            return a;
        }

        private static int BoundaryCount(PuzzleDefinition p)
        {
            int boundary = 0;
            for (int r = 0; r < p.rows; r++)
                for (int c = 0; c < p.columns; c++)
                {
                    int z = p.RegionAt(r, c);
                    if (r + 1 < p.rows && p.RegionAt(r + 1, c) != z) boundary++;
                    if (c + 1 < p.columns && p.RegionAt(r, c + 1) != z) boundary++;
                }
            return boundary;
        }

        private static int SymmetryScore(PuzzleDefinition p)
        {
            int score = 0;
            for (int r = 0; r < p.rows; r++)
                for (int c = 0; c < p.columns / 2; c++)
                    if (p.RegionAt(r, c) == p.RegionAt(r, p.columns - 1 - c)) score++;
            return score;
        }
    }
}