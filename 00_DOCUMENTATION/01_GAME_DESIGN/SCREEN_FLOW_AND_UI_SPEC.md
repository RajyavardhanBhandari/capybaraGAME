# Screen Structure & Gameplay Flow

## Reference Translation

The supplied visual reference establishes the target interaction language: a compact mobile puzzle board, soft rounded UI, pastel regions, expressive animal pieces, immediate X-mark feedback, simple rule cards, and lightweight bottom actions.

The reference is inspiration only. All production art must be original and must follow the project's art-direction rules.

## 1. App Flow

BOOT
→ HOME
→ PLAY CURRENT LEVEL
→ LEVEL READY
→ PLAYING
→ RESULT
→ NEXT LEVEL

Secondary screens:
- STORE
- HOW TO PLAY
- SETTINGS

Daily/competitive screens are reserved for their later feature phases.

### Progression
HOME
→ LEVEL READY
→ PLAYING
→ placement / removal / error
→ SOLVED or FAILED
→ REWARD
→ RESULTS
→ NEXT LEVEL or HOME

### Daily
HOME
→ DAILY INTRO
→ DAILY PLAY
→ DAILY RESULT
→ WEEKLY POSITION
→ HOME

### Collection
HOME
→ SHOP
→ CHARACTER / COSMETIC / AID
→ ITEM DETAIL
→ PURCHASE / UNLOCK
→ COLLECTION

### Competitive
HOME
→ LEADERBOARD
→ WEEKLY RANKING
→ PLAYER DETAIL
→ HOME

## 2. Home Screen

### Header
- Profile/avatar button, left
- Coin balance, right
- Settings button, right

### Primary content
- Current progression level
- Large "Play" button
- Current streak
- Daily Puzzle card
- Weekly leaderboard preview

### Navigation
Launch UI keeps the progression loop focused:
- Play current level
- Store
- How To Play
- Settings

Daily, leaderboard and profile navigation are added in their respective feature phases rather than crowding the initial progression loop.

Do not overcrowd the home screen.

## 3. Sequential Progression

There is no player-facing level-select screen or progression map.

The player:
1. plays the currently unlocked level
2. completes or fails it
3. on completion advances to the next level
4. can never jump to another level from a selector

Every 10th level receives Hard Challenge presentation while remaining part of the same sequential flow.

## 4. Level Ready Screen

Show:
- Level number
- difficulty band
- 3 mistake tokens shown as the active character's resource
- active character
- short rule reminder
- Play button

Rule reminder:
- 1 per region
- 1 per row
- 1 per column
- no touching

Do not add tutorial copy once the player has demonstrated understanding.

## 5. Gameplay Screen

### Top bar
- Pause
- Puzzle number / Daily label
- Character-specific mistake resource
- Settings

### Board
- Large centered grid
- Rounded cells
- Clearly separated colored regions
- Character centered inside occupied cells
- X marks for eliminated/invalid cells where appropriate

### Bottom action row
No purchasing or aid controls during active play.
Only instructional feedback and the coin balance may appear.

Aids are purchased in the Store before the puzzle starts and may be prepared on the Level Ready screen.

### Active character selector
Show the selected character as a small, clear control. Character selection changes appearance only.

### Core interactions
- Empty cell → single tap marks an X
- Empty cell → double-tap attempts to place the active character
- Occupied character → tap removes it
- Invalid double-tap → error + lose one character-specific mistake token

- Input is ignored during animations

### Placement feedback
Correct:
1. character pop-in
2. tiny scale settle
3. soft haptic
4. subtle success sound

Invalid:
1. short cell shake
2. clear X/error state
3. life decrement
4. short warning haptic/sound

Never use harsh punishment visuals.

## 6. Solved State

Immediately lock input.

Animation sequence:
1. remaining placements settle
2. board celebrates
3. character performs completion reaction
4. reward panel appears

Show:
- completion
- lives remaining
- coins earned
- Treats if applicable
- next level button

Perfect completion receives a stronger celebration but never changes puzzle rules.

## 7. Failed State

When lives reach zero:
- lock input
- show failed result
- explain that the attempt ended
- offer normal retry
- offer at most one permitted ad-based extra life for progression
- never offer ad revival for Golden Challenge
- Daily competitive score cannot be improved through revival

