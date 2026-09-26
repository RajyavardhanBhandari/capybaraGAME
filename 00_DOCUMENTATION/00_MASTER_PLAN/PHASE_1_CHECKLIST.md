# Phase 1 Task Checklist

## A. Project Foundation
- [ ] Pin supported Unity LTS version
- [ ] Create Unity project
- [ ] Configure iOS/Android targets
- [ ] Configure portrait orientation
- [ ] Configure safe-area handling
- [ ] Create folder structure
- [ ] Add assembly definitions where useful

## B. Core Domain
- [ ] PuzzleDefinition
- [ ] PuzzleState
- [ ] CharacterDefinition
- [ ] Puzzle validator
- [ ] Placement service
- [ ] Life manager
- [ ] Reward calculator
- [ ] Progression manager

## C. Puzzle
- [ ] Grid renderer
- [ ] Region renderer
- [ ] Cell input
- [ ] Placement
- [ ] Removal
- [ ] Invalid placement feedback
- [ ] Completion detection
- [ ] Failure detection

## D. UI
- [ ] Boot
- [ ] Home
- [ ] Level select
- [ ] Level ready
- [ ] Gameplay
- [ ] Result
- [ ] Settings

## E. Character System
- [ ] Capybara
- [ ] Cat
- [ ] Dog
- [ ] Penguin
- [ ] Panda
- [ ] Character selector
- [ ] Selected state
- [ ] Placement state

## F. Persistence
- [ ] Save progression
- [ ] Save unlocked characters
- [ ] Save active character
- [ ] Save settings
- [ ] Load on boot
- [ ] Corruption-safe defaults

## G. Tests
- [ ] Constraint tests
- [ ] Touching tests
- [ ] Placement tests
- [ ] Life tests
- [ ] Completion tests
- [ ] Reward idempotency
- [ ] Progression unlock tests

## H. Polish
- [ ] Transitions
- [ ] Haptics abstraction
- [ ] Audio abstraction
- [ ] Reduced motion
- [ ] Error states
- [ ] Safe-area QA
- [ ] Small-device QA

## I. Verification
- [ ] Fresh install test
- [ ] Restart test
- [ ] Solve test
- [ ] Fail test
- [ ] Retry test
- [ ] Offline test
- [ ] Build test
