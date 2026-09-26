using System;using System.Collections.Generic;using CapybaraGame.Core;
namespace CapybaraGame.Puzzle {
 public static class PuzzleRepository {
  public const int LaunchLevelCount=500,GeneratorVersion=1;static readonly Dictionary<int,PuzzleDefinition> Cache=new Dictionary<int,PuzzleDefinition>();
  public static PuzzleDefinition Get(int level){if(level<1||level>LaunchLevelCount)throw new ArgumentOutOfRangeException(nameof(level));if(Cache.TryGetValue(level,out var p))return p.Clone();
   var cfg=new PuzzleGenerationConfig{rows=10,columns=10,regionCount=10,seed=StableSeed(level),generationVersion=GeneratorVersion,maxAttempts=500,requireUniqueSolution=false};
   p=GenerateLegacy(cfg,level);Cache[level]=p;return p.Clone();}
  public static PuzzleGenerationResult Generate(PuzzleGenerationConfig c)=>PuzzleGenerator.Generate(c);
  public static string Fingerprint(PuzzleDefinition p)=>PuzzleFingerprint.Compute(p);
  public static int StableSeed(int level){unchecked{uint x=(uint)level*0x9E3779B9u;x^=0xC0FEBABEu;x^=x>>16;x*=0x85EBCA6Bu;x^=x>>13;return(int)x;}}
  static PuzzleDefinition GenerateLegacy(PuzzleGenerationConfig c,int level){for(int a=0;a<c.maxAttempts;a++){var rng=new DeterministicRandom(unchecked(c.seed+a*7919));var sol=MakeSolution(10,rng);if(sol==null)continue;var regions=MakeRegions(10,sol,rng);
    var p=new PuzzleDefinition{id="P"+level.ToString("000"),seed=c.seed,rows=10,columns=10,regionCount=10,regions=regions,solution=sol,initialState=Empty(100),generationVersion="phase2-v"+GeneratorVersion};
    if(!PuzzleValidator.IsRegionMapValid(p))continue;var d=PuzzleDifficultyEvaluator.Evaluate(p);p.difficulty=d.Score;p.difficultyBand=d.Band.ToString();p.category=PuzzleCategory.Balanced.ToString();p.solutionCount=PuzzleSolver.Solve(p,2).SolutionCount;p.fingerprint=PuzzleFingerprint.Compute(p);return p;}throw new InvalidOperationException("Unable to generate level "+level);}
  static int[] MakeSolution(int n,DeterministicRandom rng){var s=new int[n];for(int i=0;i<n;i++)s[i]=-1;for(int r=0;r<n;r++){var cs=new List<int>();for(int c=0;c<n;c++){bool ok=true;for(int q=0;q<r;q++)if(s[q]==c||(Math.Abs(q-r)<=1&&Math.Abs(s[q]-c)<=1)){ok=false;break;}if(ok)cs.Add(c);}if(cs.Count==0)return null;s[r]=cs[rng.NextInt(0,cs.Count)];}return s;}
  static int[] MakeRegions(int n,int[] sol,DeterministicRandom rng){var a=Empty(n*n);var f=new List<int>[n];for(int i=0;i<n;i++)f[i]=new List<int>();for(int z=0;z<n;z++){int i=z*n+sol[z];a[i]=z;f[z].Add(i);}int left=a.Length-n;
   while(left>0){var ids=new List<int>();for(int z=0;z<n;z++)if(f[z].Count>0)ids.Add(z);if(ids.Count==0)throw new InvalidOperationException("Region growth stalled");int z=ids[rng.NextInt(0,ids.Count)],src=f[z][rng.NextInt(0,f[z].Count)],r=src/n,c=src%n;var cs=new List<int>();Add(r-1,c);Add(r+1,c);Add(r,c-1);Add(r,c+1);if(cs.Count==0){f[z].Remove(src);continue;}int pick=cs[rng.NextInt(0,cs.Count)];a[pick]=z;f[z].Add(pick);left--;void Add(int rr,int cc){if(rr>=0&&rr<n&&cc>=0&&cc<n&&a[rr*n+cc]==-1)cs.Add(rr*n+cc);}}return a;}
  static int[] Empty(int n){var a=new int[n];for(int i=0;i<n;i++)a[i]=-1;return a;}
 }
}