# LaserGame — Iteration 14: Big Fields + Pan + 10 Unique Levels

## Что сделано

- Свайп-пан в любую сторону (full 2D drag) для больших полей
- Cell size фиксирован 110px (поле растёт с количеством клеток, а не сжимается)
- 10 уникальных конфигов уровней (1 простой + 9 сложных, размеры 5×5 → 15×15)
- Случайная (но детерминированная) раскидка configs 1-9 по уровням 2-30. L1 всегда простой

### Новые скрипты
- **FieldPanController** — IDragHandler. Двигает fieldRoot.anchoredPosition по дельте свайпа. Зажимает в границы (контент не уходит дальше чем cellsSize - viewport*0.85). Метод `SetContentSize(w, h)` вызывается из GameController после grid.Build.

### Обновлённые
- **GridSystem** — добавлено `useFixedCellSize` + `fixedCellSize`. В режиме fixed клетка всегда 110px, поле автоматически растёт по cols×rows. В non-fixed (старый) режиме — сжимается под fieldRect.
- **GameController** — `fieldPanController` ссылка. После каждого `ApplyLevelDefinition` вызывает `SetContentSize(cols * cellSize, rows * cellSize)` чтобы pan controller знал размер контента и центрировался.

### 10 уникальных конфигов

| Slot | Размер | Описание | maxMoves |
|------|--------|----------|----------|
| 0 | 5×5 | Easy intro (1 mirror) | 1 |
| 1 | 7×7 | Zigzag small (3 mirrors, 1 bat, 2 stars) | 3 |
| 2 | 8×8 | Wall maze (3 mirrors, 2 walls, 1 bat, 2 stars) | 3 |
| 3 | 9×9 | Square loop (4 mirrors, 1 wall, 2 bats, 2 stars) | 4 |
| 4 | 9×9 | Splitter triple (1 splitter, 2 mirrors, 3 bats, 2 stars) | 2 |
| 5 | 10×10 | Long zigzag (5 mirrors, 2 bats, 3 stars) | 5 |
| 6 | 11×11 | Double splitter (2 splitters, 3 mirrors, 4 bats) | 3 |
| 7 | 12×12 | W-shape (6 mirrors, 2 walls, 2 bats, 3 stars) | 6 |
| 8 | 13×13 | Big maze (6 mirrors, 3 walls, 3 bats, 3 stars) | 6 |
| 9 | 15×15 | Monster final (7 mirrors, splitter, 4 walls, 4 bats, 3 stars) | 7 |

L1 всегда = config 0. Уровни 2-30 — случайная раскидка configs 1-9 (Fisher-Yates с seed 12345). Каждый config используется ~3 раза.

---

## Установка

Распаковать поверх (после Iteration 12 или 13). Файлы:

**Новые:**
- `Assets/LaserGame/Scripts/FieldPanController.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration14_Setup.cs`

**Обновлённые:**
- `Assets/LaserGame/Scripts/GridSystem.cs`
- `Assets/LaserGame/Scripts/GameController.cs`

---

## Setup

**`LaserGame → Iteration 14 → Run All (Configs + Pan + Shuffle)`** делает:
1. Создаёт 10 LevelConfig assets (`Level_01_v14` ... `Level_10_v14`)
2. Расширяет LevelsDatabase до 10 unique configs
3. Добавляет FieldPanController на fieldRoot, ставит useFixedCellSize=true
4. Перемешивает levelToConfigMapping[30] с фиксированным seed 12345

Отдельные команды:
- `Create Extended Configs (10 unique)` — только конфиги
- `Update Game Scene (Pan)` — только pan + cell size
- `Shuffle Level Mapping (seed 12345)` — только перемешать маппинг

---

## Test

1. Run All
2. Open MainMenu → Play → LevelSelect
3. L1 простой — поле 5×5 умещается, pan не нужен
4. L2-30 — поля разных размеров от 7×7 до 15×15
5. На больших уровнях:
   - Свайп пальцем в любую сторону → поле двигается
   - Тап на зеркале — крутит (если палец не двигался > 8px = drag threshold)
   - Reset/Pause кнопки на HUD остаются на месте
6. После пройденного уровня → Next → переход на след. уровень с другой случайной конфигурацией

---

## Архитектура

### Pan
`FieldPanController` живёт на fieldRoot. Имеет Image (transparent) для raycast. IDragHandler обрабатывает дельту → шифт fieldRoot.anchoredPosition. Зажимает в границы.

Bounds calc:
```
maxOffset.x = max(0, (contentSize.x - viewportSize.x * 0.85) / 2)
```
Контент может уйти на ~7.5% за viewport (overscroll feel).

Конфликт tap vs drag — Unity EventSystem решает по `pixelDragThreshold` (default 5px). Можно поднять до 8 для более лояльных тапов.

### Cell size
`GridSystem.useFixedCellSize = true` → cell всегда 110px вне зависимости от размера grid. fieldRect перестаёт ограничивать. Контент рендерится в реальный размер cols×110 × rows×110.

Возможный визуальный артефакт: для 15×15 (1650×1650) контент выходит за canvas, виден за HUD при pan. Не критично.

### Shuffle determinism
`UnityEngine.Random.InitState(12345)` перед Fisher-Yates. Сохраняем prev state, восстанавливаем после. Маппинг одинаковый при каждом запуске Setup. Игрок получает те же конфиги на тех же уровнях.

Если нужен новый порядок — поменять `ShuffleSeed` в Iteration14_Setup.

---

## Notes

- Конфиги задизайнены по принципу zigzag-path с альтернированием mirror states. Не все 100% протрассированы вручную, риск что какой-то уровень неоптимально сбалансирован. Если найдёшь не проходимый — скажи номер, переделаю.
- maxMoves в каждом конфиге выставлен с запасом (на 1-2 поворота больше минимума), чтобы 3★ был достижим при ровной игре.
- Boost'ы (Hint/Undo/Skip из i11/i12) работают как обычно — особенно полезны на больших уровнях где зеркал много.
- `_activeMirrors[0]` для подсказки — это первое зеркало в LevelDefinition. На больших уровнях стоит указывать его как самое полезное в конфиге.
- Pan не работает в момент анимации win sequence (popup блокирует raycast).
- Если не нравится auto-shuffle — можно вручную править `levelToConfigMapping[i]` в инспекторе LevelsDatabase.
