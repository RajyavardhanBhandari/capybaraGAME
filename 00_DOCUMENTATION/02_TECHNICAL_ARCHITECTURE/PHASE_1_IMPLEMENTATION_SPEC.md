# Phase 1 Implementation Specification

## Objective

Turn the approved Phase 0 product definition and the supplied visual reference into the first playable vertical slice.

Phase 1 is not the final production build. It proves the complete core loop end-to-end with original art placeholders:

BOOT → HOME → LEVEL → PLAY → SOLVE/FAIL → REWARD → NEXT LEVEL.

## Phase 1 Scope

### Must ship
- Unity project
- Mobile portrait layout
- Home screen
- Progression level selection
- One playable puzzle
- Deterministic puzzle data
- Four-rule validator
- 3-life system
- place/remove interaction
- invalid-placement feedback
- solved state
- failed state
- reward result
- next-level progression
- active character selection
- original placeholder character assets
- settings shell
- local save
- automated puzzle-rule tests

### Explicitly defer
- real backend
- authentication
- real leaderboard
- real ads
- IAP
- remote config provider
- production analytics provider
- final character art
- final audio
- 500-level content package
- App Store submission

Interfaces/stubs must exist where later systems will connect.

## Recommended Unity Structure

Assets/
  Art/
    Characters/
    UI/
    Board/
  Audio/
  Prefabs/
  Scenes/
    Boot.unity
    Home.unity
    Gameplay.unity
  ScriptableObjects/
    Characters/
    PuzzleContent/
  Scripts/
    Core/
    Puzzle/
    Progression/
    Characters/
    Economy/
    UI/
    Services/
    Platform/
    Audio/
  Tests/
    EditMode/
    PlayMode/
  Documentation/

## Core Domain Objects

### PuzzleDefinition
- id
- seed
- rows
- columns
- regionMap
- solutionCells
- difficulty
- difficultyBand
- generatorVersion
- fingerprint

### PuzzleState
- puzzleId
- placedCells
- livesRemaining
- status
- elapsedTime
- aidUsage
- completionData

### CharacterDefinition
- id
- displayName
- rarity
- unlockedByDefault
- art references
- audio references

Characters have no gameplay modifiers.

## Validator

Implement pure C# validation methods:

IsRegionValid()
IsRowValid()
IsColumnValid()
IsNonTouchingValid()
IsPlacementValid()

Final completion requires:
- every region has exactly one character
- every row has exactly one character
- every column has exactly one character
- no two characters are adjacent horizontally, vertically, or diagonally

Invalid placement must be detected before mutating the placed state.

## Board Input

Tap empty cell:
- calculate candidate validity
- if valid: place
- if invalid: do not place, consume one life, show feedback

Tap occupied cell:
- remove

Lives:
- start at 3
- minimum 0
- decrement once per invalid placement
- reaching 0 transitions to FAILED
- lives are per attempt, never a global energy system

## Completion

After every valid placement:
1. update state
2. evaluate completion
3. if complete, transition SOLVED
4. calculate rewards exactly once
5. disable gameplay input
6. show result

Reward processing must be idempotent even in the Phase 1 local implementation.

## UI Implementation Order

1. Safe-area/root canvas
2. Home
3. Level selection
4. Gameplay top bar
5. Board
6. Cell interaction
7. Character placement
8. Lives
9. Actions
10. Result overlay
11. Settings

## Gameplay Screen Layout

Portrait:

Top:
- back
- level
- lives
- settings

Center:
- puzzle board

Below board:
- active character
- hint
- reveal
- extra life

Bottom:
- lightweight secondary information only

The board receives the largest visual area.

## Placeholder Art

Use original simple vector/2D placeholders.

Required:
- Capybara
- Cat
- Dog
- Penguin
- Panda

Each placeholder must already support:
- idle
- selected
- placed
- wrong
- success

Do not import or trace the reference image's characters.

## First Puzzle Content

Create at least:
- Puzzle P001
- Puzzle P002
- Puzzle P003

All must be manually verified against the validator.

At least one should be an easy onboarding puzzle.

## Test Requirements

### Validator tests
- valid solved board accepted
- duplicate region rejected
- duplicate row rejected
- duplicate column rejected
- horizontal touching rejected
- vertical touching rejected
- diagonal touching rejected
- valid non-touching placement accepted

### State tests
- starts with 3 lives
- invalid placement loses exactly 1 life
- valid placement does not lose life
- occupied tap removes piece
- zero lives enters FAILED
- solved puzzle enters SOLVED
- reward cannot grant twice

### Progression tests
- completion unlocks next level
- failed attempt does not unlock next level
- already unlocked level remains unlocked

## Phase 1 Definition of Done

A developer can launch the Unity project and:

1. reach Home
2. start Level 1
3. understand the four rules
4. place an animal
5. receive immediate feedback
6. make mistakes and lose lives
7. remove a placed animal
8. solve the puzzle
9. see a reward
10. advance to the next puzzle
11. close/reopen and retain progression

Automated tests pass.

## Quality Bar

The vertical slice must feel like a real mobile game, not a debug prototype.

Required:
- no placeholder debug text
- no raw Unity default buttons where avoidable
- responsive touch
- polished transitions
- readable board
- consistent spacing
- no visible exceptions/errors
- no accidental double placement
- safe-area support
- reduced-motion setting wired into animations

## Phase 1 Engineering Rule

Do not implement new gameplay mechanics merely because they seem fun.

If a proposed mechanic changes:
- placement rules
- character capabilities
- lives
- scoring
- board behavior

it is a scope change and must be documented before implementation.
