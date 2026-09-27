using System.Collections.Generic;
using UnityEngine;
using CapybaraGame.Core;

namespace CapybaraGame.Levels
{
    [PreferBinarySerialization]
    public sealed class LevelDatabaseAsset : ScriptableObject
    {
        public int databaseVersion = 1;
        public int generationVersion = 1;
        public List<LevelDefinition> levels = new List<LevelDefinition>();
    }

    [PreferBinarySerialization]
    public sealed class PuzzleDatabaseAsset : ScriptableObject
    {
        public int databaseVersion = 1;
        public int generationVersion = 1;
        public List<PuzzleDefinition> puzzles = new List<PuzzleDefinition>();
    }
}