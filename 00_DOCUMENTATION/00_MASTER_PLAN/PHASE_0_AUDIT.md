# Phase 0 Audit

## Purpose

Phase 0 converts the approved high-level brief into an implementation-ready foundation without changing the locked core concept.

## Confirmed source

The original project brief establishes the core puzzle, five launch animals, progression, Hard Challenge, Golden Challenge, Daily Puzzle, leaderboard, economy philosophy, monetization constraints, UX direction, Unity/C# direction, analytics requirements, and launch checklist.

## Repository audit

Official repository: `RajyavardhanBhandari/capybaraGAME`

At the start of Phase 0 the repository was empty: no code, commits, Unity project, README, or documentation. Therefore there was no existing implementation to reconcile.

## Locked product decisions

1. One character per colored region.
2. One character per row.
3. One character per column.
4. Characters cannot touch, including diagonally.
5. Normal puzzle starts with 3 lives.
6. Incorrect placement consumes one life.
7. Tapping an occupied cell removes that character.
8. Completion grants rewards.
9. No bombs, special tiles, moving obstacles, timed mechanics, match-3, or additional puzzle rules at launch.
10. Capybara is the flagship/default mascot.
11. Launch animals are Capybara, Cat, Dog, Penguin, Panda.
12. Characters are cosmetic and provide no gameplay advantage.
13. 500 progression levels.
14. Every 10th progression level is a Hard Challenge.
15. Golden Challenge is a separate optional challenge.
16. Daily Puzzle is shared across players.
17. Weekly competition is separate from progression.
18. Coins are soft currency.
19. Lives are an in-puzzle mistake budget, not an energy system.
20. Treats are performance/competitive points, not spendable currency.
21. No premium currency at launch.
22. No pay-to-win.
23. No competitive score advantage from ads or purchases.
24. Unity + C#.
25. Cross-platform gameplay codebase, iOS-first release priority.

## Phase 0 decisions formalized

### Daily scoring

The Daily Puzzle uses a fixed shared puzzle/seed. The primary competitive score is the Treat value from the completed attempt:

- 3 lives remaining: 3 Treats
- 2 lives remaining: 2 Treats
- 1 life remaining: 1 Treat
- failed attempt: 0 Treats

Only the player's first eligible Daily completion is ranked. Hints, reveals, and extra-life purchases/ads cannot increase competitive score. The Daily Puzzle cannot be replayed for a higher ranked score.

### Weekly scoring

Weekly score is the sum of the player's eligible Daily Treat scores during the weekly period. Missing a day contributes zero. No paid or ad-based competitive advantage is permitted.

Tie-breaker order:
1. Higher number of perfect Daily completions.
2. Earlier timestamp of the tied player's final qualifying Daily completion.
3. Stable server-generated player identifier only as a final deterministic tie-break.

### Progression lives

Lives exist only inside an individual puzzle attempt. There is no cross-level energy gate.

### Daily vs personalization

Progression puzzles may be personalized. The Daily Puzzle is never personalized and uses the shared daily seed/puzzle.

### Aid scoring

Hints, reveals, and extra lives may help progression gameplay but never increase a Daily competitive score. Daily competitive attempts must remain comparable.

## Unresolved by design

The following are implementation parameters rather than open product ambiguity and must be tuned through playtesting:

- exact coin prices
- exact ad frequency within the documented limits
- exact puzzle difficulty thresholds
- final visual asset dimensions
- audio mix levels
- backend provider
- exact rewarded-ad network
- exact IAP SKU pricing by market

These must remain configurable rather than hard-coded.

## Phase 0 completion criterion

Phase 0 is complete when all specifications in this directory are present, internally consistent, and sufficient for Phase 1 implementation without inventing core product rules.
