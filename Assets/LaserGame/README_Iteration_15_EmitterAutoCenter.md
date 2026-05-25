# LaserGame — Iteration 15: Emitter Auto-Center + Arrow Rotation

## Що зроблено

1. Стрілка емітера автоматично повертається в напрямку лазера (за `emitter.direction`)
2. При старті рівня pan автоматично snap'иться так щоб емітер був видимий (центр viewport)
3. При натисканні Reset кнопки — pan теж скидається на емітер (хук додано через editor script)

### Як працює

**EmitterAutoCenter.cs** — компонент який вішається на LaserEmitter object. У `LateUpdate` стежить за `emitter.cell` і `emitter.direction`. Як тільки щось змінюється (новий рівень, рестарт через сцену) — викликає:
- `ApplyRotation()` — обчислює angle через `Atan2(direction.y, direction.x) * Rad2Deg`, ставить на `emitter.rectTransform.localEulerAngles`
- `ApplyPan()` — викликає `FieldPanController.SnapToContentLocalPos(emitter.anchoredPos)` яка центрує content так щоб ця точка опинилась в центрі viewport (з clamping в границі)

Метод `Recenter()` публічний — щоб хукати на reset кнопку. Iteration15 editor script автоматично додає його як persistent listener до ResetButton.onClick.

### Файли

**Нові:**
- `Assets/LaserGame/Scripts/EmitterAutoCenter.cs`
- `Assets/LaserGame/Scripts/Editor/Iteration15_Setup.cs`

**Оновлений:**
- `Assets/LaserGame/Scripts/FieldPanController.cs` — додано метод `SnapToContentLocalPos(Vector2 localPos)` + рефакторинг clamp в private `ClampToBounds()`

---

## Установка

1. Розпакувати поверх проекту (після Iteration 14)
2. `LaserGame → Iteration 15 → Setup Emitter Auto-Center`
3. Запустити гру

---

## Що працює

- **Старт рівня** — pan центрується на емітері, стрілка повернута правильно
- **Next level (з popup)** — сцена перезавантажується → знов центр
- **Перехід між рівнями через LevelSelect** — те саме
- **Reset кнопка** — викликає Recenter (хукнуто через editor script)

## Обмеження

- **Restart з паузи** — якщо емітер на тій же клітинці що й раніше, не зреагує. Користувач може тапнути Reset.
- **Replay з LevelComplete popup** — те саме, що Restart з паузи.

Якщо хочеш щоб і ці випадки авто-центрувались, треба додати рядок до GameController вручну:

В методах `HandleRestart` і `HandleReplay` додай після `RecalculateRay()`:
```csharp
var auto = FindObjectOfType<EmitterAutoCenter>();
if (auto != null) auto.Recenter();
```

---

## Тест

1. Запусти setup
2. Грай L3 (великий рівень 15×15) — поле відразу проскролено на емітер (зліва)
3. Поверни кілька зеркал, поспостерігай за лазером
4. Тапни Reset — поле повертається до емітера
5. Виграй рівень, Next Level — теж скол на емітер нового рівня
6. Перевір ще раз L1 (5×5) — теж все ок (поле і так помістилось, pan не зрушується)

---

## Архітектура

```
LaserEmitter (GameObject)
├── LaserEmitter component (cell, direction, rectTransform)
└── EmitterAutoCenter component  ← НОВИЙ
    ├── references LaserEmitter
    ├── references FieldPanController
    ├── LateUpdate: detect changes → Apply
    └── Recenter(): public, hookable on Reset button
```

`FieldPanController.SnapToContentLocalPos(localPos)`:
- target offset = -localPos (бо щоб точка опинилась в центрі viewport, треба зрушити field на протилежний вектор)
- clamp в границі (як при drag)
- встановити `fieldRoot.anchoredPosition`

Stays consistent з drag bounds — на маленькому полі (5×5) clamp поверне 0 (бо max offset = 0 коли content менше viewport).
