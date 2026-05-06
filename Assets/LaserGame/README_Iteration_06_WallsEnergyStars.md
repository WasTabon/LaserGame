# LaserGame — Iteration 06: Walls + Energy Stars

## Что сделано

Стены блокируют луч. Energy Stars дают монеты при сборе с летящей анимацией к HUD-счётчику. Условие 2★ теперь реальное — все Energy Stars на уровне собраны.

### Новые скрипты
- **WallElement** — пассивный блокер. Solid magenta-dark body с magenta neon border + 3 диагональные полоски внутри для "блочной" текстуры. Не интерактивен. Размер cellSize × 0.92 (заметно крупнее mirror/battery).
- **EnergyStarElement** — собирается лучом. TMP "★" + glow halo. Idle pulse (alpha 0.4↔1.0). При сборе: glow burst (scale 1→2.4 + fade), star scale-down + alpha 0.25, light haptic, sound.

### Обновлённые скрипты
- **LevelDefinition** — добавлен `walls: List<Vector2Int>`.
- **RayCalculator** — теперь принимает `List<Vector2Int> walls`. При движении луча: если в `next` стена → segment закрывается на 0.55 границы между cur и wall (визуально луч упирается в стену) → break.
- **GameController** — поля `wallTemplate`, `energyStarTemplate`, `coinFlyHost`, `coinsIconRect`. Spawn/clear walls и energy stars через единый `ClearAllElements` (рефакторинг — `ClearList<T>` универсальный helper). `RecalculateRay` теперь учитывает walls и проверяет energy stars в visitedCells. Новый метод `CollectEnergyStar` → `PlayCoinFly` → spawning yellow coin Image flying от star до coinsIcon с DOTween. `TickCounterDuringFly` корутина инкрементирует displayed coins постепенно (0.7s ease).
- **AllEnergyStarsCollected** — теперь реальная проверка по `_activeEnergyStars`.

### Game Scene additions
- `WallElementTemplate` (hidden child Canvas)
- `EnergyStarElementTemplate` (hidden child Canvas)
- `CoinFlyHost` (full canvas RectTransform, над HUD, под WinFlash и popup) — куда спавнятся flying-coins

Также Iteration06 переупорядочивает siblings Canvas в правильный render order: Background → Field → HUD → ResetButton → CoinFlyHost → WinFlash → Popup → Logic → Templates.

---

## Логика монет

**Energy Star собирается:**
1. Save коинт сразу: `SaveSystem.Data.coins += 5`, save persistence
2. Анимация: star burst + летящая yellow Image-coin от world-pos звезды к coins icon HUD
3. Параллельно: counter в HUD тикает от oldTotal до newTotal за 0.7s (`TickCounterDuringFly` через unscaled time)
4. При landing: punch-scale на coins icon + click sound + light haptic

**Win bonus в popup:** только базовая награда `10 + stars * 5` (15/20/25 за 1/2/3★). Energy stars дают coins **сразу при сборе**, не дублируются в popup.

---

## Установка

Распаковать поверх существующего проекта (после Iteration 5).

**Новые файлы:**
- `Assets/LaserGame/Scripts/WallElement.cs`
- `Assets/LaserGame/Scripts/EnergyStarElement.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration06_Setup.cs`

**Обновлённые файлы:**
- `Assets/LaserGame/Scripts/LevelDefinition.cs` — добавлено поле `walls`
- `Assets/LaserGame/Scripts/RayCalculator.cs` — новая сигнатура с walls
- `Assets/LaserGame/Scripts/GameController.cs` — большой расширенный (+ refactor ClearAllElements)

Editor скрипты I3-I5 не трогаем — совместимы.

---

## Настройка через editor скрипт

### Главная команда
**`LaserGame → Iteration 06 → Update Game Scene`**

Создаёт wall и energy star templates, CoinFlyHost. Прокидывает все ссылки в GameController. Также переупорядочивает siblings канваса для правильного render order.

### Тестовые уровни
**Запускать после Update Game Scene (I3-I6).**

