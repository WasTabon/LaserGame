# LaserGame — Iteration 09: Pause + Reset Wave + UI Polish

## What's done

- Pause button in TopHud (right side, "II" cyan)
- PausePopup with Resume / Restart / Settings / Home
- In-game SettingsPopup (same toggles as MainMenu)
- Reset button now triggers wave-animated mirror rotation (delay by distance from field center)
- Moves counter punch-scale animation on increment
- Time.timeScale handled correctly for pause (set to 0 on pause, restored on resume / restart / home / next / menu)

### New scripts
- **PausePopup** — extends PopupBase. 4 buttons: Resume, Restart, Settings, Home. Action callbacks (OnResume, OnRestart, OnSettings, OnHome). Resume / Restart / Home close popup before invoking. Settings keeps popup open (settings opens on top).

### Updated scripts
- **MirrorElement** — added `AnimateResetTo(targetStep, delay)`. Delays per-mirror rotation (used by reset wave). Punch scale on each. Visual rotation through `_visualRotationDeg += 90` for continuous spin look.
- **GameController** — added `pauseButton`, `pausePopup`, `gameSettingsPopup` fields. Pause handlers (OnPauseClicked, HandleResume, HandleRestart, HandleSettings, HandleHome). Time.timeScale = 0/1 management. Reset uses wave delay calc by distance from field center. UpdateMoves does punch scale on text. Time.timeScale=1 in OnDisable, HandleNext, HandleMenu.

---

## Install

Unzip over project (after Iteration 8).

**New:**
- `Assets/LaserGame/Scripts/PausePopup.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration09_Setup.cs`

**Updated:**
- `Assets/LaserGame/Scripts/MirrorElement.cs`
- `Assets/LaserGame/Scripts/GameController.cs`

---

## Setup

**`LaserGame → Iteration 09 → Update Game Scene`**:
1. Adds Pause button to TopHud (right side, before existing CoinsHud — coins shifted left)
2. Builds PausePopup (initially inactive, sortedlast in canvas)
3. Builds GameSettingsPopup (initially inactive, sortedlast)
4. Wires references on GameController
5. Reorders siblings so PausePopup, LevelCompletePopup, GameSettingsPopup are at top of hierarchy

---

## Test

1. Run setup
2. Open Game scene → Play (or via MainMenu → LevelSelect → tap level 1)
3. Test pause:
   - Tap "II" (right side of HUD) → Time.timeScale = 0, popup opens with scale-bounce
   - Tap Resume → popup closes, game resumes
   - Tap Restart → popup closes, level rebuilds (mirrors reset, batteries discharge, energy stars uncollect, moves=0)
   - Tap Settings → settings popup opens on top of pause popup. Toggle sound/music/haptics, close → return to pause popup
   - Tap Home → fade transition to LevelSelect
4. Test reset wave:
   - Use a level with multiple mirrors (e.g., level 2 — Wall Detour)
   - Rotate both mirrors → tap Reset
   - Mirrors animate rotation back to initial state with wave delay (closer to field center first, outer last)
5. Test moves animation:
   - Rotate any mirror → MOVES counter increments with punch-scale on text

---

## Architecture

### Time.timeScale management
- Pause click → `Time.timeScale = 0`
- All popups use `SetUpdate(true)` on tweens → continue working with timeScale=0
- Field tweens (laser pulse, mirror pulse, energy star pulse) do NOT use `SetUpdate(true)` → naturally pause
- Resume / Restart / Home / Next / Menu / OnDisable all reset `Time.timeScale = 1f` to ensure consistency

### Reset wave
```
maxDist = max anchored magnitude over all mirrors
for each mirror i: delay = (anchoredMag(i) / maxDist) * 0.25
mirror.AnimateResetTo(initialStep, delay)
```
Wave: center mirrors animate first, outer mirrors animate last, ~0.25s spread.

### In-game vs MainMenu Settings
Both scenes have own SettingsPopup. Same component class (SettingsPopup.cs), same logic. Each scene-instance reads/writes `SaveSystem.Data` which is shared.

---

## Changes from I8

- New PausePopup component
- Pause button on TopHud
- Game scene also has SettingsPopup
- MirrorElement.AnimateResetTo for wave reset
- GameController pause handling, moves anim, reset wave
- Time.timeScale lifecycle properly managed

---

## Notes

- Back button (top-left) still exists, takes you directly to LevelSelect (existing behavior). Pause button is alternative path with extras.
- Pause popup buttons stack vertically in 720x880 panel: Resume (large primary cyan) → Restart → Settings → Home (last 3 are panel-color secondary).
- Settings popup opened from pause popup uses SetAsLastSibling on open, ensuring it overlays pause popup.
- Reset wave uses anchored position magnitude (distance from field center, since field is centered at 0,0 local). For asymmetric grids (eg., 7x3), wave still works directionally.
