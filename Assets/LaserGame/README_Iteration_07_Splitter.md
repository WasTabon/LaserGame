# LaserGame — Iteration 07: Splitter

## Что сделано

Splitter (разделитель луча) — элемент, который пропускает прямой луч и одновременно создаёт перпендикулярный отражённый. RayCalculator переписан с linear traversal на stack-based branching.

### Новые скрипты
- **SplitterElement** — пассивный (НЕ интерактивный в I7) разделитель. Хранит `cell` и `rotationStep` (фиксированный из конфига). Визуал: glow halo (cyan dim) → body (panel color) → 2 cyan diagonals (lineA full alpha, lineB semi-transparent) образующие X-pattern. Idle pulse glow для привлечения внимания.

### Обновлённые скрипты
- **LevelDefinition** — добавлены `splitters: List<SplitterPlacement>` и struct `SplitterPlacement { cell, rotationStep }`.
- **RayCalculator** — **переписан** с branching через `Stack<BeamState>`. Каждый луч (beam) отслеживается state-структурой `{curCell, dir, segmentStart, splittersHit}`. При встрече splitter: выдаётся текущий segment в его центр, push'ится перпендикулярный beam в стек, текущий beam продолжает в том же направлении (transmit). Защита: `MaxTotalSteps = 256` глобальный лимит, `splittersHit < 8` per beam (для loop-предотвращения).
- **GameController** — добавлены `splitterTemplate`, `_activeSplitters`, `SpawnSplitters`, `ClearAllElements` теперь чистит и splitters. `RecalculateRay` передаёт `_activeSplitters` в RayCalculator.

### Game Scene additions
- `SplitterElementTemplate` (hidden child Canvas) для Instantiate.

---

## Логика разделителя

Splitter с `rotationStep` имеет ту же геометрию отражения что и mirror:
- **rotStep = 0** (`/` диагональ):
  - Transmit: dir остаётся неизменным
  - Perpendicular: `right → up`, `up → right`, `left → down`, `down → left`
