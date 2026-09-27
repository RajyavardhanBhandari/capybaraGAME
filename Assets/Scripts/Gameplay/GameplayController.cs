using System;
using CapybaraGame.Core;
using CapybaraGame.Puzzle;

namespace CapybaraGame.Gameplay
{
    public enum GameplayState { Loading, Ready, Playing, Paused, Completed, Failed, Reward, NextLevel }
    public enum GameplayEventType { PuzzleStarted, CellSelected, MoveAttempted, MoveCorrect, MoveIncorrect, LifeLost, CharacterPlaced, CharacterRemoved, PuzzleCompleted, PuzzleFailed, RewardGranted, LevelAdvanced }
    public readonly struct GameplayEvent
    {
        public readonly GameplayEventType Type; public readonly int Row; public readonly int Column; public readonly int LivesRemaining;
        public GameplayEvent(GameplayEventType type,int row=-1,int column=-1,int livesRemaining=-1){Type=type;Row=row;Column=column;LivesRemaining=livesRemaining;}
    }
    public sealed class GameplayController
    {
        public const int MaxLives=PuzzleRules.StartingLives;
        public PuzzleDefinition Puzzle{get;private set;} public PuzzleState State{get;private set;} public GameplayState CurrentState{get;private set;}=GameplayState.Loading; public int LevelId{get;private set;} public CharacterId ActiveCharacter{get;private set;} public RewardResult LastReward{get;private set;}
        public event Action<GameplayEvent> EventRaised; public event Action StateChanged; public event Action<RewardResult> Completed; public event Action Failed;
        public void LoadLevel(int levelId,CharacterId character){LevelId=levelId;ActiveCharacter=character;CurrentState=GameplayState.Loading;var puzzle=ProductionPuzzleRepository.Get(levelId);LoadPuzzle(puzzle,levelId,character);}
        public void LoadPuzzle(PuzzleDefinition puzzle,int levelId,CharacterId character){if(puzzle==null)throw new ArgumentNullException(nameof(puzzle));UnityEngine.Time.timeScale=1f;Puzzle=puzzle.Clone();LevelId=levelId;ActiveCharacter=character;State=new PuzzleState(Puzzle.id,Puzzle.CellCount);State.status=PuzzleStatus.Playing;CurrentState=GameplayState.Playing;LastReward=default;Raise(GameplayEventType.PuzzleStarted);StateChanged?.Invoke();}
        public void MarkCell(int row,int column)
        {
            if(CurrentState!=GameplayState.Playing||Puzzle==null||State==null||!Puzzle.IsInBounds(row,column))return;
            int index=row*Puzzle.columns+column;
            if(State.placed[index]!=-1)
            {
                State.placed[index]=-1;
                State.marks[index]=false;
                Raise(GameplayEventType.CharacterRemoved,row,column,State.livesRemaining);
                StateChanged?.Invoke();
                return;
            }
            State.marks[index]=!State.marks[index];
            Raise(GameplayEventType.CellSelected,row,column,State.livesRemaining);
            StateChanged?.Invoke();
        }

        public void TapCell(int row,int column){if(CurrentState!=GameplayState.Playing||Puzzle==null||State==null||!Puzzle.IsInBounds(row,column))return;Raise(GameplayEventType.CellSelected,row,column,State.livesRemaining);Raise(GameplayEventType.MoveAttempted,row,column,State.livesRemaining);int index=row*Puzzle.columns+column;if(State.placed[index]!=-1){State.placed[index]=-1;Raise(GameplayEventType.CharacterRemoved,row,column,State.livesRemaining);StateChanged?.Invoke();return;}if(!PuzzleValidator.IsPlacementValid(Puzzle,State.placed,row,column)){State.livesRemaining=Math.Max(0,State.livesRemaining-1);Raise(GameplayEventType.MoveIncorrect,row,column,State.livesRemaining);Raise(GameplayEventType.LifeLost,row,column,State.livesRemaining);StateChanged?.Invoke();if(State.livesRemaining==0){State.status=PuzzleStatus.Failed;CurrentState=GameplayState.Failed;Raise(GameplayEventType.PuzzleFailed,row,column,0);Failed?.Invoke();}return;}State.placed[index]=(int)ActiveCharacter;
        State.marks[index]=false;Raise(GameplayEventType.MoveCorrect,row,column,State.livesRemaining);Raise(GameplayEventType.CharacterPlaced,row,column,State.livesRemaining);StateChanged?.Invoke();if(PuzzleValidator.IsSolved(Puzzle,State.placed))Complete();}
        private void Complete(){if(CurrentState!=GameplayState.Playing)return;State.status=PuzzleStatus.Solved;CurrentState=GameplayState.Completed;LastReward=RewardCalculator.CalculateCompletion(State.livesRemaining,LevelRewardConfig.Default);Raise(GameplayEventType.PuzzleCompleted,-1,-1,State.livesRemaining);Completed?.Invoke(LastReward);}
        public bool ReviveWithBerry()
        {
            if (CurrentState != GameplayState.Failed || State == null || State.livesRemaining > 0) return false;
            State.livesRemaining = 1;
            State.status = PuzzleStatus.Playing;
            CurrentState = GameplayState.Playing;
            UnityEngine.Time.timeScale = 1f;
            Raise(GameplayEventType.PuzzleStarted, -1, -1, 1);
            StateChanged?.Invoke();
            return true;
        }

        public void Pause(){if(CurrentState!=GameplayState.Playing)return;CurrentState=GameplayState.Paused;UnityEngine.Time.timeScale=0f;}
        public void Resume(){if(CurrentState!=GameplayState.Paused)return;CurrentState=GameplayState.Playing;UnityEngine.Time.timeScale=1f;}
        public void Restart(){if(Puzzle==null)return;UnityEngine.Time.timeScale=1f;LoadLevel(LevelId,ActiveCharacter);}
        public void Exit(){UnityEngine.Time.timeScale=1f;if(CurrentState==GameplayState.Completed||CurrentState==GameplayState.Failed)return;CurrentState=GameplayState.NextLevel;}
        public bool TryGetSolutionCell(out int row,out int column){row=-1;column=-1;if(Puzzle==null||Puzzle.solution==null||State==null)return false;for(int r=0;r<Puzzle.rows;r++){int c=Puzzle.solution[r];if(State.placed[r*Puzzle.columns+c]==-1){row=r;column=c;return true;}}return false;}
        public void GrantReward(){if(CurrentState!=GameplayState.Completed)return;CurrentState=GameplayState.Reward;Raise(GameplayEventType.RewardGranted,-1,-1,State.livesRemaining);}
        public void Advance(){if(CurrentState!=GameplayState.Reward)return;CurrentState=GameplayState.NextLevel;Raise(GameplayEventType.LevelAdvanced);}
        private void Raise(GameplayEventType type,int row=-1,int column=-1,int lives=-1)=>EventRaised?.Invoke(new GameplayEvent(type,row,column,lives));
    }
}