## 8. Daily Screen

Header:
- Daily Puzzle
- date/season identifier
- streak

Body:
- today's shared puzzle
- current submitted status
- weekly Treat total

Before first play:
- explain that the first eligible completed attempt is ranked

After submission:
- show result
- show weekly position
- prevent replay from replacing the ranked result

Daily aids may be available only if their use does not create competitive score advantage.

## 9. Leaderboard

Tabs:
- This Week
- My Position

Rows:
- rank
- avatar
- player name
- Treat score

Tie-break behavior is server-defined:
1. perfect Daily count
2. earlier final qualifying completion
3. stable server player ID

Never display pay/ad advantages as ranking mechanics.

## 10. Shop

Three explicit tabs:

### Characters
- Capybara
- Cat
- Dog
- Penguin
- Panda

### Cosmetics
- themes
- backgrounds
- avatar presentation

### Aids
- Hint
- Reveal
- Recovery resource

Character cards show:
- portrait
- name
- rarity
- coin price
- owned/equipped state

No character stats.

## 11. Profile

Show:
- avatar
- display name
- progression level
- current streak
- puzzles completed
- perfect completions
- characters collected
- cosmetics collected
- settings

## 12. Settings

- Sound on/off
- Music on/off
- Haptics on/off
- Reduced motion
- Accessibility/text scaling where supported
- Restore purchases
- Privacy
- Terms
- Support

## 13. Tutorial

First launch should take approximately 10–20 seconds.

Four visual cards:
1. One character per colored region
2. One character per row
3. One character per column
4. Characters cannot touch, including diagonally

Finish with one guided placement.

No long text tutorial.

## 14. Visual System

### Palette direction
- warm off-white background
- soft pastel region colors
- dark neutral text
- white/near-white cards
- restrained accent color for primary CTA
- red/orange only for error states

### Geometry
- rounded cards
- rounded cells
- generous spacing
- large touch targets
- consistent corner radius

### Typography
- rounded contemporary sans-serif
- strong hierarchy
- avoid thin weights for gameplay information

### Board
The board must dominate the gameplay screen.

Target hierarchy:
Board > lives/status > actions > secondary information.

## 15. Accessibility

Never communicate a critical state through color alone.

Use:
- X marks
- outlines
- icons
- labels
- shape differences
- animation with reduced-motion alternative

Minimum touch targets should be comfortable for one-handed mobile play.

## 16. Animation Language

Global:
- fast
- soft
- slightly bouncy
- never excessive

Suggested timing:
- micro interaction: 80–160 ms
- placement: 160–240 ms
- panel transition: 180–280 ms
- celebration: 500–900 ms

Respect reduced-motion settings.

## 17. Audio / Haptics Hooks

Events:
- button tap
- valid placement
- remove
- invalid placement
- life lost
- level complete
- perfect complete
- unlock
- reward
- daily submitted

Every event must have an audio/haptic-off path.

## 18. Monetization Placement

Never interrupt active puzzle solving with forced ads.

Rewarded ad entry points:
- progression extra mistake token
- optional coin reward

Limits remain configuration-driven and follow the master plan.

No ad:
- on Daily competitive score
- for Golden Challenge revival
- as a ranking advantage

## 19. Responsive Layout

Primary target:
- iPhone portrait

The board must scale to available width while preserving square cells.

Secondary target:
- Android portrait

Avoid placing essential controls inside unsafe areas.

## 20. Screen Acceptance Checklist

A screen is accepted only when:
- core action is obvious within 2 seconds
- no essential information depends on color alone
- touch targets are comfortable
- board remains readable at smallest supported phone
- animations do not block input longer than necessary
- no monetization element obscures the puzzle
- visual language remains consistent with the capybara-led identity
- all states have loading, empty, error, success, and reduced-motion behavior where relevant

## 21. Reference Mapping

Reference element → production interpretation:

- compact colored board → central puzzle board
- cat pieces → active collectible animal system
- X marks → constraint/elimination feedback
- rule cards → onboarding/rule reminder
- bottom character/hint controls → gameplay action bar
- score/competition cues → Daily + Weekly leaderboard
- soft pastel presentation → original game-wide visual language

Do not copy the reference's exact UI, artwork, character designs, typography, icons, or layout.
