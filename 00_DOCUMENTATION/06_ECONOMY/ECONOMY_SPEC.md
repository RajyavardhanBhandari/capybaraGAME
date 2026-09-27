# Economy Specification

## Resources

### Coins
Spendable soft currency.

Sources:
- progression completion
- configured daily activities
- optional rewarded ads within limits
- weekly rewards
- approved events

Sinks:
- character unlocks
- cosmetics
- Store-only Hint purchases
- Store-only Reveal purchases
- post-failure single-resource recovery

### Character-specific mistake resources
Three per normal puzzle attempt.

They are displayed using the active character's resource:
- Capybara: berries
- Cat: fish
- Dog: bones
- Penguin: fish
- Panda: bamboo

They are consumed by incorrect placement. They are not an energy system.

Recovery after failure:
- watch one rewarded ad for +1 resource where available, or
- buy exactly 1 resource for 500 coins

Recovery is only offered after the attempt reaches zero resources.

### Treats
Competitive/performance points.

Treats are not spendable and cannot be purchased.

Normal completion:
3 remaining lives = 3 Treats
2 = 2
1 = 1
0/failure = 0

Golden Challenge success = +5 Golden Treats for the active character.

## Starting prices

Common character: 500–1,000 coins.
Rare: 1,500–2,500.
Epic: 3,000–5,000.

Hint: ~100 coins per aid.
Reveal: ~175 coins per aid.
Single post-failure berry/resource: 500 coins.

These are tuning ranges, not final prices.

## Target earning

Casual: ~150–250 coins/day.
Regular: ~250–400.
Highly active: ~400–600.

The economy must be simulated before launch to ensure:
- first non-default character is attainable
- long-term collection remains meaningful
- ads do not dominate progression
- players do not routinely receive excessive currency
- sinks remain relevant

## Economy rules

Every grant/spend creates an auditable transaction.
All reward claims are idempotent.
Server-authoritative balances are used for online accounts.
Remote config controls tunable amounts.

## Anti-inflation

Do not introduce premium currency at launch.
Do not add duplicate character fragments.
Do not create unlimited ad currency loops.
Do not allow competitive Treats to become a spendable resource.
