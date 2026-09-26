using UnityEngine;
using CapybaraGame.Core;

namespace CapybaraGame.Puzzle
{
    public static class PuzzleSerialization
    {
        public static string ToJson(PuzzleDefinition puzzle, bool pretty = false)
            => JsonUtility.ToJson(puzzle, pretty);

        public static PuzzleDefinition FromJson(string json)
            => string.IsNullOrEmpty(json) ? null : JsonUtility.FromJson<PuzzleDefinition>(json);

        public static bool RoundTrips(PuzzleDefinition puzzle)
        {
            var copy = FromJson(ToJson(puzzle));
            return copy != null
                && copy.fingerprint == puzzle.fingerprint
                && copy.seed == puzzle.seed
                && copy.generatorVersion == puzzle.generatorVersion;
        }
    }
}