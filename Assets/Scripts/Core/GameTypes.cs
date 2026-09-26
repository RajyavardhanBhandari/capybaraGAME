using System;
using System.Collections.Generic;
using UnityEngine;

namespace CapybaraGame.Core
{
    public enum GameScreen { Home, LevelSelect, Gameplay, Result, Pause }
    public enum PuzzleStatus { Ready, Playing, Solved, Failed }
    public enum CharacterId { Capybara, Cat, Dog, Penguin, Panda }
    public enum PuzzleDifficultyBand { Easy, Medium, Hard }
    public enum PuzzleCategory { Balanced, Sparse, ComplexRegions, DeepLogic, QuickSolve, ChainReaction }

    [Serializable] public readonly struct CellCoordinate : IEquatable<CellCoordinate>
    {
        public readonly int Row; public readonly int Column;
        public CellCoordinate(int row,int column){Row=row;Column=column;}
        public bool Equals(CellCoordinate other)=>Row==other.Row&&Column==other.Column;
        public override bool Equals(object obj)=>obj is CellCoordinate other&&Equals(other);
        public override int GetHashCode()=>unchecked(Row*397^Column);
        public override string ToString()=>Row+","+Column;
    }
    [Serializable] public sealed class PuzzleDefinition
    {
        public string id; public int seed; public int rows=10; public int columns=10; public int[] regions; public int[] solution; public int[] initialState; public int regionCount; public float difficulty; public string difficultyBand; public string category; public string fingerprint; public string generatorVersion="phase2-v1"; public string contentVersion="1"; public int solutionCount; public bool isHardChallenge;
        public int CellCount=>rows*columns; public int RegionAt(int r,int c)=>regions[r*columns+c]; public int SolutionColumnAt(int r)=>solution[r]; public bool IsInBounds(int r,int c)=>r>=0&&r<rows&&c>=0&&c<columns;
        public PuzzleDefinition Clone()=>new PuzzleDefinition{id=id,seed=seed,rows=rows,columns=columns,regions=regions==null?null:(int[])regions.Clone(),solution=solution==null?null:(int[])solution.Clone(),initialState=initialState==null?null:(int[])initialState.Clone(),regionCount=regionCount,difficulty=difficulty,difficultyBand=difficultyBand,category=category,fingerprint=fingerprint,generatorVersion=generatorVersion,contentVersion=contentVersion,solutionCount=solutionCount,isHardChallenge=isHardChallenge};
    }
    [Serializable] public sealed class PuzzleState
    {
        public string puzzleId; public int[] placed; public int livesRemaining=3; public PuzzleStatus status=PuzzleStatus.Ready;
        public PuzzleState(string id,int cellCount=100){puzzleId=id;placed=new int[cellCount];for(int i=0;i<placed.Length;i++)placed[i]=-1;}
    }
    [Serializable] public sealed class LocalSave
    {
        public int unlockedLevel=1; public int currentLevel=1; public int coins=750; public int treats=0; public int activeCharacter=(int)CharacterId.Capybara; public bool sound=true; public bool haptics=true; public bool reducedMotion=false; public List<int> completedLevels=new List<int>();
    }
    public static class CharacterCatalog
    {
        public static readonly CharacterId[] All={CharacterId.Capybara,CharacterId.Cat,CharacterId.Dog,CharacterId.Penguin,CharacterId.Panda};
        public static string Name(CharacterId id){switch(id){case CharacterId.Capybara:return "Capybara";case CharacterId.Cat:return "Cat";case CharacterId.Dog:return "Dog";case CharacterId.Penguin:return "Penguin";default:return "Panda";}}
        public static string Symbol(CharacterId id){switch(id){case CharacterId.Capybara:return "C";case CharacterId.Cat:return "K";case CharacterId.Dog:return "D";case CharacterId.Penguin:return "P";default:return "B";}}
    }
}