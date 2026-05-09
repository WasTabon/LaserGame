# LaserGame — Iteration 10: Final Polish

## What's done

Final polish iteration. Tutorial hint, win confetti, canvas shake, audio cue hooks, audio music tracks for menu/game.

### New scripts
- **TutorialHint** — floating pointer (TMP "☟" yellow) with idle scale-pulse. `ShowOn(target)` positions above target world-pos. `Hide()` fades and deactivates. Persistent flag in `PlayerPrefs` ("tutorial_shown_v1") prevents re-showing after first rotation on level 1.

### Updated scripts
- **AudioManager** — added clip slots: `gameMusicClip`, `mirrorRotateClip`, `batteryChargeClip`, `energyStarClip`, `winClip`. Helper methods `PlayGameMusic`, `PlayMirrorRotate`, `PlayBatteryCharge`, `PlayEnergyStarCollect`, `PlayWin`. All `null`-safe — no clip → no-op.
- **GameController** — added `tutorialHint` field. `Start` plays game music + triggers tutorial (level 1 only, first run). `HandleMirrorRotated` plays mirror rotate sfx + hides tutorial after first tap. `UpdateBatteryStates` plays battery charge sfx. `CollectEnergyStar` plays energy star sfx. `WinSequenceRoutine` adds: win sfx + canvas shake (`fieldRoot.DOShakeAnchorPos`) + 24 confetti particles spawned at field center with random colors, directions, rotations.

---

## Install

Unzip over project (after Iteration 9).

**New:**
- `Assets/LaserGame/Scripts/TutorialHint.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration10_Setup.cs`

**Updated:**
- `Assets/LaserGame/Scripts/AudioManager.cs`
- `Assets/LaserGame/Scripts/GameController.cs`

---

## Setup

**`LaserGame → Iteration 10 → Update Game Scene`**:
1. Builds `TutorialHint` object in canvas (hidden by default)
2. Wires reference on GameController
3. Reorders siblings so TutorialHint is below popups but above field

**`LaserGame → Iteration 10 → Reset Tutorial Flag`** — clears `PlayerPrefs` flag so tutorial shows again on next level 1 visit. Useful for retesting.

---

## Test

### Tutorial hint
1. Run setup, then `Reset Tutorial Flag`
2. Open MainMenu → Play → tap level 1
3. After fade-in (0.7s delay), yellow pointer "☟" appears above first mirror with pulse animation
4. Tap mirror → hint fades, flag set
5. Replay or open level 1 again — hint does NOT show

### Win confetti + shake
1. Tap level 1, rotate mirror to win → win sequence plays
2. Field shakes (anchored pos shake 0.5s)
3. 24 colored confetti rectangles fly outward from field center with random rotation, fade out over 1-1.5s
4. Standard win flash + popup follow

### Audio cues
Drop AudioClips into AudioManager component on AudioManager root in scene:
- `gameMusicClip` — looping ambient game music (different from menu)
- `mirrorRotateClip` — short click/swoosh on mirror rotation
- `batteryChargeClip` — power-up sound on battery charge
- `energyStarClip` — chime on star collect
- `winClip` — fanfare on win

If clips not assigned, all PlaySFX calls are no-op (silent). Game runs fine without audio.

---

## Architecture

### Tutorial flag
Stored in `PlayerPrefs` key `tutorial_shown_v1`. Set after first mirror rotation. Versioned suffix `_v1` allows future tutorial revisions without conflicting with old saves.

### Confetti spawning
Spawns 24 GameObjects with Image components in `coinFlyHost` (above HUD). Each:
- Random size 14-26 px
- Random color from palette (cyan, magenta, yellow, green)
- Random angle + distance + rotation
- DOMove + DORotate + DOFade in parallel via Sequence
- OnComplete destroys particle

All tweens use `SetUpdate(true)` (timeScale-independent — works during paused win sequence).

### Canvas shake
`fieldRoot.DOShakeAnchorPos(duration=0.5, strength=25, vibrato=18, randomness=90)`. Subtle, doesn't dislodge UI.

---

## Final notes

This is the last planned iteration. Game has all 10 scoped features:
- I1: Foundation (save, audio, transitions, popups, button animator, main menu)
- I2: Level Select (30 buttons, locked/unlocked/completed states)
- I3: Game scene base (grid, laser, ray rendering)
- I4: Mirrors (rotate, reflect)
- I5: Batteries + win condition + level complete popup
- I6: Walls + energy stars + coin fly
- I7: Splitter (branching ray)
- I8: 5 unique level configs + cyclic mapping (SO-based)
- I9: Pause popup + reset wave + moves animation + in-game settings
- I10: Tutorial + confetti + canvas shake + audio cue hooks

Remaining for production: art assets (sprites for elements), audio files (music + sfx), level design tuning (more variety in 5 configs or expand to more uniques), iOS-specific haptics via Unity native plugin.
