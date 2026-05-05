# LaserGame — Iteration 04: Mirrors

## Что сделано

Зеркала с плавным поворотом и отражение луча в реальном времени. Это ядро паззл-механики.

### Новые скрипты
- **MirrorReflection** — статический helper. `Reflect(dir, rotationStep)`: rotStep=0 (`/`) — swap x/y; rotStep=1 (`\`) — swap x/y и negate.
- **LevelDefinition** + **MirrorPlacement** — `[Serializable]` структуры (НЕ ScriptableObject пока — это в Iteration 8). Inline-конфиг уровня: cols, rows, emitterCell, emitterDir, list of mirrors.
- **MirrorElement** — компонент зеркала с `cell`, `rotationStep`, `OnRotated` event. Click → toggle rotationStep между 0 и 1, визуально поворачивает diagonal-line на +90° (через `RotateMode.FastBeyond360` чтобы продолжать вращение в одну сторону, а не туда-сюда). Punch scale на body, ripple expand+fade, light haptic, sound.

### Обновлённые скрипты
- **RayCalculator** — теперь принимает `List<MirrorElement>`. Алгоритм: идём по cells, при встрече с mirror закрываем сегмент в его центре, отражаем направление, продолжаем. `MaxBounces = 64` защита от петель.
- **GameController** — удалены deprecated `defaultRows/Cols/EmitterCell/EmitterDir`, осталось только `testLevel` (LevelDefinition). `ApplyLevelDefinition(def)` делает grid.Build + place emitter + ClearMirrors + SpawnMirrors. `HandleMirrorRotated` → `moves++` + `RecalculateRay`. Reset → возврат всех зеркал в initial rotation steps.
- **Iteration03_Setup.cs** — обновлены команды `Test 5x5/7x7/10x10 Grid` и `Toggle Laser Direction` чтобы работать с новым `testLevel` API. Логика та же.

### Game Scene additions
В существующей Field появляется `MirrorElementTemplate` как hidden child Canvas (используется для Instantiate). `ElementsHolder` (был создан в I3) теперь активно используется — runtime спавнит сюда зеркала.

---

## Установка

Распаковать поверх существующего проекта (после Iteration 3). Файлы:

**Новые:**
- `Assets/LaserGame/Scripts/MirrorReflection.cs`
- `Assets/LaserGame/Scripts/LevelDefinition.cs`
- `Assets/LaserGame/Scripts/MirrorElement.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration04_Setup.cs`

**Обновлённые (заменяют I3 версии):**
- `Assets/LaserGame/Scripts/RayCalculator.cs` — расширенная сигнатура с `List<MirrorElement>`
- `Assets/LaserGame/Scripts/GameController.cs` — `testLevel` API, spawn mirrors, OnRotated handling
- `Assets/LaserGame/Scripts/Editor/Iteration03_Setup.cs` — обновлено под новый GameController API

---

## Настройка через editor скрипт

### Главная команда
**`LaserGame → Iteration 04 → Update Game Scene`**

Эта команда:
1. Открывает Game.unity (созданный в I3)
2. Создаёт `MirrorElementTemplate` (hidden child GameCanvas) с полным набором visual elements: ripple → glow → body (с Button) → diagonalLine
3. Прокидывает ссылки на GameController: `mirrorTemplate`, `elementsHolder`
4. Сохраняет

### Тестовые уровни
Запускать **после Update Game Scene**:

- **`Apply Default Test Level (5x5 Empty)`** — 5×5, эмиттер (0,2)→, без зеркал. Луч просто пролетает на правый край.
- **`Test Level - 1 Mirror Diagonal`** — 5×5, mirror (2,2) `/`. Луч идёт right, в (2,2) поворачивает up, уходит вверх.
- **`Test Level - 2 Mirrors Bounce`** — 5×5, mirrors (3,2) `/` + (3,4) `\`. Луч right → up → left.
- **`Test Level - 3 Mirrors Zigzag`** — 7×7, mirrors (3,3) `\` + (3,1) `/` + (5,1) `\`. Луч right → down → right → up.

Все тестовые команды:
1. Открывают Game scene
2. Меняют `ctrl.testLevel`
3. Применяют grid + emitter в editor mode (без spawning mirrors — runtime сделает при Play)
4. Сохраняют

Команды I3 (`Test 5x5/7x7/10x10 Grid`, `Toggle Laser Direction`) **продолжают работать**, очищают зеркала (testLevel.mirrors = empty).

---

## Логика отражения

**rotationStep = 0** означает зеркало `/` (диагональ snizu-vlevo do verkhu-vpravo):
- right `(1,0)` → up `(0,1)`
- up `(0,1)` → right `(1,0)`
- left `(-1,0)` → down `(0,-1)`
- down `(0,-1)` → left `(-1,0)`

**rotationStep = 1** означает зеркало `\` (диагональ sverkhu-vlevo do nizu-vpravo):
- right `(1,0)` → down `(0,-1)`
- down `(0,-1)` → right `(1,0)`
- left `(-1,0)` → up `(0,1)`
- up `(0,1)` → left `(-1,0)`

Каждый тап → toggle rotationStep. Визуально line поворачивается на +90° (через FastBeyond360, всегда в одну сторону).

---

## Как тестировать

1. Запустить `LaserGame → Iteration 04 → Update Game Scene`
2. Запустить любой `Test Level - ...`
3. Открыть Game.unity → Play
4. Проверить:
   - Зеркала появляются на своих cells со scale-bounce (как часть Field fade-in)
   - Луч идёт по правильной траектории, отражается на каждом зеркале
   - Тап по зеркалу:
     - Body делает scale-punch (0.12 amplitude, 0.3s)
     - Diagonal line поворачивается на +90° (OutBack, 0.2s)
     - Magenta ripple-кольцо разлетается из центра mirror (scale 0.6→2.0, fade 0.55→0)
     - Light haptic
     - Click sound
     - Луч мгновенно пересчитывается и перерисовывается
     - MOVES counter инкрементируется
   - Reset:
     - Все зеркала возвращаются к initial rotationStep (мгновенно, без анимации)
     - MOVES обнуляется
     - Луч пересчитывается и переанимируется (RevealAnimation stretch)
     - Reset button делает scale-punch
   - Несколько последовательных тапов — line продолжает крутиться в одну сторону (visualRotationDeg += 90 each tap, не зависает на shortest path)
5. Запустить `Apply Default Test Level (5x5 Empty)` → перезапуск Play → видно поле без зеркал (как в I3)
6. Запустить I3 команды (`Test 7x7 Grid` и т.д.) → тоже работают, очищают зеркала

---

## Архитектура отражения

`RayCalculator.Calculate` идёт по cells пошагово (curCell + dir). На каждом шаге:
1. Если `next` out of bounds → закрываем segment до boundary edge → break
2. Если в `next` есть mirror → закрываем segment до mirror center → отражаем dir → продолжаем с этой cell
3. Иначе → просто curCell = next, продолжаем

Сегмент закрывается ТОЛЬКО на отражении или выходе. Прямой пролёт через пустые cells = один длинный сегмент. RayRenderer pool оптимально расходует Image-ы.

---

## Изменения с Iteration 3

- `RayCalculator.cs` — новая сигнатура `Calculate(grid, emitter, mirrors)`
- `GameController.cs` — удалены `defaultRows/Cols/EmitterCell/EmitterDir`, добавлены `testLevel`, `elementsHolder`, `mirrorTemplate`, `_activeMirrors`. Новые методы `ApplyLevelDefinition`, `ClearMirrors`, `SpawnMirrors`, `HandleMirrorRotated`.
- `Iteration03_Setup.cs` — внутренние команды переключены на `testLevel` API.
- Game.unity — добавлен hidden `MirrorElementTemplate` под Canvas. ElementsHolder теперь используется runtime.

---

## Известные особенности

- Зеркала имеют **2 функциональных состояния** (`/` и `\`), но визуально line каждый тап поворачивается на +90°. После 4 тапов вернётся к исходной позиции — это работает потому что diagonal line симметрична.
- `MaxBounces = 64` — если в уровне сложилась петля (4+ зеркала по кольцу), луч отрисует 64 сегментов и остановится. На I4 уровнях такие конфигурации не используются.
- Зеркала могут быть размещены в любых cells, **включая ту же что и эмиттер** — но в коде не предусмотрено особого случая, mirror просто перекроет emitter визуально и луч начнётся из центра mirror cell. В тестовых уровнях такого нет.
- Reset мгновенно сбрасывает rotation без анимации — это намеренно, дает чёткое чувство сброса. В Iteration 9 добавим волновую анимацию reset.
