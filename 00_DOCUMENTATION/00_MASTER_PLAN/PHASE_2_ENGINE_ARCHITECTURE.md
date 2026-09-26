# Phase 2 Puzzle Engine Architecture
## Audit
Phase 0 locks four rules: one character per region, row and column uniqueness, and no touching including diagonals. Progression contains 500 levels with every tenth level a Hard Challenge. Production content must be generated, validated, solved for uniqueness, difficulty-scored, fingerprinted, curated, and playtested.

Phase 1 established Unity 6.3 LTS, a runtime bootstrap, a 10x10 vertical-slice board, deterministic prototype content, a validator, three lives, local persistence, and EditMode tests. The existing PuzzleRepository file was not a repository implementation: it contained a duplicate test class. Phase 2 replaces that gap without moving puzzle logic into UI.

## Architecture
- PuzzleDefinition: immutable-by-convention puzzle content and metadata.
- PuzzleState: player progress only.
- PuzzleRules: locked mechanical constraints.
- PuzzleValidator: placement, completion, board and region-map validation.
- PuzzleSolver: MRV-style backtracking solver with solution counting capped by a practical limit.
- PuzzleGenerator: deterministic candidate generation, region growth, uniqueness filtering and metadata creation.
- PuzzleDifficultyEvaluator: independently weighted structural and solver metrics.
- PuzzleFingerprint: structural identity and similarity foundation.
- PuzzleSerialization: JSON persistence contract.
- PuzzleSeed: stable progression and Daily seed derivation.
- ProductionPuzzleRepository: 500-level production-facing repository; Phase 1 PuzzleRepository.Get remains API-compatible for the current vertical slice.
- Editor/PuzzleGeneratorWindow: development-only candidate generation tool.

## Generation contract
Same seed + generation version + configuration produces the same candidate sequence and resulting puzzle. Production candidates are rejected when they are invalid, disconnected, unsolved, non-unique when uniqueness is required, or outside a requested difficulty band.

Supported generation sizes are 4x4, 5x5, 6x6 and 7x7. Region count equals grid dimension.

## Difficulty
Difficulty is a configurable 0-100 score composed of grid size, region complexity, candidate availability, forced moves, deduction depth, backtracking, ambiguity and region topology. Easy/Medium/Hard thresholds live in DifficultyProfile and are deliberately tunable.

## Known boundary
Unity 6.3 compilation, EditMode execution, device profiling and human playtesting require a local Unity editor session. The repository now contains the required automated tests and developer tools, but this environment cannot truthfully mark those runtime checks as executed.
