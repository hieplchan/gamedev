# AGENTS.md

This file defines persistent guidance for agents working in this repository.
Scope: entire repo rooted at `/workspace/gamedev`.

# Working branch
- sp23_mar261

## Project Context
- Unity project path: `water6/`
- Primary scene scripting area: `water6/Assets/Scenes/WaterScene/`
- Game direction: **boat survival + water gathering + rescue signaling**.

## Core Game Idea (Current Canon)
The player is lost at sea and must survive long enough to be rescued.

### Survival needs
- Player has a **Hunger** bar and a **Thirst** bar.
- Player dies if hunger or thirst is not maintained.

### Two primary goals
1. Catch fish and obtain drinkable water to survive.
2. Craft tools for survival and build visible rescue signals (e.g., smoke, red flag, flares) so ships/planes can detect the player.

### Non-goal (unless explicitly requested)
- Progression should **not** be centered around “diving deeper.”

## Onboarding / Narrative Direction
Preferred opening flow:
1. Player starts near/on water surface close to the boat.
2. Tutorial teaches return to boat and basic survival loop.
3. Early distant ship passes by without noticing player.
4. Prompt teaches rescue-signal objective.

## Implementation Priorities
1. Editor setup and object hierarchy clarity first.
2. Core movement/gameplay loop second.
3. Advanced interactions (threats, polish, VFX) later.

## Coding & Architecture Preferences
- Keep systems modular and inspector-tunable.
- Prefer simple, explicit state machines for gameplay loops.
- Avoid overengineering in early iterations.
- Keep scripts focused: one responsibility per component where practical.

## Agent Response Preferences
- Discuss idea first when user requests design discussion.
- If user asks “no code yet,” provide planning/setup only.
- For implementation requests, explain editor setup before code when applicable.
- Keep recommendations practical and incremental.
