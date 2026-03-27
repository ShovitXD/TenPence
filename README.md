# TenPence
Game Jam 

Shovit-

# Room Time System Prototype

A Unity prototype built around a multi-room time simulation system where each room progresses through time at different rates depending on the player's current location.

This project includes:
- Room-based space detection
- Relative teleportation between rooms
- Independent year simulation per room
- Trigger events based on accumulated years
- Food state progression from raw to aged to spoiled

---

## Overview

The core idea of this prototype is that each room exists in a different time context.

The player can move between rooms, and depending on which room they are currently in, time progresses differently in all rooms. Objects inside rooms can react to this passage of time through trigger systems.

For example:
- One room may progress very slowly
- Another may behave like normal time
- Another may fast-forward heavily

This creates gameplay systems where object states, events, and world conditions change depending on where the player is and how much time has passed.

---

## Main Systems

### 1. Room Management
`RoomManager.cs`

Handles:
- Defining rooms using plane transforms
- Calculating room bounds from renderer size
- Detecting whether a world position is inside a room
- Converting world positions to normalized room-relative coordinates
- Converting normalized room-relative coordinates back into world positions
- Drawing debug gizmos for room bounds in the editor

Each room contains:
- A name
- A plane transform
- A configurable vertical height
- A Y offset

This script is the base spatial system for the rest of the project.

---

### 2. Relative Room Teleportation
`RoomRelativeTeleporter.cs`

Handles:
- Detecting the player's current room
- Teleporting the player to another room
- Preserving the player's relative XZ position between rooms
- Optional ground snapping after teleport
- CharacterController-safe teleporting

This allows the player to move between equivalent positions in different rooms while maintaining layout consistency.

---

### 3. Room Year Simulation
`RoomYearDirector.cs`

Handles:
- Storing a year value for each room
- Updating room years every frame
- Applying different time speeds depending on the player's current room
- Supporting designer-editable time presets
- Allowing RoomN to be temporarily paused for a configurable number of seconds

### Room indices
- `0 = RoomS`
- `1 = RoomN`
- `2 = RoomF`

### Default starting years
- RoomS = `100`
- RoomN = `2026`
- RoomF = `3000`

### Time behavior
The time speed of each room changes based on which room the player is currently standing in.

Example behavior from the script:
- Baseline:
  - RoomS = slow
  - RoomN = normal
  - RoomF = fast
- If player enters RoomS:
  - RoomN becomes fast
  - RoomF becomes super fast
- If player enters RoomF:
  - RoomN becomes slow
  - RoomS becomes super slow

This system is the core time simulation logic of the project.

---

### 4. Year-Based Object Triggers
`RoomYearTriggers.cs`

Handles:
- Detecting which room an object is currently in
- Tracking the room year for that room
- Accumulating how many years an object has experienced over time
- Firing UnityEvents once certain year thresholds are reached
- Falling back to the player's room if the object is not currently inside a room

Each trigger element includes:
- `yearsToPass`
- `triggerOnce`
- `onTriggered`

This allows objects to react to time progression without hardcoding behavior in scripts.

Example uses:
- Change food state after 10 years
- Trigger visual changes
- Activate story or environment events
- Cause objects to decay over time

---

### 5. Food State System
`FoodState.cs`

Handles:
- Switching an object between three visual states:
  - Raw
  - Aged
  - Spoiled
- Activating only one state object at a time
- Providing simple public methods:
  - `SetRaw()`
  - `SetAged()`
  - `SetSpoiled()`

This script is designed to be driven by `RoomYearTriggers` through UnityEvents.

Example flow:
- Object starts as `Raw`
- After enough years pass, trigger `SetAged()`
- After more years pass, trigger `SetSpoiled()`

---

## Script Relationships

### Core flow
- `RoomManager` defines and detects rooms
- `RoomYearDirector` updates the year progression for each room
- `RoomRelativeTeleporter` moves the player between rooms while preserving relative position
- `RoomYearTriggers` tracks how much time an object has experienced
- `FoodState` responds to trigger events and swaps visuals

---

## Example Gameplay Loop

1. The player starts in one room
2. Each room's year value updates according to the current time rules
3. The player teleports to another room
4. Time speeds change dynamically
5. Objects in rooms keep accumulating experienced years
6. Once thresholds are reached, UnityEvents fire
7. Food or other props visually change state over time

---

## Features

- Multi-room time simulation
- Independent room year tracking
- Relative player teleportation
- Year accumulation per object
- Event-based progression system
- Food aging/spoilage state switching
- Debug logging and editor gizmos
- Room pause support for specific scenarios

---

## How It Works

### Room bounds
Rooms are created from plane renderers.  
`RoomManager` reads the renderer bounds and expands them vertically using configurable height values.

### Time simulation
`RoomYearDirector` stores room years as `double` values and advances them every frame using `Time.deltaTime`.

### Object aging
`RoomYearTriggers` compares the current room year against the last observed year, adds positive changes to `totalYearsPassed`, and fires configured UnityEvents when thresholds are met.

### Visual state switching
`FoodState` enables only the matching visual GameObject for the current state.

---

## Example Setup in Unity

### Room setup
Create a `RoomHandler` GameObject and attach:
- `RoomManager`
- `RoomYearDirector`

Assign:
- Room planes
- Player transform
- Starting years
- Time presets

### Player setup
Attach `RoomRelativeTeleporter` to the player object with a `CharacterController`.

Assign:
- `RoomManager`

### Object setup
For a food object:
1. Attach `FoodState`
2. Assign:
   - Raw visual
   - Aged visual
   - Spoiled visual
3. Attach `RoomYearTriggers`
4. Add trigger entries for year thresholds
5. Hook trigger UnityEvents to:
   - `FoodState.SetAged()`
   - `FoodState.SetSpoiled()`

---

## Current Included Scripts

- `RoomManager.cs`
- `RoomRelativeTeleporter.cs`
- `RoomYearDirector.cs`
- `RoomYearTriggers.cs`
- `FoodState.cs`

---

## Notes

- `RoomYearTriggers` appears twice in the provided scripts, but it is the same system.
- `RoomYearTriggers` expects a GameObject named `RoomHandler` by default unless references are assigned manually.
- `RoomRelativeTeleporter` requires a `CharacterController`.
- Room detection depends on renderer bounds being present on the assigned room plane or its children.

---

## Possible Extensions

- More object state systems beyond food
- NPC aging or behavior changes based on room time
- Save/load for room years and trigger states
- UI display for current room year
- Audio/lighting changes tied to room year thresholds
- Puzzles based on moving objects through different time rooms

---

## Project Status

Prototype / systems implementation.

This project currently focuses on the underlying gameplay framework for room-based time manipulation and time-reactive objects.

---
