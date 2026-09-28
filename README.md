# CrossApp
Наскрізний проєкт з крос-платформного програмування.

**Предметна область:** Бібліотека. 
**Сутності:** Book, BookCopy, Reader, Loan.
**Призначення:** облік видач примірників книг читачам 

## Запуск
```bash
dotnet build
dotnet run --project src/Cli 
dotnet run --project src/Cli -- --json
```
**Середовище:** .NET SDK 10.0, macOS
## Додаткове завдання
Розмір каталогу publish для osx-arm64: 83 MB
Розмір каталогу publish для win-x64: 77 MB

## Лабораторна 2 – Class Library, Publish

### Структура solution
```
CrossApp/
  CrossApp.slnx
  README.md
  .gitignore
  src/
    Core/
      Core.csproj             (multi-targeting: net8.0;net10.0)
      EnvironmentInfo.cs      (namespace Core)
    Cli/
      Cli.csproj              (ProjectReference → Core)
      Program.cs
```

### Команди
```bash
dotnet build
dotnet run --project src/Cli
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained true -f net10.0
dotnet publish src/Cli -c Release -r osx-arm64 --self-contained false -f net10.0
```

### Self-contained vs Framework-dependent

**Self-contained** — включає .NET Runtime, працює без встановленого .NET, але великий розмір.
**Framework-dependent** — лише код застосунку, потрібен встановлений .NET Runtime.

| RID | Режим | Розмір publish | Потрібен runtime |
|-----|-------|---------------|------------------|
| osx-arm64 | self-contained | ~83 МБ | ні |
| osx-arm64 | framework-dependent | ~0.2 МБ | так (.NET 10) |
| win-x64 | self-contained | ~77 МБ | ні |
| win-x64 | framework-dependent | ~0.2 МБ | так (.NET 10) |

### Конвенції каталогів Core
- `Core/Dto/` — record-типи формату даних
- `Core/Domain/` — сутності з поведінкою та інваріантами
- `Core/Storage/` — реалізації сховищ