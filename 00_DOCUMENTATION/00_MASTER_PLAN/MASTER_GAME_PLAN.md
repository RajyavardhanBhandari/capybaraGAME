# Master Game Plan / Game Bible

## 1. Product

A friendly, polished capybara-led puzzle game built around a simple placement puzzle and a deeper surrounding ecosystem.

**Product principle:** Simple puzzle. Deep ecosystem.

## 2. Core loop

Open game → play the current progression puzzle → mark candidates with X → double-tap to place the active animal → mistakes consume character-specific berries/tokens → solve → receive coins/Treats → automatically advance → collect/customize → return for Daily Puzzle → participate in weekly competition.

## 3. Puzzle rules

A puzzle contains colored regions and cells.

A valid solution must satisfy:
- one character per colored region
- one character per row
- one character per column
- no two placed characters may touch, including diagonally

Input:
- tap empty cell = mark/remove an X
- tap occupied animal = remove it
- double-tap empty cell = attempt to place the active character
- invalid placement = error feedback + one character-specific mistake token
- zero mistake tokens = failed attempt

No alternate rules at launch.

## 4. Progression

500 levels at launch.

Levels are generated, validated, solved, difficulty-scored, fingerprinted, deduplicated, curated, and playtested before production use. Progression is strictly sequential. There is no level-select screen and no manual level jumping in the player-facing flow.

Every 10th level is a Hard Challenge.

## 5. Hard Challenge

Levels 10, 20, 30 ... 500.

Same rules, higher difficulty target, stronger presentation, normal progression rewards.

## 6. Golden Challenge

Optional challenge presented separately from normal progression.

- one life
- success: +5 Golden Treats for the active character
- failure: 0
- skip: no penalty
- no ad-based extra life
- golden treat uses the normal character's collectible identity

Golden Challenge does not change the core puzzle rules.

## 7. Daily Puzzle

One shared puzzle/seed every 24 hours.

The Daily is separate from progression and uses the same core puzzle rules.

Eligibility:
- first completed eligible attempt counts
- replay does not replace a submitted competitive result
- aids cannot increase competitive score
- ads cannot create competitive advantage
- daily score is based on remaining lives/Treats

## 8. Weekly leaderboard

Weekly score = sum of eligible Daily Treat scores.

Week resets on a server-defined UTC boundary.

Tie-break:
1. perfect Daily count
2. earlier final qualifying completion
3. stable server player ID

Rewards are cosmetic/soft-economy rewards and never create competitive power.

## 9. Resources

### Coins
Spendable soft currency.

Uses:
- character unlocks
- cosmetics
- gameplay aids

### Puzzle Mistake Tokens
Three per normal puzzle attempt, presented using the active character's resource rather than the word “lives”. Capybara uses berries, Cat/Penguin use fish, Dog uses bones, Panda uses bamboo. These are temporary mistake protection and are not spendable currency.

### Treats
Performance/competitive score. Not spendable.

## 10. Launch characters

| Character | Personality |
|---|---|
| Capybara | Chill, goofy, relaxed |
| Cat | Playful, slightly sassy |
| Dog | Energetic |
| Penguin | Cute/comedic |
| Panda | Calm/sleepy |

Capybara is the default/free character.

## 11. Economy

Starting tuning assumptions:
- common character: 500–1,000 coins
- rare character: 1,500–2,500
- epic character: 3,000–5,000
- Hint: ~100
- Reveal: ~175
- Extra Life: ~250

Target earning:
- casual: ~150–250 coins/day
- regular: ~250–400
- highly active: ~400–600

All economy values are remote/configurable.

## 12. Monetization

Launch monetization:
- rewarded ads
- optional cosmetic purchases
- optional character purchases where approved
- no premium currency

Constraints:
- no forced ad during active gameplay
- no interstitial after every level
- no competitive ad advantage
- no Golden Challenge revival through ads
- no pay-to-win
- maximum one ad-based extra life per normal puzzle
- initial target of about three coin-rewarded ads/day

## 13. UX

Home:
current level, Capybara mascot, Play, Store, How To Play, Settings, coins, progress snapshot.

Gameplay:
level, active character, character-specific mistake resource, rule reminder, board, interaction guidance, coins.

Progression is strictly sequential. There is no player-facing Level Select or progression map.

Aids are purchased only in Store and prepared before the puzzle. Active gameplay has no purchasing controls.

Onboarding explains only:
- one per region
- one per row
- one per column
- characters cannot touch

Target onboarding: 10–20 seconds.

## 14. Personalization

Allowed:
- character
- avatar
- themes
- backgrounds
- audio
- haptics
- animation intensity

Progression puzzle selection may adapt to player performance. Daily Puzzle never adapts.

## 15. Game feel

Core interaction:
Visual → Animation → Sound → Haptic → Reward.

Correct placement: positive feedback.
Incorrect placement: clear but non-punitive feedback.
Completion: stronger celebration.
Perfect completion: enhanced celebration.
Unlock: strong collectible reveal.

## 16. Technical target

Unity + C#.

Architecture:
- shared gameplay code
- deterministic puzzle logic
- data-driven content
- server-authoritative competitive results
- offline-capable progression
- online synchronization for Daily/leaderboard
- remote-configurable economy and tuning

## 17. Launch platforms

iOS first.
Android supported by shared architecture.

## 18. Quality bar

The game should feel:
- simple
- friendly
- polished
- responsive
- readable
- satisfying
- fair
- collection-driven
- competitive without pay-to-win

## 19. Explicit non-goals

Do not add:
- match-3
- bombs
- special tiles
- moving obstacles
- timers
- energy systems
- premium currency
- gameplay-affecting character stats
- paid competitive advantage
- unnecessary meta complexity
