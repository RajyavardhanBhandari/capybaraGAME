# Monetization Specification

## Principles

Monetization is optional acceleration and cosmetic collection. The active puzzle itself is never a shop.

## Store-only aid purchases

Hints, Reveals and Extra Berries/Mistake Tokens are **never purchased from the active gameplay screen**.

The Store is the only place where the player can spend coins on aids.

Aids may be purchased before starting a progression puzzle. The level-ready screen may show owned aids and allow the player to prepare them before pressing Play.

Once the puzzle begins:
- no aid purchase buttons
- no coin spending
- no character purchase
- no store overlay
- no buying a berry during active play

## Store

The Store contains:
- Characters / avatars
- Cosmetics
- Aids

Launch character purchases are cosmetic only.

## Failure recovery

When a normal progression puzzle reaches 0 mistake resources:
1. the attempt ends
2. offer **WATCH AD → +1 berry/resource** where a rewarded-ad provider is available
3. offer **BUY 1 berry/resource** for a deliberately expensive soft-currency price
4. offer normal retry

The purchase is for exactly **one** temporary mistake resource, not a bundle.

Starting soft-currency price:
- 1 berry/resource: **500 coins**

Only one recovery is possible per failed attempt because recovery returns the player to the active puzzle.

The rewarded-ad implementation must grant the berry only after the ad SDK confirms a completed rewarded view. Never fake ad completion in production.

## Rewarded ads

Allowed examples:
- +30 coins
- one post-failure berry/resource
- other configured non-competitive rewards

Starting design target:
- approximately 3 coin-reward opportunities/day
- maximum one ad-based recovery per normal puzzle attempt

No ad reward may affect Daily competitive score.

## Interstitials

Avoid interstitials during active puzzle solving.
Do not show one after every level.
If used, show only at natural breaks and make it clearly identifiable and dismissible in accordance with platform rules.

## Golden Challenge

No ad-based revival.
No coin purchase of a recovery during Golden Challenge.

## Daily

No paid or ad-based competitive advantage.

## Purchases

Launch may include direct digital purchases for approved cosmetics/characters and platform-compliant products.

No premium currency at launch.

## Purchase integrity

Verify platform receipts/entitlements for real-money purchases.
Make grants idempotent.
Never trust a client-only purchase success flag.

## Compliance

Before store submission, verify current Apple App Store and Google Play rules for:
- digital goods
- advertising
- privacy
- age rating
- tracking/consent
- billing
- required SDK/target versions
