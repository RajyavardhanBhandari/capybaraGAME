#if UNITY_EDITOR
using System;using UnityEditor;using UnityEngine;using CapybaraGame.Puzzle;
namespace CapybaraGame.Editor{public static class PuzzleStressTest{
[MenuItem("Capybara/Puzzle Engine/Stress 100")]
static void Run100()=>Run(100);
[MenuItem("Capybara/Puzzle Engine/Stress 1000")]
static void Run1000()=>Run(1000);
[MenuItem("Capybara/Puzzle Engine/Stress 10000")]
static void Run10000()=>Run(10000);
static void Run(int count){var sw=System.Diagnostics.Stopwatch.StartNew();int accepted=0,duplicates=0;var seen=new System.Collections.Generic.HashSet<string>();for(int i=0;i<count;i++){int size=4+(i%4);var r=PuzzleGenerator.Generate(new PuzzleGenerationConfig{rows=size,columns=size,regionCount=size,seed=700000+i,maxAttempts=1000});if(r.Puzzle!=null){accepted++;if(!seen.Add(r.Puzzle.fingerprint))duplicates++;}}sw.Stop();Debug.Log($"Puzzle stress {count}: accepted={accepted}, duplicateFingerprints={duplicates}, elapsedMs={sw.ElapsedMilliseconds}, avgMs={(double)sw.ElapsedMilliseconds/count:0.00}");}
}}
#endif