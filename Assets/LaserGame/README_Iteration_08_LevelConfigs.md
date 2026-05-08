# LaserGame — Iteration 08: Level Configs + 5 Unique Levels + Cyclic Mapping

## What's done

5 unique level configs as ScriptableObjects + LevelsDatabase with cyclic mapping for levels 6-30. GameController loads level by `GameSession.CurrentLevel` from database (with `testLevel` as fallback).

### New scripts
- **LevelConfigSO** — ScriptableObject wrapping `LevelDefinition`. CreateAssetMenu "LaserGame/Level Config".
- **LevelsDatabaseSO** — ScriptableObject with `uniqueConfigs[5]` + `levelToConfigMapping[30]` + `totalLevels`. Method `GetConfigForLevel(int)` returns config by level index.
- **LevelLoader** — static helper. `LoadForLevel(index, database, fallback)` — returns LevelDefinition. Falls back to `testLevel` if database null.

### Updated scripts
- **GameController** — added `levelsDatabase` field. `Start` now uses `LevelLoader` to fetch definition by `GameSession.CurrentLevel`. `_activeLevel` field caches the current loaded definition. Reset / Replay / CalculateStars use `_activeLevel` instead of `testLevel`. `testLevel` retained as fallback for debug (Iteration 4-7 test commands still work).

### 5 unique levels

1. **Level_01_Mirror** — 5×5, 1 mirror (`\` initial), 1 battery. Player rotates to `/`. maxMoves=1.
2. **Level_02_WallDetour** — 5×5, 2 mirrors + 1 wall blocking direct path + 1 battery. Player rotates both. maxMoves=2.
3. **Level_03_EnergyStar** — 5×5, 1 mirror + 1 battery + 1 energy star on initial path. Player rotates mirror, star auto-collected first frame. maxMoves=1.
4. **Level_04_MultiBattery** — 7×7, 1 mirror + 2 batteries on opposite paths from mirror. Player rotates so beam passes through both. maxMoves=1.
5. **Level_05_Splitter** — 5×5, 1 mirror + 1 splitter + 2 batteries. Player rotates mirror, splitter splits beam, both batteries charge. maxMoves=1.

### Cyclic mapping
Level → config index = `(level - 1) % 5`:
- Levels 1,6,11,16,21,26 → config 0
- Levels 2,7,12,17,22,27 → config 1
- Levels 3,8,13,18,23,28 → config 2
- Levels 4,9,14,19,24,29 → config 3
- Levels 5,10,15,20,25,30 → config 4

Stored in `LevelsDatabase.levelToConfigMapping[30]`. User can edit in inspector to change mapping.

---

## Install

Unzip over project (after Iteration 7).

**New files:**
- `Assets/LaserGame/Scripts/LevelConfigSO.cs`
- `Assets/LaserGame/Scripts/LevelsDatabaseSO.cs`
- `Assets/LaserGame/Scripts/LevelLoader.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration08_Setup.cs`

**Updated:**
- `Assets/LaserGame/Scripts/GameController.cs`

---

## Setup

### Main command
**`LaserGame → Iteration 08 → Run Both (Create + Assign)`** — runs both:

1. Creates folder `Assets/LaserGame/Levels/`
2. Creates 5 LevelConfigSO assets with the unique level definitions
3. Creates LevelsDatabase asset with 5 configs + cyclic mapping 1-30
4. Opens Game scene and assigns LevelsDatabase to GameController

### Separate commands
- `Create Levels Database + 5 Configs` — only creates assets
- `Assign Database To Game Scene` — only assigns existing database to scene

---

## Test

1. Run `Run Both`
2. Open MainMenu → Play → Level Select
3. Tap level 1 → Game → see Level_01_Mirror config (5×5, 1 mirror, 1 battery). Rotate mirror → win.
4. After win, tap NEXT → loads level 2 (Wall Detour config). And so on.
5. Tap level 6 from select → loads Level_01_Mirror (cyclic).
6. Test command `Trigger Win` from I5 still works for debug.
7. I4-I7 test commands still set `testLevel` — but `levelsDatabase` takes priority. To test via testLevel, temporarily clear `GameController.levelsDatabase` field in inspector.

---

## Architecture

### Priority: database over testLevel
GameController.Start:
```csharp
var def = LevelLoader.LoadForLevel(GameSession.CurrentLevel, levelsDatabase, testLevel);
_activeLevel = def;
ApplyLevelDefinition(def);
```
- If `levelsDatabase` set + has config for current level → use database config
- Otherwise → `testLevel` fallback (debug commands still work)

### `_activeLevel` field
Cached current LevelDefinition. Used by Reset, Replay, CalculateStars instead of `testLevel`. Ensures consistency with what was actually applied.

---

## Changes from Iteration 7

- New: LevelConfigSO, LevelsDatabaseSO, LevelLoader, Iteration08_Setup
- GameController: + `levelsDatabase` field, + `_activeLevel` field. Start uses LevelLoader. Reset/Replay/CalculateStars use `_activeLevel`. testLevel kept as fallback.
- Game scene: GameController.levelsDatabase reference set to created database asset.

---

## Notes

- Database mapping editable per-level in inspector. Default cyclic, you can shuffle to your taste.
- 5 level designs reachable through `LevelLoader.LoadForLevel`. To replay specific config in editor: tap target level number in LevelSelect.
- HandleNext reloads scene → Start fetches new level config from database. Scene state fully fresh.
- testLevel still serializes; useful for I4-I7 test commands. Clear `levelsDatabase` field in inspector to disable database and use testLevel only.
