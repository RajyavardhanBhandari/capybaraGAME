# Capybara Game

A cute mobile puzzle game built around four simple rules:

1. One character per colored region.
2. One character per row.
3. One character per column.
4. Characters cannot touch, including diagonally.

## Phase 1 vertical slice

The repository now contains a playable Unity vertical slice with:

- Home screen
- Progression levels
- 10×10 puzzle board
- Deterministic puzzle content
- 3 lives per attempt
- Place/remove interaction
- Invalid placement life loss
- Solve/fail results
- Coin rewards
- Hint / reveal / extra-life economy hooks
- Character selection
- Shop
- Daily Puzzle shell
- Weekly leaderboard shell
- Profile/settings shell
- Local persistence
- Edit-mode puzzle tests

## Unity

Pinned editor:

Unity 6000.3.16f1, Unity 6.3 LTS

Unity 6.3 LTS is the production baseline for this new project.

Open the repository in Unity Hub, allow packages to resolve, then open:

Assets/Scenes/Main.unity

The game bootstraps its Phase 1 UI at runtime, so no prefab setup is required for the vertical slice.

## Project structure

Assets/Scripts/Core
Domain types and character catalog.

Assets/Scripts/Puzzle
Puzzle data and four-rule validator.

Assets/Scripts/Characters
Character selection.

Assets/Scripts/Services
Local persistence.

Assets/Scripts/GameBootstrap.cs
Phase 1 runtime UI and gameplay flow.

Assets/Tests/EditMode
Automated puzzle tests.

00_DOCUMENTATION
Product, UX, technical, art and Phase 1 specifications.

## Current status

### Completed
- Phase 0 product foundation
- Phase 1 design/specification
- Phase 1 playable vertical slice implementation

### Phase 5 in progress
- Configurable 500-level progression model
- 50 explicit Hard Challenges
- Deterministic candidate batch generation
- Difficulty and structural-variety selection
- Persistent LevelDatabase/PuzzleDatabase architecture
- Level generator, preview and validation editor tools

### Not yet production-ready
- Final character artwork
- Final audio/haptics assets
- Phase 5 level-system architecture and offline generation tooling
- 500 curated production puzzles
- Authentication/backend
- Server-authoritative Daily
- Real leaderboard
- Ads
- IAP
- Production analytics
- App Store / Play Store configuration

Those systems are deliberately deferred behind the documented architecture so they can be added without changing the core puzzle rules.

## Design principle

Simple puzzle. Deep ecosystem.

Characters are cosmetic. Competitive systems cannot provide gameplay advantages through purchases or ads.
