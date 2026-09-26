# Launch Requirements

## Platforms

### iOS
iOS-first launch.

As of the current Phase 0 research in September 2026, Apple requires newly submitted apps/updates to use the current required SDK/toolchain and has a current minimum target of iOS 13+ for uploads under its published 2026 requirements. Verify the exact Xcode/SDK requirement again immediately before submission.

### Android
Android support from the beginning.

As of the current Phase 0 research in September 2026, Google Play requires new apps and updates to target Android 16 / API 36+ from August 31, 2026. Verify immediately before submission because platform requirements can change.

## Build

Pin:
- Unity editor version
- Android SDK/NDK versions as applicable
- Xcode version
- package/plugin versions

Commit lock/config files where applicable.

## QA gates

Functional:
- puzzle validation
- solver
- lives
- rewards
- Daily
- leaderboard
- shop
- ads
- purchases
- offline/online sync

UX:
- onboarding
- touch targets
- readability
- accessibility
- animations
- audio
- haptics

Technical:
- crash rate
- memory
- battery
- startup time
- frame rate
- network failure handling
- save corruption recovery

Security:
- reward duplication
- score tampering
- purchase verification
- leaderboard abuse
- client/server time manipulation

## Store readiness

Prepare:
- app name
- icon
- screenshots
- preview assets
- description
- privacy policy
- support URL
- age rating
- data safety/privacy disclosures
- account deletion flow if applicable
- IAP metadata
- ad disclosures where required

## Release strategy

Internal development build
→ internal QA
→ closed/beta testing
→ soft launch
→ telemetry review
→ production release

Do not treat a passing build as launch-ready until economy, puzzle difficulty, competitive integrity, and platform compliance have been validated.

## Final pre-submit check

Re-check current Apple and Google requirements immediately before store submission.
