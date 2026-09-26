# Game Design Document

## Core experience

The player solves compact grid puzzles by placing animal characters under four rules: region uniqueness, row uniqueness, column uniqueness, and non-touching adjacency.

## State machine

BOOT
→ HOME
→ PROGRESSION SELECT
→ PUZZLE READY
→ PLAYING
→ {ERROR / PLACEMENT / REMOVE}
→ {SOLVED / FAILED}
→ REWARD
→ RESULTS
→ NEXT LEVEL or HOME

Separate:
HOME → DAILY → DAILY PLAY → DAILY RESULT
HOME → SHOP
HOME → LEADERBOARD
HOME → PROFILE

## Puzzle states

READY: board visible, 3 lives.
PLAYING: input accepted.
ERROR: invalid placement feedback; life decremented.
SOLVED: all constraints satisfied.
FAILED: lives reach zero.
REWARD: rewards calculated once.
RESULTS: completion summary.

## Input rules

Empty valid cell: place.
Empty invalid cell: reject placement and consume one life.
Occupied cell: remove.
Input during transition/celebration: ignored.

## Rewards

Progression rewards are granted once per completed level attempt according to configured reward tables.
Hard Challenge uses the same economy framework.
Golden Challenge uses Golden Treat rewards.
Daily competitive result is server-recorded.

## Accessibility

Minimum requirements:
- do not communicate state using color alone
- sufficient contrast
- scalable UI text where platform permits
- reduced motion option
- haptics toggle
- audio toggle
- clear touch targets
- readable error states
- no essential information hidden only in animation

## Anti-frustration principles

- mistakes are understandable
- feedback is immediate
- no hidden rule
- no forced monetization
- no irreversible accidental purchase
- no confusing life depletion
- no surprise timers

## Collection

Characters are collectible cosmetics. Unlocking a character changes presentation, not puzzle capability.

Shop must clearly separate:
- Characters
- Cosmetics
- Aids

Competitive rewards must never imply gameplay power.