- **rotStep = 1** (`\` диагональ):
  - Transmit: dir остаётся неизменным
  - Perpendicular: `right → down`, `up → left`, `left → up`, `down → right`

Алгоритм `MirrorReflection.Reflect(dir, rotStep)` переиспользуется для perpendicular branch.

В отличие от mirror — splitter создаёт **2 луча** (transmit + perpendicular) вместо одного отражённого.

---

## Установка

Распаковать поверх существующего проекта (после Iteration 6).

**Новые файлы:**
- `Assets/LaserGame/Scripts/SplitterElement.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration07_Setup.cs`

**Обновлённые файлы:**
- `Assets/LaserGame/Scripts/LevelDefinition.cs`
- `Assets/LaserGame/Scripts/RayCalculator.cs` — **переписан** с branching
- `Assets/LaserGame/Scripts/GameController.cs` — добавлен splitter handling

---

## Настройка через editor скрипт

### Главная команда
**`LaserGame → Iteration 07 → Update Game Scene`** — создаёт SplitterElementTemplate, прокидывает ссылку.

### Тестовые уровни

- **`Test Level - Basic Splitter`** — 5×5, эмиттер (0,2)→, splitter (2,2) `/`, batteries (4,2) и (2,4). **Instant win**: луч right разделяется в (2,2) на → right (заряжает (4,2)) и → up (заряжает (2,4)).
- **`Test Level - Splitter + Mirror`** — 5×5, mirror (3,2) `\`-initially, splitter (3,3) `\`, batteries (3,4) и (1,3). Игрок поворачивает mirror в `/`: луч right → mirror reflects up → splitter в (3,3) разделяется на up (заряжает (3,4)) и left (заряжает (1,3)). 3★ за 1 ход.
- **`Test Level - Two Splitters`** — 7×7, цепочка splitter (2,3) `/` + splitter (4,3) `\`. Один initial beam right разделяется дважды → 3 batteries: (6,3), (2,6), (4,0). Демонстрация chained splitters. **Instant win**.
- **`Test Level - Splitter Full Demo`** — 7×7, all mechanics: mirror + splitter + walls + energy stars. Игрок поворачивает mirror, splitter раздваивает луч, обе батареи зарядятся, обе energy stars соберутся.

---

## Как тестировать

### Splitter visual
1. `Update Game Scene` → `Test Level - Basic Splitter` → Play
2. Видеть в (2,2): cyan halo вокруг panel-body, X-pattern из двух cyan диагоналей (одна полная, другая полупрозрачная)
3. Splitter glow пульсирует (idle)
4. Луч от эмиттера попадает в splitter → визуально расщепляется на два сегмента: один продолжает right, другой идёт up
5. Обе батареи (4,2) и (2,4) **одновременно** заряжаются → win sequence

### Branching algorithm
1. `Test Level - Two Splitters` → Play
2. Видеть **3 сегмента луча** от двух splitter'ов:
   - Главная линия right → through splitter1 → through splitter2 → out
   - Branch up из splitter1 → out
   - Branch down из splitter2 → out
3. Все 3 батареи мгновенно заряжаются
4. Win sequence — поле полностью светится

### Interaction with mirrors
1. `Test Level - Splitter + Mirror` → Play
2. Изначально луч right → mirror `\` reflects → down → out. Без splitter активации.
3. Тапнуть mirror → `/` → луч up → splitter (3,3) → разделение на up (battery) + left (battery)
4. Обе зарядились → win за 1 ход → 3★

### Full demo
1. `Test Level - Splitter Full Demo` → Play
2. Изначально: mirror `\` отражает луч down. Wall (4,3) не задействован.
3. Energy star (1,3) на пути → collected immediately + coin fly
4. Тапнуть mirror → `/` → up → splitter `/` разделяет на up (battery (2,6)) и right (через energy star (4,5) и до battery (6,5))
5. Обе батареи charged + обе energy stars collected → 3★ за 1 ход → +25 win bonus + 5+5 за stars

---

## Архитектура branching

### Stack-based traversal
```csharp
struct BeamState {
    Vector2Int curCell;
    Vector2Int dir;
    Vector2 segmentStart;
    int splittersHit;
}

Stack<BeamState> stack;
stack.Push(initialBeam);

while (stack.Count > 0 && totalSteps < 256) {
    var beam = stack.Pop();
    while (totalSteps < 256) {
        // step beam
        // if wall/bounds: terminate
        // if splitter: push perpendicular, continue with transmit
        // if mirror: change dir, continue
    }
}
```

### Защита от циклов
- `MaxTotalSteps = 256` — общий лимит шагов (предотвращает зависание UI)
- `splittersHit < 8` per beam — каждый beam может пройти max 8 splitter'ов (предотвращает рекурсивные циклы splitter↔mirrors)
- Если beam опять заходит в splitter после loop через mirrors — на 9-м проходе splitter работает как обычный pass-through (transmit без branching)

### Visited cells
Все cells через которые прошёл ANY beam собираются в `result.visitedCells`. Это используется для:
- Charge batteries (проверка cell в visited)
- Collect energy stars (проверка cell в visited)

Один HashSet — всё дерево лучей. Если battery достигается через любой branch — заряжается.

---

## Изменения с Iteration 6

- `LevelDefinition` — добавлены `splitters` + `SplitterPlacement` struct
- `RayCalculator` — переписан с linear traversal на stack-based branching
- `GameController` — добавлен splitter handling (template, list, spawn, clear, передача в RayCalculator)
- Game scene — добавлен SplitterElementTemplate

---

## Известные особенности

- **Splitter не интерактивен в I7** — поворот фиксирован из LevelDefinition. Игрок не может крутить splitter. Если в будущем нужен interactive splitter — easy добавить click handler аналогично MirrorElement.
- **Visual orientation splitter** — X-pattern одинаков для rotStep=0 и rotStep=1 (X симметричен на 90°). Игрок не отличит rotStep на глаз. В I7 это OK — splitter just split, поведение детерминированно. В будущем (если нужно) можно сделать direction indicator (маленькую стрелочку в центре).
- **Splitter в emitter cell** — теоретически возможно, но в RayCalculator emitter cell не проверяется на splitter (проверка только `next` cell). Так что splitter в emitter cell не сработает. В тестовых уровнях такое не используется.
- **Закрытые петли через mirrors + splitter** — defended limits: 256 шагов total, 8 splittersHit per beam. В extreme cases beam'ов может быть много (до сотен), но defensive limits предотвращают зависание.
- **Performance** — 256 step ceiling очень consequence-free для текущих уровней (typical 10-20 шагов). На сложных уровнях со множественными splitters может быть до 50-100 шагов. UI всё ещё responsive.
