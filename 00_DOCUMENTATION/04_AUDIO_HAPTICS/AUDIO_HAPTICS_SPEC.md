# Audio and Haptics Specification

## Interaction sequence

Visual → Animation → Sound → Haptic → Reward.

## Required sound states

- valid placement
- invalid placement
- character removal
- life lost
- puzzle completion
- perfect completion
- character unlock
- coin reward
- Golden Challenge success/failure
- UI navigation
- purchase confirmation

## Character audio

Each launch character may have a restrained signature sound palette consistent with personality.

Capybara: relaxed/light.
Cat: playful.
Dog: energetic.
Penguin: comedic.
Panda: calm.

Avoid excessive repetition.

## Haptics

Use light feedback for:
- selection
- valid placement
- removal

Medium feedback for:
- invalid placement/life loss

Stronger feedback for:
- completion
- unlock

Provide a haptics setting and respect platform/user accessibility settings.

## Audio settings

Separate:
- master
- music
- SFX
- character sounds

Provide mute controls and persist preferences.

## Performance

Audio must not cause frame spikes, memory pressure, or delayed puzzle input.
