# Phase 5 — Level System

## Status
Phase 5 implementation is underway on branch feat/phase-5-level-system.

## Product contract
- 500 progression levels.
- Every 10th level is a Hard Challenge: 50 total.
- Core puzzle rules remain unchanged.
- Supported grids: 4×4, 5×5, 6×6, 7×7.
- Difficulty is a configurable score, not a grid-size shortcut.
- Level IDs remain stable independently of puzzle IDs.
- Puzzle content and progression metadata are separate databases.
- Generation is deterministic by seed + generation version + configuration.
- Candidate generation is offline/editor-side.
- Final content remains subject to human review.

## Architecture
LevelGenerationConfig → LevelBatchGenerator → Candidate Pool → validity/solver/uniqueness → difficulty/fingerprint → variety selection → LevelDatabase + PuzzleDatabase → ProductionPuzzleRepository → GameplayController.

## Progression bands
| Levels | Intended range |
|---|---|
| 1–100 | Learning / Early Mastery |
| 101–200 | Developing Skill |
| 201–300 | Advanced |
| 301–400 | Hard |
| 401–500 | Expert |

These bands guide candidate selection; they are not hard difficulty walls.

## Variety controls
- Maximum consecutive grid-size repetition
- Maximum consecutive style repetition
- Recent structural-fingerprint window
- Minimum structural distance
- Variety targets per 10 levels
- Variety targets per 50 levels

Structural distance considers region-size distribution, region boundaries, symmetry and solution differences.

## Hard Challenges
Hard Challenge metadata is explicit: IsHardChallenge and HardChallengeIndex.
The first 500 levels therefore contain exactly 50 Hard Challenges. They use the normal puzzle rules; the designation is progression metadata.

## Generation
Default production target is 10,000 candidates for the initial curation pass. This is configurable.

Pipeline:
1. Generate deterministic candidates.
2. Reject invalid/disconnected puzzles.
3. Solve and require uniqueness.
4. Calculate difficulty.
5. Calculate fingerprint.
6. Remove exact duplicates.
7. Remove structurally similar recent candidates.
8. Select candidates against progression targets.
9. Persist final level and puzzle databases.
10. Validate all 500 levels.
11. Human-review the final pool.

## Editor tools
When Unity is available:
- Capybara > Level System > Level Generator
- Capybara > Level System > Level Preview
- Capybara > Level System > Validate 500 Levels

The generator persists LevelDatabase.asset and PuzzleDatabase.asset under Assets/Resources/Levels. JSON copies are also emitted for inspection/versioning.

## Runtime
ProductionPuzzleRepository first attempts to load the persistent databases. If they are not yet generated, it falls back to deterministic generation so development gameplay remains usable.

Once the production databases are generated and committed, normal progression should not perform expensive puzzle generation at runtime.

## Determinism
A production puzzle is identified by its stable puzzle ID and generation version. Re-running the same generation configuration should produce the same candidate sequence and selection result.
Daily Puzzle compatibility is preserved through the existing date/version seed derivation; the Daily system itself remains outside Phase 5.

## QA
Automated coverage includes level 1/500 existence, invalid level handling, exactly 50 Hard Challenges, Hard Challenge indices 1–50, progression bands, deterministic level selection, database integrity, fingerprint determinism, production loading, region validity and uniqueness.

Unity Editor execution, stress tests, profiling and human playtesting remain Unity-dependent and must not be marked complete until actually executed.

## Unity version note
The repository currently declares Unity 6000.3.16f1 in ProjectSettings/ProjectVersion.txt. Phase 5 does not silently change that baseline. If the newly installed Unity 6.6 editor is to become the project baseline, that should be treated as a separate migration decision and tested first.

## Remaining work
1. Open the project in the installed Unity editor.
2. Resolve and verify compilation.
3. Run EditMode tests.
4. Run 100 / 1,000 / 10,000 candidate stress passes.
5. Generate the actual production database.
6. Run complete 500-level validation.
7. Inspect representative levels with the preview tool.
8. Review the generated 500 manually.
9. Profile load/runtime memory.
10. Commit generated production assets and the final validation report.