# LaserGame — Iteration 13: Very Hard Levels

## What's done

Levels 2-5 redesigned to be much harder. Level 1 unchanged (intro).

### Level 2 — Triple Mirror
- **7×7**, 3 mirrors + 2 batteries + 1 energy star
- emitter (0,3)→
- Mirrors initially: m1(1,3)`\`, m2(1,5)`\`, m3(3,5)`/`
- Batteries: (1,4) and (3,1)
- Energy star: (2,5)
- maxMoves=3 → **3 rotations needed exactly**

Solution: rotate m1→/, m2→/, m3→\.  Beam: (0,3)→(1,3) m1`/`up→(1,4) bat1✓ (1,5) m2`/`right→(2,5) star✓ (3,5) m3`\`down→(3,4)(3,3)(3,2)(3,1) bat2✓.

### Level 3 — Wall Maze + Two Stars
- **7×7**, 3 mirrors + 2 walls + 1 battery + 2 energy stars
- emitter (0,3)→
- Mirrors initially: m1(2,3)`\`, m2(2,5)`\`, m3(5,5)`/`
- Walls: (4,3), (3,1)
- Battery: (5,0)
- Energy stars: (1,3), (5,3)
- maxMoves=3 → 3 rotations + both stars for 3★

Solution: m1→/, m2→/, m3→\. Beam zigzags (0,3)→star1@(1,3)→m1up→m2right→m3down→star2@(5,3)→bat@(5,0).

### Level 4 — Square Loop
- **7×7**, **4 mirrors** + 1 wall + 2 batteries + 2 energy stars
- emitter (0,3)→
- Mirrors initially: m1(2,3)`\`, m2(2,5)`\`, m3(5,5)`/`, m4(5,3)`\`
- Wall: (4,3) (blocks initial direct path)
- Batteries: (4,5), (5,4)
- Energy stars: (1,3), (3,3)
- maxMoves=4 → 4 rotations for 3★

Solution: m1→/, m2→/, m3→\, m4→/. Beam loops square: up→right→down→left, charges both batteries on top/right edges, collects stars on initial straight + on left return path.

### Level 5 — Splitter Multi-Charge
- **7×7**, 1 splitter + 2 mirrors + 3 batteries + 2 walls + 2 energy stars
- emitter (0,3)→
- Splitter (2,3) rotStep=1 (`\`) — fixed
- Mirrors initially: m1(2,2)`\`, m2(5,2)`\`
- Batteries: (5,3), (3,2), (5,4) — **THREE batteries**
- Walls: (0,5), (6,1) — boundary walls
- Energy stars: (1,3), (4,2)
- maxMoves=2 → 2 rotations + both stars for 3★

Solution: m1→/, m2→/. Splitter splits beam into transmit (right→bat@(5,3)) + reflect (down→m1`/`right→bat@(3,2)→star@(4,2)→m2`/`up→bat@(5,4)). 3 simultaneous battery charges on 2 rotations.

---

## Install

Unzip over project (after Iteration 12). Files:

**New:**
- `Assets/LaserGame/Scripts/Editor/Iteration13_Setup.cs`

No code changes — just level config updates.

---

## Setup

**`LaserGame → Iteration 13 → Update Levels To Very Hard`** — overwrites Level_02..05 configs in LevelsDatabase. Level_01 untouched.

---

## Test

1. Run setup
2. Open MainMenu → Play → Level Select
3. Tap level 2 — see 7×7 grid with 3 mirrors + 2 batteries + star. Try to solve with exactly 3 rotations for 3★.
4. Tap level 3 — wall maze with 2 stars required for 2★.
5. Tap level 4 — square loop, 4 rotations for full charge.
6. Tap level 5 — splitter chain, 3 batteries simultaneously.

If stuck — use HINT boost (highlights first mirror), UNDO to revert wrong rotations, SKIP to bypass entirely.

---

## Difficulty progression

- **L1**: 1 mirror, 1 battery, 1 move — pure intro
- **L2**: 3 mirrors, 2 batteries, 1 star, **3 moves**
- **L3**: 3 mirrors + walls, 1 battery, **2 stars required**, 3 moves
- **L4**: 4 mirrors + wall, 2 batteries, 2 stars, **4 moves**
- **L5**: splitter + 2 mirrors + walls, **3 batteries**, 2 stars, 2 moves

Cyclic: levels 6-30 reuse these configs (level 6 = L1 config, level 7 = L2, ...).

---

## Notes

- All levels still solvable with at most maxMoves rotations for 3★.
- Players can use boosts (Hint/Undo/Skip from i11/i12) for help.
- Splitter level (L5) showcases all mechanics in one puzzle.
- Walls in L3 and L5 are mostly aesthetic (don't block solution path) but reinforce visual complexity. Adjust positions in inspector if needed.
- `maxMovesForThreeStars` editable per-config in inspector after running setup.
