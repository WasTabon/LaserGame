# LaserGame — Iteration 03: Game Scene Base (Grid + Laser + Ray)

## Что сделано

Базовая Game сцена с UI-сеткой, эмиттером лазера и рендерингом луча. Геймплей пока минимальный (нет зеркал, батарей, win condition) — это I4-I5. Цель этой итерации — фундамент и красивый рендеринг луча.

### Новые скрипты
- **GridSystem** — построение и адресация UI-сетки. `Build(cols, rows)` создаёт `cellsHolder` с квадратными ячейками. `GetCellLocalPos(Vector2Int)` возвращает локальную позицию центра ячейки. `IsInBounds(cell)` проверка границ. `CellSize` — размер ячейки (вычисляется из размера field и spacing).
- **LaserEmitter** — компонент эмиттера. Хранит `cell` и `direction`. `PlaceOnGrid(grid)` ставит эмиттер в нужную ячейку и масштабирует под cellSize. `GetExitLocalPos(grid)` — точка откуда выходит луч (центр ячейки + смещение в направлении).
- **RaySegment** + **RayCalculator** — статический калькулятор пути луча. В I3 возвращает один сегмент: от точки выхода эмиттера до края поля. Спроектирован с расчётом на отражения в I4.
- **RayRenderer** — pool сегментов, два слоя (Core + Glow), pulse через CanvasGroup на parent. `Render(segments, cellSize)` обновляет визуализацию. `RevealAnimation()` — стрейч появления. Толщина beam пропорциональна cellSize (адаптивно к гриду).
- **GameController** — главный контроллер сцены: HUD, BuildLevel, RecalculateRay, Reset, Back, AnimateIn.

### Game Scene композиция
```
GameCanvas
├── Background (#0F0F1E)
├── TopHud (200px, sticky top)
│   ├── BackButton ("<", magenta)
│   ├── LevelText ("LEVEL N", cyan)
│   └── CoinsHud (right, panel + yellow icon + count)
├── SubHud (80px, под TopHud)
│   └── MovesText ("MOVES: 0", white)
├── Field (1000×1000, центр, anchoredY=-50)
│   ├── FieldBackground (#0A0A17, темнее основного фона)
│   ├── CellsHolder (внутрь которого спавнятся cells через GridSystem.Build)
│   ├── SegmentsHolder (для RayRenderer, sibling 2 — луч позади elements/emitter)
│   ├── ElementsHolder (для I4+: зеркала, батареи)
│   └── EmitterHolder
│       └── LaserEmitter (Glow + Body + Arrow внутри как children)
├── ResetButton (380×130, центр снизу, magenta pill)
└── GameController_Host
```

### Дефолтный уровень (I3)
- 5×5 grid
- Эмиттер в `(0, 2)`, направление `(1, 0)` — слева в центре, луч идёт вправо
- Без других элементов — луч просто долетает до правого края поля

---

## Установка

Распаковать поверх существующего проекта (после Iteration 2). Все файлы новые — никаких файлов из I1/I2 не заменяется.

Новые файлы:
- `Assets/LaserGame/Scripts/GridSystem.cs`
- `Assets/LaserGame/Scripts/LaserEmitter.cs`
- `Assets/LaserGame/Scripts/RayCalculator.cs`
- `Assets/LaserGame/Scripts/RayRenderer.cs`
- `Assets/LaserGame/Scripts/GameController.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration03_Setup.cs`

Файл `GameSceneBackBridge.cs` остаётся в проекте (не используется на Game сцене после I3, но не вредит).

---

## Настройка через editor скрипт

### Главная команда
**`LaserGame → Iteration 03 → Setup Game Scene`**

Эта команда:
1. Открывает `Assets/LaserGame/Scenes/Game.unity` (созданная в I1, обновлённая в I2)
2. **Удаляет старый placeholder из I1**: PlaceholderText, BackButton (старая standalone), BackBridge с компонентом GameSceneBackBridge
3. Сохраняет: Background, Camera, EventSystem, AudioManager, SceneTransitionManager (DontDestroyOnLoad менеджеры)
4. Создаёт TopHud, SubHud, Field (с GridSystem, LaserEmitter, RayRenderer), ResetButton, GameController_Host
5. Выполняет `grid.Build(5,5)` и `emitter.PlaceOnGrid(grid)` в editor mode — сцена сразу выглядит готовой в Scene view

