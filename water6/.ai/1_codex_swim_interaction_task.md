# Unity Task: Boat ↔ Swim Interaction System

## Project Context

This is a **Unity 6 prototype** for a boat survival game.

The player is stranded at sea and can:

* stand on a boat
* jump into the ocean to swim
* climb back onto the boat

The boat has a **SwimTriggerZone** collider that allows the player to switch between boat and swimming states.

This document describes the **Unity scene setup** and the **code to implement the interaction system**.

---

# 1. Unity Scene Setup

Open the scene:

```
Assets/Scenes/WaterScene.unity
```

Important hierarchy:

```
WaterScene
├── Water
├── Camera
│   ├── Camera_Top
│   └── Camera_Water
├── GameManager
│   └── AnimJumpToSwim
│       └── JumpToSwimPos
├── Canvas
│   └── BgImg
│       └── Text (TMP)
├── Boat
│   └── Hull
│       └── TriggerZones
│           └── SwimTriggerZone
```

Important objects:

### SwimTriggerZone

* Located on the **front half of the boat**
* Uses a **Collider with `Is Trigger = true`**

This trigger allows the player to:

* jump into the water
* climb back to the boat

### GameManager

Contains references for:

* UI
* camera switching
* animation sequence

### JumpToSwimPos

```
GameManager
└── AnimJumpToSwim
    └── JumpToSwimPos
```

This transform represents the **target position where the player lands when jumping into the water**.

---

# 2. Gameplay Behavior

The SwimTriggerZone supports **two interactions** depending on player state.

---

## Case 1 — Player On Boat

```
Player enters SwimTriggerZone
Press X
→ Player jumps into ocean
→ Play jump animation
→ Camera switches to underwater camera
→ Player state becomes swimming
```

---

## Case 2 — Player Swimming

```
Player enters SwimTriggerZone
Press X
→ Player jumps/climbs back to boat
→ Camera switches to top camera
→ Player state becomes on boat
```

---

# 3. Architecture

## PlayerController

PlayerController owns **player gameplay state**.

It must store:

```
bool IsPlayerInSwimTriggerZone
bool IsPlayerSwimming
```

Responsibilities:

* player movement
* trigger detection
* input handling
* jump physics
* player state

Expose public properties:

```
public bool IsPlayerSwimming { get; }
public bool IsPlayerInSwimTriggerZone { get; }
```

---

### PlayerController Event

PlayerController must notify other systems when the interaction state changes.

Create an event:

```
public event Action OnSwimInteractionStateChanged;
```

Trigger this event when:

* player enters swim trigger
* player exits swim trigger
* player starts swimming
* player climbs back to boat

---

# 4. GameManager Responsibilities

GameManager manages **game orchestration**:

* UI prompts
* camera switching
* animation sequence

GameManager subscribes to:

```
PlayerController.OnSwimInteractionStateChanged
```

Then updates the UI.

---

## UI Rules

```
if player not in trigger
    hide UI

if player in trigger AND player on boat
    show "X - Swim"

if player in trigger AND player swimming
    show "X - Climb"
```

---

# 5. Animation System

The project uses **Bruno Mikoski Animation Sequencer**.

GameManager already has a reference:

```
AnimationSequencerController _jumpToSwimAnimSequencerController
```

To trigger the animation:

```
_jumpToSwimAnimSequencerController.Play();
```

This animation handles the visual jump transition.

---

# 6. Jump Physics

PlayerController already has a function:

```
JumpToPos(Transform target)
```

This performs a **projectile motion jump** using physics.

Use it for:

```
jump into water
jump back to boat
```

---

# 7. Input

Use the **Unity Input System**.

Handle the **Action button (X)**.

Example function:

```
void OnAction(InputValue value)
```

When pressed:

```
if player not in swim trigger
    ignore input

if player on boat
    jump to water

if player swimming
    jump to boat
```

---

# 8. Camera Switching

GameManager has references:

```
Transform topCam
Transform underWaterCam
```

When swimming:

```
enable underwater camera
disable top camera
```

When returning to boat:

```
enable top camera
disable underwater camera
```

---

# 9. Expected Interaction Flow

```
Player enters swim trigger
→ UI shows X prompt

Press X
→ GameManager starts animation
→ PlayerController jumps to target
→ camera switches
→ player state updates
```

Returning to the boat works the same way.

---

# 10. Implementation Tasks

Implement the following scripts:

```
PlayerController.cs
GameManager.cs
```

PlayerController must implement:

* trigger detection
* player state variables
* event system
* input handling
* jump logic

GameManager must implement:

* UI prompt updates
* camera switching
* animation sequencer trigger
* event subscription

The goal is a **clean interaction system** where the same trigger allows both:

```
Boat → Swim
Swim → Boat
```
