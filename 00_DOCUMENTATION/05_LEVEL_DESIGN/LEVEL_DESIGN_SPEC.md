# Level Design and Puzzle Engine Specification

## Generation pipeline

Generate
→ validate constraints
→ solve
→ verify uniqueness
→ calculate difficulty
→ calculate fingerprint
→ deduplicate
→ assign band
→ curate
→ playtest
→ production pool

## Validity

Every production puzzle must have:
- at least one valid solution
- exactly one intended solution unless explicitly approved otherwise
- valid region assignment
- no contradictory starting state
- deterministic reproduction
- known difficulty score

## Difficulty inputs

Difficulty score should be derived from measurable features:
- grid size
- region count
- region complexity
- candidate count
- clue density
- forced moves
- deduction depth
- estimated solve time
- plausible wrong moves
- structural complexity
- mistake probability

The first implementation should expose each component independently so weights can be tuned from playtest data.

## Difficulty bands

Easy: introductory and low-friction puzzles.
Medium: normal progression.
Hard: challenge-level deduction.

Do not use grid size alone to determine difficulty.

## Progression curve

500 levels with deliberate pacing. Every tenth level is a Hard Challenge.

The curve should mix:
- teaching
- consolidation
- escalation
- relief
- challenge

Avoid monotonically increasing difficulty.

## Fingerprinting

Fingerprint should capture structural characteristics rather than raw cell coordinates so near-duplicate puzzles can be removed.

## Test requirements

A generator test suite must verify:
- constraint validity
- solver finds solution
- uniqueness
- deterministic seed reproduction
- fingerprint stability
- duplicate rejection
- difficulty score reproducibility
- generation failure handling

## Content pipeline

The game should ship a curated production pool rather than generate arbitrary new content directly in the player session.

Runtime generation may be used only where it preserves validation, deterministic behavior, difficulty controls, and competitive integrity.
