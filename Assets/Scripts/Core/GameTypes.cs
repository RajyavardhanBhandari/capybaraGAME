using System;
using UnityEngine;
namespace CapybaraGame.Core {
 public enum GameScreen { Home,LevelSelect,Gameplay,Daily,Shop,Leaderboard,Profile }
 public enum PuzzleStatus { Ready,Playing,Solved,Failed }
 public enum CharacterId { Capybara,Cat,Dog,Penguin,Panda }
 public enum PuzzleDifficultyBand { Easy,Medium,Hard }
 public enum PuzzleCategory { Balanced,Sparse,ComplexRegions,DeepLogic,QuickSolve,ChainReaction }
 [Serializable] public readonly struct CellCoordinate:IEquatable<CellCoordinate>{
  public readonly int Row,Column; public CellCoordinate(int row,int column){Row=row;Column=column;}
  public bool Equals(CellCoordinate o)=>Row==o.Row&&Column==o.Column; public override bool Equals(object o)=>o is CellCoordinate c&&Equals(c);
  public override int GetHashCode()=>unchecked(Row*397^Column); public override string ToString()=>Row+","+Column;
 }
 [Serializable] public sealed class PuzzleDefinition {
  public string id; public int seed; public int rows=10,columns=10; public int[] regions,solution,initialState;
  public int regionCount; public float difficulty; public string difficultyBand,category,fingerprint;
  public string generatorVersion="phase2-v1",contentVersion="1"; public int solutionCount;
  public int CellCount=>rows*columns; public int RegionAt(int row,int column)=>regions[row*columns+column];
  public int SolutionColumnAt(int row)=>solution[row]; public bool IsInBounds(int r,int c)=>r>=0&&r<rows&&c>=0&&c<columns;
  public PuzzleDefinition Clone()=>new PuzzleDefinition{id=id,seed=seed,rows=rows,columns=columns,
   regions=regions==null?null:(int[])regions.Clone(),solution=solution==null?null:(int[])solution.Clone(),
   initialState=initialState==null?null:(int[])initialState.Clone(),regionCount=regionCount,difficulty=difficulty,
   difficultyBand=difficultyBand,category=category,fingerprint=fingerprint,generatorVersion=generatorVersion,
   contentVersion=contentVersion,solutionCount=solutionCount};
 }
 [Serializable] public sealed class PuzzleState {
  public string puzzleId; public int[] placed; public int livesRemaining=3; public PuzzleStatus status=PuzzleStatus.Ready;
  public PuzzleState(string id,int cellCount=100){puzzleId=id;placed=new int[cellCount];for(int i=0;i<placed.Length;i++)placed[i]=-1;}
 }
 [Serializable] public sealed class LocalSave { public int unlockedLevel=1,coins=750,activeCharacter=(int)CharacterId.Capybara; public bool sound=true,haptics=true,reducedMotion=false; }
 public static class CharacterCatalog {
  public static readonly CharacterId[] All={CharacterId.Capybara,CharacterId.Cat,CharacterId.Dog,CharacterId.Penguin,CharacterId.Panda};
  public static string Name(CharacterId id){switch(id){case CharacterId.Capybara:return"Capybara";case CharacterId.Cat:return"Cat";case CharacterId.Dog:return"Dog";case CharacterId.Penguin:return"Penguin";default:return"Panda";}}
  public static string Symbol(CharacterId id){switch(id){case CharacterId.Capybara:return"C";case CharacterId.Cat:return"K";case CharacterId.Dog:return"D";case CharacterId.Penguin:return"P";default:return"B";}}
 }
}