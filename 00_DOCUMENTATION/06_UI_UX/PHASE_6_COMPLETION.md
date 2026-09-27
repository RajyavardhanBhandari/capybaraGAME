# Phase 6 — UI/UX Completion

## Status

Implemented on main.

## Launch progression flow

HOME
→ LEVEL READY
→ PLAYING
→ RESULT
→ NEXT LEVEL

No player-facing Level Select or progression map.

## Implemented

- Capybara-led home screen with animated mascot
- Current-level hero card
- Store access
- How To Play
- Settings
- Level Ready screen
- Active-character presentation
- Character-specific mistake resources
- Compact gameplay HUD
- Rule reminder cards
- Large centered puzzle board
- White X candidate marks
- Red X incorrect-placement feedback
- Broken-heart error animation
- Animated character placement
- Animated character idle motion
- Responsive button press feedback
- Reduced-motion path
- Solved result screen
- Failed result screen
- One permitted recovery choice flow
- Store-only aid purchasing
- No purchasing controls during active puzzle
- Coin/status pills
- Consistent rounded cards, pastel surfaces and shadows
- Original visual language inspired by supplied references without copying their artwork or branding

## Character animation language

Default face-only characters support:
- idle bob
- blinking
- occasional double blink
- subtle ear movement where applicable
- cheek movement
- placement pop
- error shake

## Error language

Normal X:
- white

Incorrect placement:
- red X
- short cell shake
- broken heart split/fade
- mistake resource decrement

## Responsive requirements

- portrait-first 1080x1920 reference layout
- CanvasScaler with screen-size scaling
- square puzzle board
- touch-friendly controls
- no essential information communicated by color alone

## Monetization UI constraints

- aids are bought in Store only
- aids are prepared before a puzzle
- no active-puzzle shopping
- normal failure may offer one rewarded-ad +1 resource or one 500-coin resource recovery
- Golden Challenge and competitive Daily remain protected from paid/ad advantage

## Unity verification required

Before calling Phase 6 production-ready, verify in Unity:
1. Home
2. Store
3. Level Ready
4. Gameplay
5. X marking/removal
6. double-tap placement
7. incorrect placement animation
8. solved result
9. failed recovery
10. next-level progression
11. reduced motion
12. iPhone portrait aspect ratio
13. Android portrait aspect ratio
