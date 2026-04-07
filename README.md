# Image Platform Sorter

Windows WPF-приложение для раскладки изображений по папкам площадок на основе размеров в имени файла или фактического размера изображения.

## Что умеет

- выбирать входную папку с изображениями;
- выбирать отдельную выходную папку;
- отмечать нужные площадки чекбоксами;
- хранить список площадок и размеров в админке;
- копировать один файл сразу в несколько папок площадок;
- распознавать размер из имени файла, например `160x600.jpg`;
- при необходимости пытаться считать реальный размер изображения, если имя нестандартное.

## Быстрый старт

```powershell
git clone https://github.com/Deniska993/Lastmach.git
cd .\Lastmach
dotnet restore .\ImagePlatformSorter.sln
dotnet build .\ImagePlatformSorter.sln
dotnet run --project .\ImagePlatformSorter\ImagePlatformSorter.csproj
```

## Как это работает

1. Пользователь открывает приложение.
2. Выбирает папку с исходными файлами.
3. Отмечает площадки, для которых нужно сделать раскладку.
4. Нажимает `Запустить сортировку`.
5. Программа создаёт подпапки площадок в выходной папке и копирует туда подходящие файлы.

Если один и тот же размер нужен нескольким площадкам, исходный файл копируется в каждую из них.

## Где лежит конфиг

При первом запуске приложение создаёт `config.json` в:

`%LocalAppData%\Lastmach\ImagePlatformSorter\config.json`

Там же будут храниться площадки, добавленные через админку.

Если файл конфига будет повреждён, приложение создаст резервную копию проблемного файла и восстановит рабочий конфиг по умолчанию.

## Ввод размеров в админке

Размеры можно вставлять любым удобным способом:

- через запятую: `160x600, 240x400, 300x600`
- через пробел: `160x600 240x400 300x600`
- с новой строки

Приложение нормализует записи вида `160 x 600`, `160х600` и `160X600` в единый формат `160x600`.

## Сборка на Windows

Нужны:

- Windows 10/11
- .NET 8 SDK
- Visual Studio 2022 или `dotnet build`

Открыть:

- решение `ImagePlatformSorter.sln`

Собрать:

```powershell
dotnet build .\ImagePlatformSorter.sln
```

Запустить:

```powershell
dotnet run --project .\ImagePlatformSorter\ImagePlatformSorter.csproj
```

Собрать publish:

```powershell
dotnet publish .\ImagePlatformSorter\ImagePlatformSorter.csproj -c Release -r win-x64 --self-contained false
```

Собрать автономный publish без установленного .NET:

```powershell
dotnet publish .\ImagePlatformSorter\ImagePlatformSorter.csproj -c Release -r win-x64 --self-contained true
```

## GitHub Actions

В репозитории есть workflow:

- `.github/workflows/windows-build.yml`

Он автоматически:

- восстанавливает зависимости;
- собирает приложение на `windows-latest`;
- публикует `win-x64` артефакт сборки.

## Стартовый конфиг

Пример стартового набора лежит в `config.example.json`
