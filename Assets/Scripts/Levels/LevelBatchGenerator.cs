using System;
using System.Collections.Generic;
using System.Linq;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Levels
{
    public sealed class LevelGenerationReport
    {
        public int RequestedCandidates;
        public int GeneratedCandidates;
        public int InvalidCandidates;
        public int UniqueCandidates;
        public int SimilarCandidatesRejected;
        public int SelectedLevels;
        public int HardChallenges;
        public TimeSpan Elapsed;
        public readonly Dictionary<PuzzleDifficultyBand, int> DifficultyDistribution = new Dictionary<PuzzleDifficultyBand, int>();
        public readonly Dictionary<int, int> GridDistribution = new Dictionary<int, int>();
        public readonly Dictionary<PuzzleCategory, int> StyleDistribution = new Dictionary<PuzzleCategory, int>();
    }

    public sealed class LevelGenerationOutput
    {
        public LevelDatabase LevelDatabase;
        public List<PuzzleDefinition> PuzzlePool;
        public LevelGenerationReport Report;
        public readonly List<string> Warnings = new List<string>();
    }

    public static class LevelBatchGenerator
    {
        public static LevelGenerationOutput Generate(LevelGenerationConfig config)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (config.launchLevelCount < 1) throw new ArgumentException("Launch level count must be positive.", nameof(config));

            var started = DateTime.UtcNow;
            var report = new LevelGenerationReport { RequestedCandidates = config.candidateCount };
            var pool = GenerateCandidates(config, report);
            var selected = SelectProgression(pool, config, report);

            report.Elapsed = DateTime.UtcNow - started;

            var output = new LevelGenerationOutput
            {
                PuzzlePool = selected.Select(x => x.puzzle).ToList(),
                LevelDatabase = new LevelDatabase
                {
                    databaseVersion = 1,
                    generationVersion = config.generationVersion,
                    levels = selected.Select(x => x.level).ToList()
                },
                Report = report
            };

            if (!output.LevelDatabase.IsComplete(config.launchLevelCount))
                output.Warnings.Add("The candidate pool did not contain enough suitable puzzles to fill the requested launch database.");

            return output;
        }

        private sealed class Candidate
        {
            public PuzzleDefinition puzzle;
            public int sourceIndex;
        }

        private sealed class Selected
        {
            public LevelDefinition level;
            public PuzzleDefinition puzzle;
        }

        private static List<Candidate> GenerateCandidates(LevelGenerationConfig config, LevelGenerationReport report)
        {
            var list = new List<Candidate>(config.candidateCount);
            var fingerprints = new HashSet<string>();
            int target = Math.Max(config.launchLevelCount * 10, config.candidateCount);

            for (int i = 0; i < target; i++)
            {
                int levelHint = 1 + (i % config.launchLevelCount);
                var band = config.GetBand(levelHint);
                if (band == null || band.allowedGridSizes == null || band.allowedGridSizes.Length == 0) continue;

                int grid = band.allowedGridSizes[i % band.allowedGridSizes.Length];
                var style = (PuzzleCategory)((i + levelHint) % 6);
                var targetBand = LevelProgressionRules.CategoryForScore(
                    band.targetDifficultyMin +
                    (band.targetDifficultyMax - band.targetDifficultyMin) *
                    ((i % 100) / 99f),
                    config.difficultyProfile);

                var generation = PuzzleGenerator.Generate(new PuzzleGenerationConfig
                {
                    rows = grid,
                    columns = grid,
                    regionCount = grid,
                    seed = unchecked(config.seed + i * 7919),
                    generationVersion = config.generationVersion,
                    maxAttempts = 300,
                    requireUniqueSolution = true,
                    requireConnectedRegions = true,
                    difficultyProfile = config.difficultyProfile,
                    targetBand = targetBand,
                    category = style
                });

                if (generation.Puzzle == null)
                {
                    report.InvalidCandidates++;
                    continue;
                }

                report.GeneratedCandidates++;
                if (!fingerprints.Add(generation.Puzzle.fingerprint))
                    continue;

                list.Add(new Candidate { puzzle = generation.Puzzle, sourceIndex = i });
                report.UniqueCandidates++;

                if (list.Count >= config.candidateCount) break;
            }

            return list;
        }

        private static List<Selected> SelectProgression(
            List<Candidate> pool,
            LevelGenerationConfig config,
            LevelGenerationReport report)
        {
            var remaining = new List<Candidate>(pool);
            var selected = new List<Selected>(config.launchLevelCount);
            var recent = new Queue<PuzzleDefinition>();

            for (int levelNumber = 1; levelNumber <= config.launchLevelCount; levelNumber++)
            {
                var target = LevelCatalog.Get(levelNumber);
                var band = config.GetBand(levelNumber);
                bool hard = target.IsHardChallenge;
                float targetScore = TargetScore(levelNumber, band, hard);
                Candidate best = null;
                float bestCost = float.MaxValue;

                foreach (var candidate in remaining)
                {
                    var puzzle = candidate.puzzle;
                    if (band != null && !band.allowedGridSizes.Contains(puzzle.rows)) continue;
                    if (hard && puzzle.difficulty < config.difficultyProfile.MediumMax) continue;
                    if (ViolatesRecentVariety(puzzle, target, recent, config.variety)) continue;

                    float cost = Math.Abs(puzzle.difficulty - targetScore);
                    if (puzzle.category != target.Style.ToString()) cost += 4f;
                    if (puzzle.rows != target.BoardSize) cost += 2f;
                    if (cost < bestCost) { best = candidate; bestCost = cost; }
                }

                if (best == null)
                {
                    foreach (var candidate in remaining)
                    {
                        if (ViolatesRecentVariety(candidate.puzzle, target, recent, config.variety)) continue;
                        float cost = Math.Abs(candidate.puzzle.difficulty - targetScore);
                        if (cost < bestCost) { best = candidate; bestCost = cost; }
                    }
                }

                if (best == null) break;

                remaining.Remove(best);
                var puzzle = best.puzzle;
                puzzle.id = "P-" + puzzle.fingerprint + "-v" + config.generationVersion;

                var definition = target.Clone();
                definition.PuzzleId = puzzle.id;
                definition.DifficultyScore = puzzle.difficulty;
                definition.Difficulty = LevelProgressionRules.CategoryForScore(puzzle.difficulty, config.difficultyProfile);
                definition.Fingerprint = puzzle.fingerprint;
                definition.IsHardChallenge = hard;
                definition.HardChallengeIndex = LevelProgressionRules.HardChallengeIndex(levelNumber, config.hardChallengeFrequency);
                definition.Version = 1;

                selected.Add(new Selected { level = definition, puzzle = puzzle });
                recent.Enqueue(puzzle);
                while (recent.Count > config.variety.recentFingerprintWindow) recent.Dequeue();

                Increment(report.DifficultyDistribution, definition.Difficulty);
                Increment(report.GridDistribution, puzzle.rows);
                Increment(report.StyleDistribution, definition.Style);
                if (hard) report.HardChallenges++;
            }

            report.SelectedLevels = selected.Count;
            return selected;
        }

        private static bool ViolatesRecentVariety(
            PuzzleDefinition puzzle,
            LevelDefinition target,
            Queue<PuzzleDefinition> recent,
            LevelVarietyConfig variety)
        {
            if (recent.Count == 0) return false;
            var last = recent.Last();
            if (last.rows == puzzle.rows && SameRecentGrid(recent, puzzle, variety.maxSameGridSizeInRow))
                return true;
            if (last.category == puzzle.category && SameRecentStyle(recent, puzzle, variety.maxSameStyleInRow))
                return true;

            foreach (var other in recent)
                if (PuzzleFingerprint.StructuralDistance(puzzle, other) <= variety.minimumFingerprintDistance)
                    return true;

            return false;
        }

        private static bool SameRecentGrid(Queue<PuzzleDefinition> recent, PuzzleDefinition puzzle, int max)
        {
            if (max <= 0) return false;
            int count = 0;
            foreach (var item in recent.Reverse())
            {
                if (item.rows != puzzle.rows) break;
                if (++count >= max) return true;
            }
            return false;
        }

        private static bool SameRecentStyle(Queue<PuzzleDefinition> recent, PuzzleDefinition puzzle, int max)
        {
            if (max <= 0) return false;
            int count = 0;
            foreach (var item in recent.Reverse())
            {
                if (item.category != puzzle.category) break;
                if (++count >= max) return true;
            }
            return false;
        }

        private static float TargetScore(int level, ProgressionBandConfig band, bool hard)
        {
            if (band == null) return hard ? 80f : 40f;
            float t = (level - band.startLevel) / (float)Math.Max(1, band.endLevel - band.startLevel);
            float score = band.targetDifficultyMin + (band.targetDifficultyMax - band.targetDifficultyMin) * t;
            return hard ? Math.Max(score, 70f) : score;
        }

        private static void Increment<T>(Dictionary<T, int> dictionary, T key)
        {
            if (!dictionary.ContainsKey(key)) dictionary[key] = 0;
            dictionary[key]++;
        }
    }
}