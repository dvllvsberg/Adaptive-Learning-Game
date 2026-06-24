# Release v1.0-thesis — Windows build

## Сборка .exe в Unity

1. Открой проект в Unity **2022.3.62f3**.
2. Меню: **Build → Windows Thesis Release (v1.0)**.
3. После сборки откроется папка `Builds/Windows/` с файлом `AdaptiveLearningGame.exe`.

## Подготовка ZIP для GitHub Release

1. Заархивируй **всё содержимое** папки `Builds/Windows/` (не только `.exe`, нужны `_Data` и `.dll`).
2. Назови архив: `Adaptive-Learning-Game-v1.0-thesis-Windows.zip`.

## Публикация Release на GitHub

1. Открой: https://github.com/dvllvsberg/Adaptive-Learning-Game/releases/new
2. Tag: `v1.0-thesis`
3. Title: `v1.0-thesis — Master's thesis prototype`
4. Описание:

```
Первая публичная версия прототипа обучающей 2D-игры (магистерская диссертация).

- Главное меню и выбор разделов
- 3 мини-игры (цвета, фигуры, числа)
- 10 уровней в каждой мини-игре
- Локализация: RU / KZ / EN
- Help, Pause, Settings

Windows x64 build. Распакуй ZIP, открой папку `Build1` и запусти `2D Learning Game.exe`.
```

5. Прикрепи ZIP и нажми **Publish release**.

## Topics репозитория

На странице репозитория → ⚙️ **Settings** (или шестерёнка About) → **Topics** → добавь:

`unity` `educational-game` `2d-game` `csharp` `unity2d` `learning-game` `cognitive-skills` `thesis-project`
