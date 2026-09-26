using System;using System.Collections.Generic;using CapybaraGame.Core;
namespace CapybaraGame.Puzzle{
public sealed class SolverMetrics{public int NodesVisited,CandidateChecks,ForcedMoves,MaximumDepth,Backtracks,FirstSolutionDepth;}
public sealed class SolutionResult{public int SolutionCount;public int[] FirstSolution;public SolverMetrics Metrics=new SolverMetrics();}
public static class PuzzleSolver{
public static SolutionResult Solve(PuzzleDefinition p,int limit=2){var outp=new SolutionResult();if(p==null||p.rows!=p.columns||p.regionCount!=p.rows)return outp;if(limit<1)limit=1;var ru=new bool[p.rows];var cu=new bool[p.columns];var gu=new bool[p.regionCount];var placed=new int[p.CellCount];for(int i=0;i<placed.Length;i++)placed[i]=-1;Search(0);return outp;
void Search(int depth){if(outp.SolutionCount>=limit)return;outp.Metrics.MaximumDepth=Math.Max(outp.Metrics.MaximumDepth,depth);if(depth==p.rows){outp.SolutionCount++;if(outp.FirstSolution==null){outp.FirstSolution=(int[])placed.Clone();outp.Metrics.FirstSolutionDepth=depth;}return;}
int br=-1,bc=int.MaxValue;List<int> best=null;for(int r=0;r<p.rows;r++){if(ru[r])continue;var cs=new List<int>();for(int c=0;c<p.columns;c++){outp.Metrics.CandidateChecks++;if(Can(r,c))cs.Add(c);}if(cs.Count==0)return;if(cs.Count<bc){bc=cs.Count;br=r;best=cs;if(bc==1)break;}}
if(br<0)return;if(bc==1)outp.Metrics.ForcedMoves++;foreach(int c in best){int i=br*p.columns+c;placed[i]=0;ru[br]=cu[c]=gu[p.RegionAt(br,c)]=true;outp.Metrics.NodesVisited++;Search(depth+1);placed[i]=-1;ru[br]=cu[c]=gu[p.RegionAt(br,c)]=false;if(outp.SolutionCount>=limit)return;outp.Metrics.Backtracks++;}}
bool Can(int r,int c){if(ru[r]||cu[c])return false;int g=p.RegionAt(r,c);if(g<0||g>=gu.Length||gu[g])return false;for(int rr=Math.Max(0,r-1);rr<=Math.Min(p.rows-1,r+1);rr++)for(int cc=Math.Max(0,c-1);cc<=Math.Min(p.columns-1,c+1);cc++)if(placed[rr*p.columns+cc]!=-1)return false;return true;}}
}}