# LaserGame — Iteration 05: Batteries + Win Condition + Level Complete

## Что сделано

Батареи, проверка условия победы, cinematic win-эффект "поле загорается", Level Complete popup со звёздами и наградой за прохождение.

### Новые скрипты
- **BatteryElement** — компонент батареи. 4 visual слоя: Glow → Body → Fill → Icon (⚡). Методы `SetChargedImmediate`, `Charge` (анимированный), `Discharge`. При charge: scale-punch + color transition + light haptic + pulse glow loop.
- **LevelCompletePopup** — попап со звёздами (3 шт), наградой в монетах, кнопками Replay / Next / Menu. Звёзды появляются по очереди (stagger 0.18s), filled-цвет yellow, empty-цвет dim grey. `Show(stars, coinsReward, hasNext)`.

### Обновлённые скрипты
- **LevelDefinition** — добавлены `batteries: List<Vector2Int>`, `energyStars: List<Vector2Int>` (заготовка для I6), `maxMovesForThreeStars: int`.
- **RayCalculator** — теперь возвращает `RayResult { segments, visitedCells }`. visitedCells — все cells через которые прошёл луч (для проверки battery hits и energy stars в I6).
- **GridSystem** — добавлен `PlayWinPulse(flashColor)`: волна-flash от центра поля наружу по cells (delay по distance).
- **GameController** — `batteryTemplate`, `winFlashOverlay`, `levelCompletePopup` ссылки. Новые методы: `SpawnBatteries`, `ClearBatteries`, `UpdateBatteryStates`, `CheckWinCondition`, `WinSequenceRoutine`, `CalculateStars`, `CalculateCoinReward`, `HandleReplay/Next/Menu`, `DebugTriggerWin`. Save progress: stars per level, unlock next level, coins reward.

### Game Scene additions
В существующую Game.unity добавляются:
- `BatteryElementTemplate` — hidden child Canvas для Instantiate
- `WinFlashOverlay` — full-canvas Image, alpha 0 → 0.55 → 0 при win
- `LevelCompletePopup` — popup со звёздами, monetary reward, 3 buttons. Изначально неактивный.

---

## Логика звёзд

- **1★** — пройти уровень (зарядить все батареи)
- **2★** — + собрать все Energy Stars (если их нет на уровне — даётся автоматически; в I6 будет реальное условие)
- **3★** — + не превысить `maxMovesForThreeStars` ходов

Coins reward: `10 + stars * 5` (т.е. 15/20/25 за 1/2/3★ соответственно).

После win сохраняется:
- `SetStarsForLevel` (max-stars не overwrite если меньше)
- `unlockedLevel = max(unlockedLevel, currentLevel + 1)` (capped at 30)
- `coins += reward`

---

## Установка

Распаковать поверх существующего проекта (после Iteration 4). Файлы:

**Новые:**
- `Assets/LaserGame/Scripts/BatteryElement.cs`
- `Assets/LaserGame/Scripts/LevelCompletePopup.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration05_Setup.cs`

**Обновлённые (заменяют I4 версии):**
- `Assets/LaserGame/Scripts/LevelDefinition.cs`
- `Assets/LaserGame/Scripts/RayCalculator.cs` — новый return type `RayResult`
- `Assets/LaserGame/Scripts/GridSystem.cs` — добавлен `PlayWinPulse`
- `Assets/LaserGame/Scripts/GameController.cs` — большой расширенный

`Iteration03_Setup.cs` и `Iteration04_Setup.cs` **не меняются** — они уже совместимы (LevelDefinition имеет inline default values для новых полей).

---

## Настройка через editor скрипт

### Главная команда
**`LaserGame → Iteration 05 → Update Game Scene`**

Создаёт:
- `BatteryElementTemplate` (hidden)
- `WinFlashOverlay` (full canvas, inactive)
- `LevelCompletePopup` со всем UI

Прокидывает ссылки в GameController.

### Тестовые уровни
**Запускать после `Update Game Scene` (Iteration 04 + 05).**

