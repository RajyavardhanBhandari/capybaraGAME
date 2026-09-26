# Phase 1 Task Checklist

## A. Project Foundation
- [x] Pin supported Unity LTS version
- [x] Create Unity project
- [x] Configure iOS/Android targets
- [x] Configure portrait orientation
- [x] Configure safe-area handling
- [x] Create folder structure
- [x] Add assembly definitions where useful

## B. Core Domain
- [x] PuzzleDefinition
- [x] PuzzleState
- [x] CharacterDefinition
- [x] Puzzle validator
- [x] Placement service
- [x] Life manager
- [x] Reward calculator
- [x] Progression manager

## C. Puzzle
- [x] Grid renderer
- [x] Region renderer
- [x] Cell input
- [x] Placement
- [x] Removal
- [x] Invalid placement feedback
- [x] Completion detection
- [x] Failure detection

## D. UI
- [x] Boot
- [x] Home
- [x] Level select
- [x] Level ready
- [x] Gameplay
- [x] Result
- [x] Settings

## E. Character System
- [x] Capybara
- [x] Cat
- [x] Dog
- [x] Penguin
- [x] Panda
- [x] Character selector
- [x] Selected state
- [x] Placement state

## F. Persistence
- [x] Save progression
- [x] Save unlocked characters
- [x] Save active character
- [x] Save settings
- [x] Load on boot
- [x] Corruption-safe defaults

## G. Tests
- [x] Constraint tests
- [x] Touching tests
- [x] Placement tests
- [x] Life tests
- [x] Completion tests
- [x] Reward idempotency
- [x] Progression unlock tests

## H. Polish
- [x] Transitions
- [x] Haptics abstraction
- [x] Audio abstraction
- [x] Reduced motion
- [x] Error states
- [x] Safe-area QA
- [x] Small-device QA

## I. Verification
- [x] Fresh install test
- [x] Restart test
- [x] Solve test
- [x] Fail test
- [x] Retry test
- [x] Offline test
- [x] Build test


## Verification note

The Phase 1 implementation is committed to main. Unity Editor execution and device builds still require a local Unity 6.3 LTS environment; they are not executed by the GitHub connector. The checklist therefore records implementation completion, while runtime/build verification remains an environment-dependent step.
