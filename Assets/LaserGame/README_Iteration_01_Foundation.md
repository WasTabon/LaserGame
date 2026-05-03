# LaserGame — Iteration 01: Foundation + Main Menu

## Что сделано в этой итерации

Фундамент проекта и Main Menu сцена. Геймплея пока нет — это Iteration 3.

### Системы
- **SaveSystem** — сохранение через PlayerPrefs + JSON (`GameData`: монеты, разблокированный уровень, звёзды по уровням, настройки sound/music/haptics)
- **AudioManager** — синглтон с DontDestroyOnLoad, переподписка на смену сцены, music/SFX источники, методы `PlayButtonClick / PlayPopupOpen / PlayPopupClose / PlaySFX`
- **HapticManager** — статический wrapper, читает `hapticsEnabled` из save
- **SceneTransitionManager** — синглтон с DontDestroyOnLoad, фейд через UI Image на отдельном Canvas (sortOrder=9999), метод `LoadScene(name)`
- **PopupBase** — базовый класс попапов: scale + fade анимация через DOTween (OutBack/InBack), подписки в OnEnable
- **UIScreenBase** — базовый класс экранов с fade-in
- **ButtonAnimator** — компонент на Button: punch scale (0.92 → 1.0 OutBack) + click sound + light haptic

### Main Menu
- Title "LASER" (cyan) + "GAME" (magenta)
- Coins HUD в правом верхнем углу
- Play button (большая cyan pill)
- Settings button (panel-style)
- Settings Popup с тремя тогглами: SOUND / MUSIC / HAPTICS + Close button + tap-on-backdrop-to-close

### Game сцена
Placeholder с надписью и кнопкой Back. Геймплей появится в Iteration 3.

---

## Установка

### 1. DOTween Free (обязательно!)
Если ещё не установлен:
1. Asset Store → найти **DOTween (HOTween v2)** (бесплатный) → Import
2. После импорта откроется окно `DOTween Utility Panel`
3. Нажать **Setup DOTween...** → **Apply** (создаст asmdef и добавит модули)

Без этого `using DG.Tweening;` не скомпилируется.

### 2. TextMeshPro Essentials
При первом открытии Main Menu сцены Unity предложит импортировать TMP Essentials — нажать **Import TMP Essentials**. Без этого тексты не будут отображаться.

### 3. Распаковка
Распаковать архив в корень проекта Unity. Структура:
```
Assets/LaserGame/
  Scripts/
    Editor/
      Iteration01_Setup.cs
    AudioManager.cs
    ButtonAnimator.cs
    GameData.cs
    GameSceneBackBridge.cs
    HapticManager.cs
    MainMenuController.cs
    PopupBase.cs
    SaveSystem.cs
    SceneTransitionManager.cs
    SettingsPopup.cs
    UIScreenBase.cs
```

---

## Настройка через editor скрипт

В меню Unity появится `LaserGame → Iteration 01`:

1. **`LaserGame → Iteration 01 → Setup Both Scenes`** — главная команда. Создаёт обе сцены (`Assets/LaserGame/Scenes/MainMenu.unity` и `Assets/LaserGame/Scenes/Game.unity`), весь UI, прокидывает все ссылки, добавляет сцены в Build Settings.

Дополнительно (по необходимости):
- `Setup Main Menu Scene` — только Main Menu
- `Setup Game Scene (placeholder)` — только Game
- `Reset Save Data` — очищает PlayerPrefs ключ сохранения

---

## Как тестировать

1. Открыть сцену `Assets/LaserGame/Scenes/MainMenu.unity`
2. Нажать Play в редакторе
3. Проверить:
   - Coins HUD появляется первым (fade + slide)
   - Title "LASER GAME" появляется со scale-bounce (OutBack)
   - Play и Settings кнопки выезжают снизу с небольшой задержкой
   - Тап по Play → фейд → переход на Game сцену → видна placeholder надпись и Back кнопка
   - Back возвращает в Main Menu (с фейдом)
   - Тап по Settings (на Main Menu) → попап раскрывается со scale (OutBack)
   - Тоггл SOUND/MUSIC/HAPTICS меняет фон с серого на cyan
   - Состояние тогглов сохраняется при перезапуске Play Mode
   - Тап на затемнённый фон (вне content) попапа → попап закрывается
   - Тап на CLOSE → попап закрывается
   - Все кнопки имеют punch-scale при нажатии

---

## Ожидаемый результат

Полностью полированное анимированное Main Menu с работающими переходами на Game сцену и обратно. Settings сохраняются между запусками. Тактилка работает на устройстве (на iOS — стандартный `Handheld.Vibrate`, в редакторе ничего не происходит — это норма).

Звуковые клипы не подключены (нет аудио ассетов в этой итерации) — `AudioManager.PlaySFX(null)` корректно отрабатывает no-op. В следующих итерациях клипы можно докинуть в инспектор `AudioManager` объекта на сцене.

---

## Изменения с предыдущей итерации

Это первая итерация — изменений нет.

---

## Известные особенности

- Тогглы settings — placeholder-стиль (меняется только цвет фона). В Iteration 9-10 будут полностью полированные тогглы с анимацией handle.
- Цвета схематические (dark navy / cyan / magenta / yellow / white). Финальные спрайты и цвета будут заменены вручную. Расположение элементов — финальное.
- На Game сцене сейчас только Back и заглушка-текст. Это норма — сцена будет наполняться с Iteration 3.
