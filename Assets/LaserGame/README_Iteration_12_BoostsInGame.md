# LaserGame — Iteration 12: Boosts In Game

## What's done

3 boost buttons in Game scene HUD with count badges. Boost activation logic for Hint, Undo, Skip.

### Boosts

- **HINT** (cyan "?") — punches scale on first mirror in level + spawns 3 expanding cyan rings (staggered) at mirror position. Visual cue to consider this mirror.
- **UNDO** (magenta "U") — pops last move from history, animates that mirror back to previous rotation step (no event fired so undo doesn't push to history again), decrements moves counter by 1, recalculates ray. Disabled if move history empty.
- **SKIP** (green ">") — sets `_skipUsed=true`, triggers normal win sequence. `CalculateStars` returns 1 when skip used, so popup shows 1★. Save: stars=1, unlock next, +15 win bonus.

### Updated scripts
- **GameController** — added boost UI fields (3 buttons + 3 count texts + bar rect/group + hint highlight template). Move history Stack (`MoveRecord{mirrorIndex, prevRotationStep}`). `_skipUsed` flag. Methods: `OnHintBoostClicked`, `OnUndoBoostClicked`, `OnSkipBoostClicked`, `RefreshBoostUI`, `SetBoostButtonInteractable`, `ShakeBoostButton`, `SpawnHintRing`. Move history pushed in `HandleMirrorRotated`. Cleared on Reset/Replay/Restart. `CalculateStars` checks `_skipUsed` first.

### New file
- **Editor/Iteration12_Setup.cs** — builds BoostBar in Game scene with 3 buttons, count badges, wires references to GameController. Includes `Grant 5 Of Each Boost (Test)` for testing without buying.

---

## Install

Unzip over project (after Iteration 11). Files:

**New:**
- `Assets/LaserGame/Scripts/Editor/Iteration12_Setup.cs`

**Updated:**
- `Assets/LaserGame/Scripts/GameController.cs`

---

## Setup

1. **`LaserGame → Iteration 12 → Update Game Scene`** — adds BoostBar above Reset button (3 buttons in row, 110×110 each, with letter icon + yellow count badge top-right).
2. **`LaserGame → Iteration 12 → Grant 5 Of Each Boost (Test)`** — gives 5 of each boost type for testing.

---

## Test

1. Run setup + grant boosts
2. Play any level (level 2-5 recommended for visible undo effect)
3. **Hint test**: tap Hint button (cyan "?") → first mirror does scale-punch, 3 cyan rings expand outward + fade. Count goes from x5 → x4. If no boosts: button dimmed (alpha 0.4), tap shakes.
4. **Undo test**: rotate a mirror (+1 move). Tap Undo (magenta "U") → mirror animates rotation back, moves counter ticks down to previous value, ray recalculates. If history empty: undo button dimmed.
5. **Skip test**: tap Skip (green ">") → win flash + confetti + popup with 1★. Tap NEXT → next level loads. Skip count decremented.
6. Reset / Replay / Pause→Restart all clear move history (undo can't undo across resets).
7. Boost counts persist between sessions (in `SaveSystem.Data`).

---

## Architecture

### Move history
```csharp
struct MoveRecord {
    public int mirrorIndex;
    public int prevRotationStep;
}
Stack<MoveRecord> _moveHistory;
```

In `HandleMirrorRotated(m)`: compute prev step as `(m.rotationStep + 1) % 2` (since mirror toggles), push record. Clear on reset/replay/restart.

In `OnUndoBoostClicked`: pop record, call `MirrorElement.AnimateResetTo(prev, 0)` which animates rotation but does NOT fire OnRotated event (avoids history loop). Manual `RecalculateRay()` and `UpdateMoves(_moves - 1)`.

### Hint visual
3 cyan rings at mirror position. Each: scale 0.5→2.4 + alpha 0.55→0 over 0.7s with 0.18s stagger. Spawned in `coinFlyHost` (over field + HUD). `SetUpdate(true)` so works during pause too (though hint not callable during pause).

### Skip
`_skipUsed = true` → `CalculateStars()` short-circuits to return 1. Then normal `WinSequenceRoutine` runs — shake, confetti, flash, popup with 1★.

### Button enable state
`RefreshBoostUI()` sets `b.interactable` based on count > 0 (and undo additionally checks `_moveHistory.Count > 0`). Image alpha dims to 0.4 when disabled, 1 when enabled.

Called in: Start, after HandleMirrorRotated, after each boost activation, after Reset/Replay/Restart.

---

## Buy flow

1. MainMenu → Shop → buy boosts (i11)
2. LevelSelect → tap level → Game scene
3. Game scene shows boost counts (loaded from SaveSystem)
4. Use boosts during gameplay

Boosts persist between levels and sessions.

---

## Notes

- Hint highlights only first mirror in `_activeMirrors` list. For levels with multiple mirrors, this might not be the most useful one — but without a solver, can't do better. In production, would add `hintMirrorIndex` to LevelDefinition (manually authored).
- Undo doesn't fire `MirrorElement.OnRotated` (uses `AnimateResetTo` which is silent). If you switch to a method that fires the event, undo would push to history infinitely.
- Skip grants 1★ + 15 coins (10 base + 1×5). Might be too generous if skip costs only 200 — adjust skip cost in i11 if unbalanced.
- `_moveHistory` is in-memory only. App close = history lost. Not a problem for normal flow (close mid-game anyway loses progress).
