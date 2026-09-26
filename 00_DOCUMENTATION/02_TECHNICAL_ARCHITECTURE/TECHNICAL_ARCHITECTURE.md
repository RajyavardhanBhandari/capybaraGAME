# Technical Architecture

## Target

Unity + C#.

Use a current supported Unity LTS release selected at project initialization. The exact editor version must be pinned in the repository and Unity version changes require an explicit migration decision.

## Architecture layers

### Presentation
UI, animation, VFX, audio, haptics.

### Gameplay
Puzzle state, placement validation, lives, rewards, progression.

### Domain
Puzzle model, constraints, solver, difficulty, economy rules.

### Services
Save service, authentication, leaderboard, analytics, remote configuration, ads, IAP.

### Infrastructure
Platform adapters, persistence, networking, serialization.

Gameplay/domain code must not depend directly on ad or platform SDK classes.

## Puzzle representation

Each puzzle is a deterministic data object containing:
- puzzle ID
- seed
- grid dimensions
- region map
- solution
- difficulty score
- difficulty band
- structural fingerprint
- content version

The shipped client may contain a signed/validated progression puzzle cache. Competitive Daily data must be server-authoritative.

## Determinism

Given the same puzzle seed + generator version, the puzzle must reproduce exactly.

Generator version is part of the puzzle identity.

## Save model

Local:
- progression
- unlocked characters
- cosmetic inventory
- coin balance
- settings
- streak metadata
- cached puzzle data

Server:
- account identity
- authoritative economy mutations
- Daily result
- leaderboard result
- entitlement/purchase records
- anti-cheat signals
- analytics ingestion where applicable

## Offline

Normal progression should remain playable offline where content is cached.

Daily competitive submission requires a server-validated timestamp/result. Offline Daily play may be allowed, but it is not leaderboard-eligible until the server can validate it, and the product should default to protecting competitive integrity over preserving an offline score.

## Backend abstraction

The game should use interfaces:
- IAuthService
- ISaveService
- ILeaderboardService
- IAnalyticsService
- IRemoteConfigService
- IAdsService
- IPurchaseService

This allows provider changes without rewriting gameplay.

## Security

Never trust client-submitted competitive scores, coin grants, ad rewards, or purchase entitlements.

Server validates:
- Daily puzzle ID/seed
- attempt eligibility
- timestamp
- score bounds
- reward idempotency
- account ownership
- purchase receipt/entitlement

## Project organization

Assets/
Scripts/
  Core/
  Puzzle/
  Progression/
  Economy/
  Daily/
  Leaderboard/
  Characters/
  UI/
  Audio/
  Services/
  Platform/
  Analytics/
Tests/
Editor/
Documentation/

## Testing

Automated tests required for:
- every puzzle constraint
- solver correctness
- uniqueness
- difficulty calculations
- reward idempotency
- life handling
- Daily eligibility
- leaderboard scoring
- offline/online synchronization
