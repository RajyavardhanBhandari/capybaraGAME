using System;using System.Collections.Generic;using CapybaraGame.Core;
namespace CapybaraGame.Puzzle {
 public static class PuzzleValidator {
  public static bool IsPlacementValid(PuzzleDefinition p,int[] placed,int row,int col){
   if(!Shape(p,placed)||!p.IsInBounds(row,col)||placed[row*p.columns+col]!=-1)return false; int region=p.RegionAt(row,col);
   for(int r=0;r<p.rows;r++)for(int c=0;c<p.columns;c++){int x=placed[r*p.columns+c];if(x==-1)continue;
    if(r==row||c==col||p.RegionAt(r,c)==region||PuzzleRules.AreAdjacent(r,c,row,col))return false;} return true;
  }
  public static bool IsSolved(PuzzleDefinition p,int[] placed){
   if(!Shape(p,placed))return false;int count=0;var rs=new HashSet<int>();var rows=new HashSet<int>();var cols=new HashSet<int>();
   for(int r=0;r<p.rows;r++)for(int c=0;c<p.columns;c++){int i=r*p.columns+c;if(placed[i]==-1)continue;count++;
    if(!rs.Add(p.RegionAt(r,c))||!rows.Add(r)||!cols.Add(c))return false;
    for(int rr=r-1;rr<=r+1;rr++)for(int cc=c-1;cc<=c+1;cc++)if(p.IsInBounds(rr,cc)&&!(rr==r&&cc==c)&&placed[rr*p.columns+cc]!=-1)return false;}
   return count==p.rows&&rows.Count==p.rows&&cols.Count==p.columns&&rs.Count==p.regionCount;
  }
  public static bool IsBoardValid(PuzzleDefinition p,int[] placed){if(!Shape(p,placed))return false;var r=new bool[p.rows];var c=new bool[p.columns];var g=new bool[p.regionCount];
   for(int y=0;y<p.rows;y++)for(int x=0;x<p.columns;x++){int i=y*p.columns+x;if(placed[i]==-1)continue;int z=p.RegionAt(y,x);
    if(r[y]||c[x]||z<0||z>=g.Length||g[z])return false;r[y]=true;c[x]=true;g[z]=true;}return true;}
  public static bool IsRegionMapValid(PuzzleDefinition p,bool connected=true){if(p==null||p.regions==null||p.regions.Length!=p.CellCount||p.regionCount<=0)return false;
   var sizes=new int[p.regionCount];foreach(int z in p.regions){if(z<0||z>=p.regionCount)return false;sizes[z]++;}for(int z=0;z<sizes.Length;z++)if(sizes[z]==0)return false;if(!connected)return true;
   for(int z=0;z<p.regionCount;z++){int start=Array.IndexOf(p.regions,z);var seen=new bool[p.CellCount];var q=new Queue<int>();q.Enqueue(start);seen[start]=true;int n=0;
    while(q.Count>0){int i=q.Dequeue();n++;int r=i/p.columns,c=i%p.columns;Visit(r-1,c);Visit(r+1,c);Visit(r,c-1);Visit(r,c+1);
     void Visit(int rr,int cc){if(!p.IsInBounds(rr,cc))return;int j=rr*p.columns+cc;if(!seen[j]&&p.regions[j]==z){seen[j]=true;q.Enqueue(j);}}}
    if(n!=sizes[z])return false;}return true;}
  static bool Shape(PuzzleDefinition p,int[] placed)=>p!=null&&placed!=null&&p.rows>0&&p.columns>0&&p.regions!=null&&p.regions.Length==p.CellCount&&placed.Length==p.CellCount;
 }
}