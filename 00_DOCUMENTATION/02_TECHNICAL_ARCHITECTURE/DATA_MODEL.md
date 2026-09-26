# Data Model

## Player

player_id
created_at
last_seen_at
display_name
selected_character_id
selected_cosmetic_id
progression_level
streak
coins
settings_version

## Character

character_id
display_name
rarity
unlock_cost
is_default
asset_version

## Puzzle

puzzle_id
seed
generator_version
grid_size
region_count
solution_hash
difficulty_score
difficulty_band
fingerprint
content_version

## ProgressionResult

player_id
puzzle_id
attempt_id
started_at
completed_at
lives_remaining
status
reward_claimed
client_version

## DailyResult

player_id
daily_id
puzzle_id
completed_at
lives_remaining
treat_score
perfect_flag
eligibility_status
server_validation_status

## WeeklyLeaderboard

week_id
player_id
daily_score_total
perfect_day_count
final_qualifying_timestamp
rank
reward_status

## EconomyTransaction

transaction_id
player_id
type
amount
currency
source
reference_id
created_at
idempotency_key

All currency mutations must be represented as auditable transactions.

## Purchase

purchase_id
player_id
platform
product_id
transaction_reference
status
created_at
verified_at

## AnalyticsEvent

event_id
player_id
session_id
event_name
timestamp
app_version
platform
properties

Competitive and economy-critical fields are server-derived where possible.