- **`Test Level - Wall Block`** — 5×5, эмиттер (0,2)→, wall (2,2), battery (4,2). **Демонстрация блокировки** — луч упирается в стену, battery не достигнута. **Не проходимо** (нет зеркал) — это namesно для теста стены. Выйти через MENU.
- **`Test Level - Wall Detour`** — 5×5, wall (2,2), 2 mirrors (1,2) `\` + (1,4) `/`, battery (4,4). Игрок поворачивает оба зеркала: (1,2) в `/` → up, (1,4) в `\` → right → battery. `maxMovesForThreeStars = 2`.
- **`Test Level - Energy Star Bonus`** — 5×5, mirror (3,2) `\`, battery (3,4), energy star (1,2). Star на initial-пути луча → собирается мгновенно при загрузке (видна летящая coin к HUD). Затем повернуть mirror в `/` → battery → win.
- **`Test Level - All Mechanics`** — 7×7, демо со всеми механиками: 3 mirrors + 2 batteries + 2 walls + 2 energy stars. `maxMovesForThreeStars = 3`.

---

## Как тестировать

### Wall блокировка
1. `Update Game Scene` → `Test Level - Wall Block`
2. Play
3. Видеть: луч от эмиттера идёт right, упирается в wall (2,2) — segment заканчивается на границе перед стеной, не проникает дальше. Battery остаётся uncharged. Игра не проходима, выйти MENU.

### Wall обход через зеркала
1. `Test Level - Wall Detour`
2. Play
3. Изначально оба зеркала `\` — луч right через (0,2), отражается от (1,2) `\` → down → out. Battery не charged.
4. Тап (1,2) → `/` → луч поднимается up, проходит через (1,3), (1,4)
5. (1,4) изначально `/` (или `\`?) — поправлю в коде если нужно.
6. После правильной комбинации — battery (4,4) charges → win

### Energy Star сбор
1. `Test Level - Energy Star Bonus`
2. Play
3. Видеть: луч идёт right через (0,2), (1,2) — там energy star, она **сразу** загорается burst-эффектом
4. Yellow coin-image спавнится на месте star, летит по diagonal к coins icon HUD (0.7s)
5. Counter в HUD начинает тикать (от oldCoins до oldCoins+5)
6. При landing: coin исчезает, coins icon делает punch-scale, click sound, haptic
7. Star остаётся видимой как "collected" (alpha 0.25, smaller scale)
8. Тапнуть mirror — battery charges → win
9. **3★** если уложился в 1 ход (energy star уже собран ДО первого хода → 2★ сразу гарантированы) → bonus +25 coins на popup
10. **Total добавлено**: 5 (energy star) + 25 (3★ win) = 30 coins

### All Mechanics
1. `Test Level - All Mechanics`
2. Play
3. 7×7 поле с миррами, стенами, батареями, звёздами. Игрок должен повернуть зеркала чтобы:
   - Обойти стены
   - Собрать обе energy stars
   - Зарядить обе батареи
4. При прохождении со всеми star + в лимите ходов → 3★

---

## Архитектура

### ClearAllElements (рефакторинг I5)
Универсальный `ClearList<T>(list, onPreDestroy)` для уничтожения mirrors, batteries, walls, energy stars. После всех типов — defensive очистка всех children `elementsHolder` (для случая editor-orphans).

### CoinFly архитектура
- `coinFlyHost` — separate RectTransform над HUD (позволяет flyers overlap HUD при landing)
- Flyer — простой Image-кружок, не template. Создаётся через `new GameObject(...)` в runtime.
- `Time.unscaledDeltaTime` в TickCounter — игнорирует timescale (готово к pause в I9).

### Wall в RayCalculator
- HashSet<Vector2Int> wallSet — O(1) lookup
- Проверка ПОСЛЕ bounds check (бесполезно проверять wall за пределами поля), ПЕРЕД mirror check (стена приоритетнее)
- Сегмент закрывается на `0.55` границы (chuть проникает в wall cell визуально для контакта)

---

## Изменения с Iteration 5

- `LevelDefinition` — добавлено поле `walls`
- `RayCalculator.Calculate` — добавлен параметр `walls`
- `GameController` — рефакторинг + spawn/clear walls + energy stars + CollectEnergyStar + PlayCoinFly + TickCounterDuringFly
- Game scene — добавлены WallElementTemplate, EnergyStarElementTemplate, CoinFlyHost. Siblings reordered.

---

## Известные особенности

- **Wall Block testlevel** — не проходимый, для демонстрации блокировки. Выйти MENU. Это намеренно.
- **Energy Star остаётся "collected" после Reset?** Нет — Reset вызывает `ApplyLevelDefinition` → respawn все elements → energy stars в uncollected. Coins **не** возвращаются (already в кошельке).
- **Двойной collect одной star** — невозможен в одной игре (после первого collect star.IsCollected=true, дальнейшие visited не triggers Collect). Между играми (Replay) — собрать снова можно, +5 coins. Это эксплоит, который будет адресован в I8 (per-level "first time" tracking).
- **Wall и energy star в одной cell** — теоретически возможно через config, но визуально перекрывают друг друга и star недоступна для сбора (луч до star не доходит). Тестовые уровни не используют такие конфигурации.
- **Counter tick + Multiple stars одновременно** — если две energy stars собраны в одном RecalculateRay, ticking может flicker. В практике редко.
