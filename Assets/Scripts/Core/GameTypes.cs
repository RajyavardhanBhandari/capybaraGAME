using System;
using System.Collections.Generic;
using UnityEngine;

namespace CapybaraGame.Core
{
    public enum GameScreen { Home, LevelSelect, Gameplay, Daily, Shop, Leaderboard, Profile }
    public enum PuzzleStatus { Ready, Playing, Solved, Failed }
    public enum CharacterId { Capybara, Cat, Dog, Penguin, Panda }

    [Serializable]
    public sealed class PuzzleDefinition
    {
        public string id;
        public int seed;
        public int rows = 10;
        public int columns = 10;
        public int[] regions;
        public int[] solution;
        public float difficulty;
        public string difficultyBand;
        public string generatorVersion = "phase1-v1";

        public int RegionAt(int row, int column) => regions[row * columns + column];
        public int SolutionColumnAt(int row) => solution[row];
    }

    [Serializable]
    public sealed class PuzzleState
    {
        public string puzzleId;
        public int[] placed = new int[100];
        public int livesRemaining = 3;
        public PuzzleStatus status = PuzzleStatus.Ready;

        public PuzzleState(string id)
        {
            puzzleId = id;
            for (int i = 0; i < placed.Length; i++) placed[i] = -1;
        }
    }

    [Serializable]
    public sealed class LocalSave
    {
        public int unlockedLevel = 1;
        public int coins = 750;
        public int activeCharacter = (int)CharacterId.Capybara;
        public bool sound = true;
        public bool haptics = true;
        public bool reducedMotion = false;
    }

    public static class CharacterCatalog
    {
        public static readonly CharacterId[] All =
        {
            CharacterId.Capybara, CharacterId.Cat, CharacterId.Dog,
            CharacterId.Penguin, CharacterId.Panda
        };

        public static string Name(CharacterId id)
        {
            switch (id)
            {
                case CharacterId.Capybara: return "Capybara";
                case CharacterId.Cat: return "Cat";
                case CharacterId.Dog: return "Dog";
                case CharacterId.Penguin: return "Penguin";
                default: return "Panda";
            }
        }

        public static string Symbol(CharacterId id)
        {
            switch (id)
            {
                case CharacterId.Capybara: return "C";
                case CharacterId.Cat: return "🐱";
                case CharacterId.Dog: return "D";
                case CharacterId.Penguin: return "P";
                default: return "B";
            }
        }
    }
}
