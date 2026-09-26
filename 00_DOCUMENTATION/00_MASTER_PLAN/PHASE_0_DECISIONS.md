# Phase 0 Decision Log

## Decision 001: Core puzzle
Locked to four rules:
1. one character per colored region
2. one per row
3. one per column
4. no touching, including diagonals

## Decision 002: Lives
Three lives per normal puzzle attempt. Lives are not energy and do not block entry into another level.

## Decision 003: Daily competition
The first eligible Daily result is the ranked result. Score equals Treats earned from remaining lives. No aid, ad, purchase, or replay can improve that competitive score.

## Decision 004: Weekly ranking
Weekly score is the sum of eligible Daily scores. Perfect-day count and then final qualifying completion timestamp break ties.

## Decision 005: Daily personalization
Daily Puzzle is shared and identical for everyone. Progression may personalize puzzle selection; Daily does not.

## Decision 006: Content generation
500 levels are produced through an offline generation/validation/curation pipeline. Runtime generation is not the source of truth for launch progression.

## Decision 007: Backend trust
Competitive scores, economy-critical grants, purchases, and Daily eligibility are server-authoritative.

## Decision 008: Economy
Coins are spendable. Treats are competitive points. Lives are attempt-level mistake budget. No premium currency at launch.

## Decision 009: Monetization
Ads are optional accelerators. No forced active-gameplay ads, no competitive ad advantage, no Golden Challenge revival.

## Decision 010: Platform
Unity + C#, iOS first, Android supported by shared architecture.

## Decision 011: Scope control
No additional puzzle mechanics at launch. Any request for a new mechanic must be treated as a scope change and reviewed against the core philosophy.

## Decision 012: Tunability
Prices, reward quantities, difficulty weights, ad caps, and presentation timing are configuration values, not hard-coded assumptions.