- **`Apply Default Test Level (5x5 + Battery)`** — 5×5, эмиттер (0,2)→, battery (4,2). **Instant win** при загрузке (для теста popup).
- **`Test Level - 1 Mirror + Battery`** — 5×5, mirror (2,2) `\` initially, battery (2,4). Игрок поворачивает зеркало в `/` → луч отражается вверх → батарея заряжается. `maxMovesForThreeStars=1` (3★ за 1 поворот).
- **`Test Level - 2 Batteries Pass-Through`** — 5×5, две батареи на одной линии (2,2) и (4,2). Луч проходит через обе. **Instant win**.
- **`Test Level - Mirror + 2 Batteries`** — 5×5, mirror (4,2) `\` initially, batteries (2,2) и (4,4). Поворот зеркала в `/` → луч right через battery (2,2), потом отражается up через battery (4,4). **Two batteries on path**, `maxMovesForThreeStars=1`.

### Debug команда
- **`Trigger Win (In Play Mode)`** — работает только в Play Mode. Заряжает все батареи и вызывает win sequence. Полезно для проверки popup без прохождения уровня.

---

## Как тестировать

### Базовый поток
1. Запустить `LaserGame → Iteration 05 → Update Game Scene`
2. Запустить `LaserGame → Iteration 05 → Test Level - 1 Mirror + Battery`
3. Открыть Game.unity → Play (или через MainMenu → LevelSelect → tap level)
4. Видеть:
   - Поле 5×5, эмиттер слева, mirror в центре с диагональю `\`, battery вверху неактивная (dim, серая, glow дimный)
   - Луч идёт right → reflects from `\` → goes down → out of field
   - Battery остаётся uncharged
5. Тапнуть mirror → диагональ переключается на `/`
6. Видеть:
   - Луч пересчитан: right → up
   - Battery в (2,4) делает scale-punch, fill становится bright yellow, icon ⚡ темнеет (контраст), glow загорается ярко с пульсацией
7. **Win sequence**:
   - Cells на field flash cyan волной от центра
   - Full-canvas cyan flash overlay (быстрый, 0.15s in / 0.6s out)
   - Через 0.95s появляется LevelComplete popup со scale+fade
   - Звёзды появляются по очереди (3 шт, с задержкой 0.18s между ними) — 3★ если уложились в 1 ход
   - "+ 25" монет подсвечивается
   - Кнопки REPLAY / MENU (внизу слева/справа), большая NEXT (cyan) сверху
8. Тап **REPLAY** → попап закрывается, level пересоздаётся, можно играть снова
9. Тап **NEXT** → CurrentLevel++, перезагрузка Game scene (тот же testLevel, но HUD показывает следующий номер)
10. Тап **MENU** → fade → LevelSelect

### Тест popup отдельно
1. Запустить `Test Level - Mirror + 2 Batteries`
2. Play
3. В Editor menu: `Iteration 05 → Trigger Win (In Play Mode)` — мгновенно зарядит обе батареи, запустит win sequence
4. Видеть полную cinematic анимацию + popup

### Тест прогресса
1. Запустить `Apply Default Test Level (5x5 + Battery)` (instant win)
2. Открыть MainMenu → видно coins=0, level 1 unlocked
3. Tap level 1 → Game → instant win → 3★ → +25 coins → tap MENU
4. LevelSelect: level 1 показывает 3★, coins=25, level 2 unlocked
5. Tap level 2 → Game → instant win (тот же testLevel) → ещё +25 coins → +1 unlock
6. Так можно "пройти" все 30 уровней (in I8 каждый будет иметь свой конфиг)

---

## Ожидаемый результат

- Батареи заряжаются мгновенно когда луч их пересекает, разряжаются если луч ушёл (через mirror rotate)
- Win condition: все батареи на уровне заряжены ОДНОВРЕМЕННО → сцена вспыхивает, popup
- Звёзды считаются: всегда 1★ + 1★ за все Energy Stars + 1★ за лимит ходов = 1-3★
- Coins начисляются и сохраняются между сессиями
- Прогресс уровней (unlocked + stars per level) сохраняется и видим в LevelSelect

---

## Изменения с Iteration 4

- `LevelDefinition` — добавлены 3 поля
- `RayCalculator.Calculate` — возвращает `RayResult` вместо `List<RaySegment>`
- `GridSystem` — метод `PlayWinPulse`
- `GameController` — battery handling, win check, win sequence, save progress, replay/next/menu handling
- Game scene — добавлены 3 объекта: BatteryElementTemplate, WinFlashOverlay, LevelCompletePopup

---

## Известные особенности

- "Instant win" уровни (без mirrors, луч сразу попадает на батарею) — попап появляется при загрузке. Это специально для тестирования popup. После Replay → instant win loop. Можно выйти через MENU.
- В I5 все уровни играют один и тот же `testLevel` (выбор уровня в LevelSelect не влияет на конфиг). Реальный mapping будет в I8 (LevelLoader + LevelsDatabase).
- Popup не закрывается тапом по backdrop — игрок должен явно выбрать действие.
- Battery icon "⚡" — Unicode символ. В финале можно заменить на спрайт.
- Reset во время заряженной батареи: после reset rotation → RecalculateRay → battery discharge animation (color transition без punch). 
- Win sequence через `WaitForSecondsRealtime` — игнорирует Time.timeScale (на случай pause в I9).