### Тестовые команды
- `Test 5x5 Grid` — 5×5, эмиттер в (0,2), вправо (это дефолт)
- `Test 7x7 Grid` — 7×7, эмиттер в (0,3), вправо
- `Test 10x10 Grid` — 10×10, эмиттер в (0,5), вправо
- `Toggle Laser Direction` — циклирует направление **right → up → left → down → right**, и автоматически ставит эмиттер в подходящую стартовую ячейку (на левой/нижней/правой/верхней грани соответственно)

Все тестовые команды:
1. Открывают Game scene
2. Меняют поля `defaultRows / defaultCols / defaultEmitterCell / defaultEmitterDir` на GameController
3. Применяют изменения в editor mode (вызывают `grid.Build()` и `emitter.PlaceOnGrid()`)
4. Сохраняют сцену

---

## Как тестировать

1. Запустить `LaserGame → Iteration 03 → Setup Game Scene`
2. Открыть Game.unity → видна сцена с 5×5 гридом и эмиттером слева в центре
3. Запустить Play (либо открыть MainMenu и пройти Play → Level → tap level 1)
4. Проверить:
   - HUD появляется сверху со slide+fade
   - Field появляется через fade
   - Эмиттер делает scale-bounce (0.3 → 1.0 OutBack)
   - Луч появляется (RevealAnimation: stretch по Y)
   - Луч пульсирует (alpha 0.85 ↔ 1.0, period 1.4s)
   - LEVEL текст показывает номер выбранного уровня (или 1 при прямом запуске Game)
   - MOVES: 0
   - Тап Reset → scale punch + medium haptic + perevyzov RecalculateRay
   - Тап Back → fade → LevelSelect (если запущено через LevelSelect) или текущая Game без перехода (если открыто напрямую — нужно открыть MainMenu для нормальной работы Back)
5. Проверить тестовые команды:
   - `Test 7x7 Grid` → перезапуск Play → видна 7×7 сетка с пропорционально меньшими ячейками, луч тоньше (пропорционально cellSize)
   - `Test 10x10 Grid` → ещё мельче, всё пропорционально
   - `Toggle Laser Direction` (несколько раз) → эмиттер прыгает на разные стороны поля, стрелка ▶ поворачивается, луч идёт в новом направлении

---

## Ожидаемый результат

Полированная Game сцена с:
- HUD сверху и Reset снизу
- Игровым полем 5×5 ячеек
- Эмиттером с glow-halo, телом, направленной стрелкой
- Лучом с двумя слоями (core + glow), толщиной пропорциональной размеру ячейки, пульсацией
- Чистой архитектурой grid/emitter/calculator/renderer готовой к расширению (зеркала в I4, батареи в I5)

Тестовые команды позволяют менять параметры дефолтного уровня без правок кода.

---

## Изменения с Iteration 2

- Game.unity: удалены `PlaceholderText`, `BackButton` (старая), `BackBridge` (с компонентом GameSceneBackBridge)
- Game.unity: создана новая иерархия с TopHud / SubHud / Field / ResetButton / GameController_Host
- В Game.unity появились новые компоненты: GridSystem, LaserEmitter, RayRenderer, GameController
- `GameSceneBackBridge.cs` остаётся как файл, но больше не используется — Back на Game сцене теперь обрабатывает GameController

---

## Архитектурные решения

### Почему RayCalculator возвращает `List<RaySegment>` а не `List<Vector2Int>` cells?
Потому что в I4 при добавлении зеркал каждое отражение → отдельный сегмент с поворотом. Сейчас 1 сегмент, в I4 будет N сегментов, RayRenderer для них уже готов через pool.

### Почему cellSize пропорционально определяет beam thickness?
Игрок видит то же визуальное соотношение beam/cell на любом размере грида. Зафиксированная толщина (например 14px) на 10×10 грида выглядела бы громоздко.

### Почему Glow и Body — это разные children внутри LaserEmitter (не Image на самом emitter)?
В Unity UI parent's Image рендерится ПЕРЕД children. Если бы Glow был child of Body-Image, он бы рендерился поверх Body. Чтобы glow был halo (за телом), LaserEmitter — пустой контейнер, а Glow / Body / Arrow — независимые children с правильным sibling order.

---

## Известные особенности

- Дефолтный grid 5×5 с одним эмиттером слева. Без зеркал, батарей, элементов — это I4-I5.
- Reset button работает (звук + scale + сброс moves), но фактически нечего сбрасывать (нет интерактивных элементов).
- Counter MOVES: 0 — заготовка под I4 (увеличивается при повороте зеркала).
- Если запустить Game scene напрямую (не через LevelSelect), `GameSession.CurrentLevel = 1` (default), Back вернёт на LevelSelect.
- Через editor скрипт можно крутить параметры (5x5/7x7/10x10/направления) — runtime каждый раз пересоздаёт grid из defaults.
