# LaserGame — Iteration 02: Level Select

## Что сделано в этой итерации

Сцена выбора уровня с прокруткой 30 кнопок, состояниями locked/unlocked/completed, отображением звёзд и интеграцией с MainMenu и Game.

### Новые скрипты
- **GameSession** — статический класс с `CurrentLevel`, передаёт выбранный уровень в Game сцену
- **LevelButton** — компонент одной кнопки уровня: 3 визуальных состояния (locked / unlocked / completed), pulse border на unlocked, shake на locked, scale punch на тапе по unlocked
- **LevelSelectController** — спавнит 30 кнопок из template, подписывается на их клики, делает stagger fade-in анимацию

### Обновлённые скрипты
- **MainMenuController** — поле `gameSceneName` переименовано в `nextSceneName`, default = `"LevelSelect"`. Логика осталась та же: `Play → fade → next scene`.

### Обновлённый editor скрипт Iteration 1
- `Iteration01_Setup.cs` — обновлено имя поля под новый MainMenuController. Логика та же.

---

## Что меняется со стороны UI

### MainMenu
Кнопка Play теперь ведёт на `LevelSelect`, а не на `Game`. Editor скрипт Iteration 02 автоматически обновляет ссылку в существующей MainMenu сцене.

### Game (placeholder)
Кнопка Back теперь возвращает на `LevelSelect`, а не на `MainMenu`. Editor скрипт Iteration 02 автоматически обновляет.

### Новая сцена LevelSelect
- Top HUD (sticky сверху, ~200px): Back button (magenta), Title "SELECT LEVEL" (cyan), Coins HUD (right)
- ScrollView (vertical, elastic) с GridLayoutGroup 3 колонки
- 30 кнопок уровней, cellSize 320×320

### Состояния LevelButton
- **Locked** — тёмный фон, текст "LOCKED" (dim grey), без border, без номера/звёзд. На тап → shake + heavy haptic.
- **Unlocked (0★)** — panel-фон, **cyan glow border (пульсирует)**, большой cyan номер, 3 dim звезды снизу
- **Completed (1-3★)** — чуть светлее panel-фон, **yellow glow border (статичный)**, большой white номер, 3 звезды снизу (заполненные жёлтым по количеству, остальные dim)

---

## Установка

Распаковать архив **поверх существующего проекта** (после Iteration 1). Будут заменены:
- `Assets/LaserGame/Scripts/MainMenuController.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration01_Setup.cs`

И добавлены:
- `Assets/LaserGame/Scripts/GameSession.cs`
- `Assets/LaserGame/Scripts/LevelButton.cs`
- `Assets/LaserGame/Scripts/LevelSelectController.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration02_Setup.cs`

После распаковки Unity рекомпилирует.

---

## Настройка через editor скрипт

### Главная команда
**`LaserGame → Iteration 02 → Setup Level Select Scene`**

Эта команда:
1. Создаёт `Assets/LaserGame/Scenes/LevelSelect.unity` (если есть — обновляет)
2. Строит весь UI (HUD, ScrollView, template кнопки)
3. Открывает существующую `MainMenu.unity` и обновляет `MainMenuController.nextSceneName = "LevelSelect"`
4. Открывает существующую `Game.unity` и обновляет `GameSceneBackBridge.targetSceneName = "LevelSelect"`
5. Добавляет LevelSelect в Build Settings и упорядочивает: MainMenu → LevelSelect → Game
6. В конце оставляет открытой LevelSelect

### Тестовые команды
- `LaserGame → Iteration 02 → Unlock All Levels (Test)` — выставляет `unlockedLevel = 30`, чтобы посмотреть как выглядят все unlocked кнопки
- `LaserGame → Iteration 02 → Set Random Stars (Test)` — разблокирует все 30 и выставляет случайные звёзды (0-3) каждому уровню — для теста completed-состояний
- `LaserGame → Iteration 02 → Reset Levels Progress` — `unlockedLevel = 1`, чистит все звёзды

Editor команды Iteration 1 продолжают работать (`Setup Both Scenes`, `Reset Save Data`).

---

## Как тестировать

1. Запустить `LaserGame → Iteration 02 → Setup Level Select Scene`
2. Открыть MainMenu сцену → Play
3. Проверить поток:
   - **MainMenu → Play → fade → LevelSelect**
   - HUD появляется сверху со slide+fade
   - Кнопки уровней появляются снизу stagger-каскадом (scale + fade, OutBack)
   - Уровень 1 — unlocked (cyan border пульсирует, номер cyan)
   - Уровни 2-30 — locked (тёмные, "LOCKED")
   - Тап по locked → shake + тактилка (на устройстве)
   - Тап по unlocked → scale punch + sound + fade → Game сцена
   - **Game → Back → fade → LevelSelect** (не на MainMenu!)
   - **LevelSelect → Back → fade → MainMenu**

4. Проверить тестовые состояния:
   - `Unlock All Levels (Test)` → перезапуск Play Mode → все 30 unlocked, cyan
   - `Set Random Stars (Test)` → перезапуск → видны 3 состояния (unlocked / 1★ / 2★ / 3★)
   - `Reset Levels Progress` → только уровень 1 unlocked

5. Скролл уровней — нижние ряды (4-10) доступны через ScrollView, elastic

---

## Ожидаемый результат

Полированная сцена выбора уровня с тремя визуальными состояниями кнопок, плавным staggered появлением, рабочей навигацией Main Menu ↔ Level Select ↔ Game, сохранением прогресса, и тестовыми командами для проверки всех состояний без необходимости проходить уровни.

---

## Изменения с Iteration 1

- `MainMenuController.gameSceneName` → `nextSceneName` (default = `"LevelSelect"`)
- `Iteration01_Setup.cs` — обновление поля под новое имя
- `MainMenu.unity` — `MainMenuController.nextSceneName` теперь `"LevelSelect"` (обновляется editor скриптом Iteration 02)
- `Game.unity` — `GameSceneBackBridge.targetSceneName` теперь `"LevelSelect"` (обновляется editor скриптом Iteration 02)

---

## Известные особенности

- Кнопка Back в HUD отображается символом `<`. В финале можно заменить на спрайт стрелки.
- Pulse border на unlocked-кнопках использует один tween на каждую кнопку — производительность ОК для 30 элементов на iOS.
- Виден только border-pulse у unlocked. Completed (с звёздами) — статичный yellow border. Так задумано — фокус привлекается к ближайшему доступному уровню.
- Шаблон `LevelButtonTemplate` хранится как inactive child Canvas — не попадает в layout, не рендерится, используется только для Instantiate.
