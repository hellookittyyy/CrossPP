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
Розмір каталогу publish для osx-arm64: 65 MB (self-contained, net10.0)
Розмір каталогу publish для win-x64: 77 MB (self-contained, net10.0)

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
| osx-arm64 | self-contained | ~65 МБ | ні |
| osx-arm64 | framework-dependent | ~0.3 МБ | так (.NET 10) |
| win-x64 | self-contained | ~77 МБ | ні |
| win-x64 | framework-dependent | ~0.2 МБ | так (.NET 10) |

### Конвенції каталогів Core
- `Core/Dto/` — record-типи формату даних
- `Core/Domain/` — сутності з поведінкою та інваріантами
- `Core/Storage/` — реалізації сховищ

## Лабораторна 3 – Parsing, Pattern Matching

### Структура
- `Core/Dto/` — містить виключно типи даних (records): `BookDto.cs`, `ReaderDto.cs`, `ImportResult.cs`.
- `Core/Import/` — містить бізнес-логіку парсингу файлів: `BookCsvImporter.cs`, `BookJsonImporter.cs`, `LibraryCsvImporter.cs` тощо.
- `data/` — містить файли з даними (`sample.csv`, `sample.json`, `mixed.csv`) для тестування парсерів.

### Особливості реалізації
- Використано **C# Pattern Matching** (List patterns, Property patterns, Relational patterns) замість багаторазових `.Split()` та `if-else`.
- Числа обробляються з `CultureInfo.InvariantCulture`.
- Реалізовано збір помилок без переривання парсингу файлу.
- **Додаткові завдання:**
  1. Реалізовано JSON-імпортер через `System.Text.Json` із підтримкою збору помилок.
  2. Реалізовано читання змішаних файлів (книги та читачі в одному CSV) через один `switch` (`LibraryCsvImporter`).
  3. Виведено статистику парсингу (`ImportStats`).

### Команди для тестування
```bash
# Базове завдання (CSV)
dotnet run --project src/Cli -- data/sample.csv

# Додаткове завдання 1 (JSON)
dotnet run --project src/Cli -- data/sample.json

# Додаткове завдання 2 (Змішаний CSV)
dotnet run --project src/Cli -- --mixed data/mixed.csv
```

## Лабораторна 4 – Доменна модель: сутності, інваріанти, інкапсуляція

### Сутності

- **BookCopy** — примірник книги (`Id`, `Isbn`, `IsIssued`). Операції: `Issue()`, `Return()`.
- **Loan** — видача примірника читачу (`Id`, `CopyId`, `ReaderId`, `IssuedOn`, `ReturnedOn`, `Status`). Операції: `Open()`, `Close()`, `AddNote()`. Агрегат із колекцією `LoanNote`.
- **LoanNote** — нотатка до видачі (value object, незмінний після створення).
- **LoanStatus** — enum: `Open`, `Closed`.

### Зв'язок сутностей

`BookCopy` ← `Loan` (за `CopyId`). `Loan` містить колекцію `LoanNote` (агрегат).

### Інваріанти

| # | Правило | Тип винятку | Метод |
|---|---------|-------------|-------|
| 1 | Id примірника не може бути порожнім | `ArgumentException` | `BookCopy.Create` |
| 2 | ISBN не може бути порожнім | `ArgumentException` | `BookCopy.Create` |
| 3 | Не можна видати вже виданий примірник | `InvalidOperationException` | `BookCopy.Issue` |
| 4 | Не можна повернути невиданий примірник | `InvalidOperationException` | `BookCopy.Return` |
| 5 | Id видачі не може бути порожнім | `ArgumentException` | `Loan.Open` |
| 6 | CopyId не може бути порожнім | `ArgumentException` | `Loan.Open` |
| 7 | ReaderId не може бути порожнім | `ArgumentException` | `Loan.Open` |
| 8 | Дата повернення не може бути раніше дати видачі | `ArgumentOutOfRangeException` | `Loan.Close` |
| 9 | Не можна закрити вже закриту видачу | `InvalidOperationException` | `Loan.Close` |
| 10 | Текст нотатки не може бути порожнім | `ArgumentException` | `LoanNote.Create` |
| 11 | Не можна додати нотатку до закритої видачі | `InvalidOperationException` | `Loan.AddNote` |

### Додаткові завдання

1. **ImportResult → доменні сутності** (`DomainImporter.ImportBookCopies`): бере `ImportResult<BookDto>` і повертає `ImportResult<BookCopy>` із зібраними помилками інваріантів.
2. **Міжсутнісний інваріант**: не видати примірник читачу з ≥ 5 відкритих видач. Такі правила виносять у сервіс (тиждень 5), бо сутність не має доступу до колекції видач іншого агрегату.
3. **Enum LoanStatus** (`Open`, `Closed`) з перевіркою допустимих переходів через `switch expression` у `Loan.Close()`.

### Структура

```
src/Core/
  Dto/
    BookDto.cs              (record з тижня 3 — DTO книги)
    ReaderDto.cs            (record з тижня 3 — DTO читача)
    BookCopyDto.cs          (НОВЕ: DTO примірника)
    LoanDto.cs              (НОВЕ: DTO видачі)
    LoanNoteDto.cs          (НОВЕ: DTO нотатки)
    ImportResult.cs
  Domain/                   (НОВЕ: сутності з поведінкою)
    BookCopy.cs             (примірник книги)
    Loan.cs                 (видача — агрегат із нотатками)
    LoanNote.cs             (нотатка — value object)
    LoanStatus.cs           (enum стану видачі)
    DomainImporter.cs       (перетворення ImportResult → доменні сутності)
  Import/
    BookCsvImporter.cs
    BookJsonImporter.cs
    LibraryCsvImporter.cs
src/Cli/
  Program.cs                (сценарії «успіх» + «порушення інваріанту»)
```

### Команди

```bash
dotnet run --project src/Cli -- --domain
```