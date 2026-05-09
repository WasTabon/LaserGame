# LaserGame — Iteration 11: Harder Levels + Shop

## What's done

- Levels 2-5 redesigned harder (more elements, energy stars required for 2★)
- SaveSystem extended with boost counters: `hintCount`, `undoCount`, `skipCount`
- ShopPopup component (3 boost rows: Hint/Undo/Skip with cost + owned count)
- Shop button on MainMenu opens ShopPopup
- MainMenuShopExtension component wires shop button to MainMenuController object (no rewrite of original controller)

### New scripts
- **ShopPopup** — popup with title, coins display, 3 buy rows, close button. Buy decreases coins + increments boost count + saves. Insufficient coins → shake animation. `Refresh()` called on enable.
- **MainMenuShopExtension** — small component, sits next to MainMenuController. Wires shop button → opens shop popup.

### Updated scripts
- **SaveSystem** — added `hintCount`, `undoCount`, `skipCount` int fields to `GameData`. Old saves load fine (default 0 for new fields).

### Updated levels (in LevelsDatabase via setup)

- **Level 2 — Wall + Star**: 5×5, 2 mirrors initially `\` + 1 wall + 1 battery + 1 energy star at (3,4). Solution: rotate both mirrors to `/`, beam visits star then battery. 2 moves = 3★, star required for 2★.
- **Level 3 — Two Stars**: 5×5, 1 mirror initially `\` + 1 battery + 2 energy stars (one on initial path at (1,2), one on rotated path at (3,3)). Both stars must be collected for 2★. 1 move = 3★.
- **Level 4 — Double Battery**: 7×7, 1 mirror initially `/` + 2 batteries (one before mirror at (2,3), one below mirror at (4,1)). Rotate mirror to `\` so beam passes through bat1 then reflects down to bat2. 1 move = 3★.
- **Level 5 — Splitter Plus**: 7×7, 1 mirror + 1 splitter + 2 batteries + 2 energy stars. Rotate mirror, splitter splits beam to both batteries, both stars on path. 1 move = 3★.

Level 1 stays as easy intro (no change).

---

## Install

Unzip over project (after Iteration 10). Files:

**New:**
- `Assets/LaserGame/Scripts/ShopPopup.cs`
- `Assets/LaserGame/Scripts/MainMenuShopExtension.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration11_Setup.cs`

**Updated:**
- `Assets/LaserGame/Scripts/SaveSystem.cs`

---

## Setup

**`LaserGame → Iteration 11 → Run Both`** does:
1. **Update Levels To Harder** — overwrites Level_02..05 configs in LevelsDatabase with harder versions (Level_01 untouched)
2. **Update MainMenu Scene (Shop)** — adds Shop button + ShopPopup to MainMenu canvas, wires via MainMenuShopExtension component on MainMenuController object

**`LaserGame → Iteration 11 → Grant 500 Coins (Test)`** — adds 500 coins to save for testing the shop without playing through levels.

---

## Test

1. Run `Run Both`
2. Open MainMenu → Play → see PLAY button + new SHOP button (magenta) below
3. Tap SHOP → ShopPopup opens with coins counter at top, 3 boost rows, CLOSE button
4. If coins low: tap any BUY → buttons dim (interactable=false). To test buying:
   - Run `Grant 500 Coins (Test)` → reopen Shop → counters show affordable, buttons enabled
   - Tap HINT BUY → coins decrease by 50, hint count increments to x1, save persists
   - Tap UNDO BUY → -75, undo count x1
   - Tap SKIP BUY → -200, skip count x1
5. Close shop → PLAY → LevelSelect → tap level 2 → see harder level with 2 mirrors + wall + battery + star
6. Solve level 2 (rotate both mirrors to `/`) → win popup
7. Repeat for levels 3-5 (each is harder than I8 versions)

Boost counts persist across sessions but are not yet usable in-game — that's Iteration 12.

---

## Architecture

### SaveSystem additions
3 new int fields: `hintCount`, `undoCount`, `skipCount`. JsonUtility serializes them. Old saves missing these fields default to 0 (no migration needed).

### Shop UI structure
```
ShopPopup
├── Backdrop (button - close on tap outside)
└── Content (panel)
    ├── Title "SHOP"
    ├── CoinsBlock (icon + counter)
    ├── HintRow (icon "?" cyan + title + desc + count + buy)
    ├── UndoRow (icon "U" magenta + title + desc + count + buy)
    ├── SkipRow (icon ">" green + title + desc + count + buy)
    └── CloseButton
```

Each shop row: icon panel (left), title/desc/count column (middle), buy button with cost (right).

Buy buttons are dimmed (alpha 0.4 + interactable=false) when player can't afford. Clicked when affordable: deduct coins, increment count, save, refresh, sound, haptic.

Insufficient (theoretical, since button is non-interactable when not affordable, but failsafe): shake content, heavy haptic.

### Wiring without modifying MainMenuController
Used separate `MainMenuShopExtension` component added to MainMenuController GameObject. No risk of breaking existing controller. Just wires button→popup.

---

## Boost costs

- HINT: 50 coins
- UNDO: 75 coins
- SKIP: 200 coins (premium — just skip-to-win)

Editable in inspector on ShopPopup component.

---

## Notes

- Boost counts are stored but not consumed yet (no in-game UI). Iteration 12 adds the boost activation in Game scene.
- Level 1 unchanged (intro level should stay easy).
- The MainMenu scene's PLAY button position may overlap with new SHOP button if the original layout was tight. SHOP placed at anchored Y=320 from bottom, large 420×130. Adjust manually in inspector if conflicts.
- Old saves continue to work — 3 new boost fields default to 0.
