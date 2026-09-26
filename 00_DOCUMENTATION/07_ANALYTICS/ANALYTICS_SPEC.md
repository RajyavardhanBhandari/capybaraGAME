# Analytics Specification

## Goals

Measure:
- onboarding completion
- puzzle engagement
- difficulty
- retention
- economy health
- monetization
- Daily participation
- leaderboard participation
- collection behavior
- technical quality

## Core events

app_open
session_start
onboarding_started
onboarding_completed
level_started
cell_tapped
character_placed
character_removed
invalid_placement
life_lost
hint_used
reveal_used
extra_life_used
level_completed
level_failed
reward_granted
character_unlocked
shop_viewed
item_purchased
ad_offer_shown
ad_started
ad_completed
ad_reward_granted
daily_started
daily_completed
daily_submitted
leaderboard_viewed
weekly_reward_claimed
settings_changed
purchase_started
purchase_verified
crash
error

## Required properties

Common:
player_id
session_id
timestamp
app_version
platform
country/region where permitted
build/environment

Puzzle:
puzzle_id
level
grid_size
difficulty_score
difficulty_band
lives_before
lives_after
completion_time
hint_used
reveal_used

Economy:
currency
amount
source
sink
balance_before
balance_after
reference_id

## Privacy

Collect only what is necessary for product operation and analytics.
Do not collect unnecessary sensitive personal data.
Provide required platform privacy disclosures and consent flows.
Analytics must not alter gameplay or competitive outcomes.

## Key dashboards

- onboarding funnel
- level funnel
- failure rate by difficulty
- average lives remaining
- completion time
- hint/reveal usage
- coin sources/sinks
- ad conversion
- Daily participation
- weekly participation
- retention
- crash-free sessions
