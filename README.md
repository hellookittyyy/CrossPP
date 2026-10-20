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

## Лабораторна 5 – Інверсія залежностей, сховище, сервіс, Composition Root

### Предметна область та компоненти
- **Домен:** Бібліотека
- **Головна сутність сховища:** `BookCopy` (примірник книги)
- **Інтерфейс сховища:** `Core.Abstractions.IBookStore`
- **Реалізації сховища:**
  1. `Core.Storage.InMemoryBookStore` — збереження в `Dictionary<string, BookCopy>` у пам'яті (засівання даними через `SampleData.BookCopies()`). Дані зникають після завершення роботи програми.
  2. `Core.Storage.FileBookStore` — кешування в пам'яті, ледаче завантаження (`EnsureLoaded`) та скидання змін на диск (`Flush`) у форматі JSON (`data/catalog.json`) через DTO (`BookCopyDto`). Кожен запис списку містить і дані примірника (`Id`, `Isbn`, `IsIssued`), і дані видання (`Title`, `Author`). Дані накопичуються між перезапусками.
  3. `Core.Storage.CachingBookStore` *(додаткове завдання 1)* — патерн **Декоратор**, що обгортає будь-який `IBookStore`, кешуючи запити `List()` та `GetById(id)` і скидаючи кеш під час мутацій (`Add`, `Update`, `Remove`).
- **Сервіс:** `Core.Services.LendingService` — містить бізнес-операції:
  - `AddBook(isbn, [title], [author], [id])` — створення примірника (з даними видання) з генерацією унікального ідентифікатора (`Guid.NewGuid().ToString("N")[..8]`) та збереження;
  - `IssueCopy(id)` — видача примірника читачу (перевірка стану та виклик `copy.Issue()`);
  - `ReturnCopy(id)` — повернення примірника до бібліотеки (виклик `copy.Return()`);
  - `RemoveCopy(id)` — видалення примірника зі сховища;
  - `All()` — отримання повного списку примірників (`IReadOnlyList<BookCopy>`);
  - `Find(id)` — пошук примірника за ID (повертає `BookCopy?`);
  - `Search(predicate)` *(додаткове завдання 2)* — фільтрація з предикатом `Func<BookCopy, bool>` (підготовка до LINQ).
- **Фабрика сховищ:** `Cli.StoreFactory` *(додаткове завдання 3)* — інкапсулює логіку вибору реалізації та конфігурацію шляхів до файлу за аргументами CLI.

### Схема залежностей
```
Cli (Program.cs / StoreFactory)
  │
  ├──► LendingService (Core.Services)
  │      │
  │      └──► IBookStore (Core.Abstractions) ◄────┐ (реалізує)
  │                                               │
  │──► [instantiates] ───────────────┬────────────┴─────────────┐
                                     │                          │
                           InMemoryBookStore              FileBookStore
                         (Core.Storage, Dictionary)    (Core.Storage, JSON)
                                                                │
                                                                ▼
                                                        data/catalog.json
```
- Проєкт `Core` містить лише абстракцію `IBookStore`, доменну логіку та сервіс `LendingService`.
- `LendingService` приймає `IBookStore` у конструкторі (`Dependency Injection`) і не має залежностей від файлової системи чи конкретних класів сховищ (`Dependency Inversion`).
- `Program.cs` є єдиним місцем застосунку (**Composition Root**), де створюються конкретні екземпляри класів.

### Методи інтерфейсу IBookStore
| Метод | Сигнатура | Призначення |
|-------|-----------|-------------|
| `List` | `IReadOnlyList<BookCopy> List()` | Повертає список усіх примірників; повертає `IReadOnlyList`, щоб клієнтський код не міг мутувати колекцію в обхід методів сховища |
| `GetById` | `BookCopy? GetById(string id)` | Знаходить примірник за ID; тип результату `BookCopy?` (nullable), оскільки запис може бути відсутній |
| `Add` | `void Add(BookCopy item)` | Додає новий примірник; перевіряє унікальність ID у колекції (`ContainsKey`), викидаючи `InvalidOperationException` при колізії |
| `Update` | `void Update(BookCopy item)` | Оновлює стан наявного примірника у сховищі |
| `Remove` | `bool Remove(string id)` | Видаляє примірник за ID; повертає `true`, якщо запис знайдено і видалено |

### Фрагмент Composition Root (Program.cs)
```csharp
bool useFile = args.Contains("--file");
string dataPath = StoreFactory.DataPath();   // data/catalog.json

IBookStore store = useFile
    ? new FileBookStore(dataPath)
    : new InMemoryBookStore(SampleData.BookCopies());

var service = new LendingService(store);
```

### Структура проєкту після тижня 5
```
src/Core/
  Abstractions/
    IBookStore.cs              (НОВЕ: контракт сховища)
  Storage/
    InMemoryBookStore.cs       (НОВЕ: реалізація в пам'яті через Dictionary)
    FileBookStore.cs           (НОВЕ: реалізація у файлі data/catalog.json)
    CachingBookStore.cs        (НОВЕ: додаткове завдання 1 — патерн Декоратор)
  Services/
    LendingService.cs          (НОВЕ: бізнес-операції бібліотеки)
  SampleData.cs                (НОВЕ: 20 зразкових записів BookCopy)
  Domain/
    BookCopy.cs                (сутність примірника книги)
    Loan.cs                    (агрегат видачі)
    LoanNote.cs                (нотатка)
    LoanStatus.cs              (enum)
    DomainImporter.cs
  Dto/
    BookCopyDto.cs             (формат збереження)
    BookDto.cs
    LoanDto.cs
    LoanNoteDto.cs
    ReaderDto.cs
    ImportResult.cs
  Import/
    BookCsvImporter.cs
    BookJsonImporter.cs
    LibraryCsvImporter.cs
src/Cli/
  Program.cs                   (Composition Root + сценарії демонстрації та відмови)
  StoreFactory.cs              (НОВЕ: додаткове завдання 3 — фабрика сховищ)
data/
  catalog.json                 (створюється автоматично під час запуску з --file)
```

### Команди для тестування

```bash
# 1. Запуск у режимі пам'яті (InMemoryBookStore)
# Дані засіваються з SampleData (20 записів), після виходу зміни зникають:
dotnet run --project src/Cli

# 2. Запуск у файловому режимі (FileBookStore)
# Дані зберігаються у data/catalog.json та накопичуються між повторними запусками:
dotnet run --project src/Cli -- --file

# 3. Перегляд збереженого файлу даних:
cat data/catalog.json

# 4. Запуск доменної моделі (Лабораторна 4):
dotnet run --project src/Cli -- --domain

# 5. Запуск імпорту CSV / JSON (Лабораторна 3):
dotnet run --project src/Cli -- data/sample.csv
dotnet run --project src/Cli -- data/sample.json
dotnet run --project src/Cli -- --mixed data/mixed.csv
```

### Самоперевірка перед здачею
- [x] Пошук у `src/Core` за `FileBookStore`: лише у файлі самої реалізації (`src/Core/Storage/FileBookStore.cs`), у сервісі — відсутній.
- [x] Пошук за `Console.` у `src/Core`: 0 збігів (Core не виконує ввід/вивід у консоль).
- [x] Повторні запуски з `--file` дають накопичення даних у `data/catalog.json`.
- [x] Якщо прибрати або закоментувати `FileBookStore`, `LendingService` все одно успішно компілюється.
- [x] Тип параметра конструктора `LendingService` — інтерфейс `IBookStore`, а не клас.
- [x] Жодного використання `Microsoft.Extensions.DependencyInjection` (ручне впровадження залежностей).

### Відповіді на запитання до захисту
1. **Що таке Composition Root і де він у проєкті?**  
   Composition Root — це єдина точка входу в застосунок, де створюються всі конкретні екземпляри класів і конфігуруються зв'язки між ними. У нашому проєкті це файл `src/Cli/Program.cs` (або допоміжна фабрика `StoreFactory.cs` у Cli).
2. **Чому сервіс не створює сховище сам? Наведіть два наслідки.**  
   Якщо сервіс сам викликає `new FileBookStore(...)`:
   - *Неможливість модульного тестування:* тест не зможе підставити швидке in-memory сховище або mock без доступу до реального диска.
   - *Жорстке зв'язування (tight coupling):* для зміни формату сховища (наприклад, перехід на базу даних або хмару) доведеться переписувати доменний сервіс.
3. **Різниця між Dependency Inversion і Dependency Injection:**  
   - *Dependency Inversion Principle (DIP)* — архітектурний принцип: високорівневі модулі (`LendingService`) не повинні залежати від низькорівневих (`FileBookStore`); обидва повинні залежати від абстракцій (`IBookStore`).
   - *Dependency Injection (DI)* — патерн реалізації, спосіб передачі залежності (об'єкта `IBookStore`) ззовні через конструктор, метод або властивість.
4. **Чому `List()` повертає `IReadOnlyList<BookCopy>`, а не `List<BookCopy>`?**  
   Щоб запобігти мутації внутрішньої колекції сховища ззовні (наприклад, через виклик `.Add()` чи `.Clear()` безпосередньо над повернутим списком), минаючи методи валідації сховища та збереження на диск.
5. **Що зміниться в LendingService, коли сховищем стане база даних?**  
   У `LendingService` не зміниться **жодного рядка коду**. Буде створено нову реалізацію `EfBookStore : IBookStore`, а в `Program.cs` (або `StoreFactory`) буде змінено один рядок створення об'єкта.
6. **Навіщо DTO між файлом і сутністю, якщо можна серіалізувати сутність?**  
   Доменна сутність має інкапсульований стан, приватні сетери, конструктори з валідацією та поведінкові методи. Серіалізатор вимагає або відкритих сетерів, або конструкторів без логіки. DTO відокремлює формат зовнішнього зберігання від моделі предметної області, дозволяючи змінювати схему JSON без ламання бізнес-правил сутності.
7. **Скільки методів має бути в інтерфейсі і чому не «більше про запас»?**  
   Лише ті методи, які безпосередньо потрібні клієнтам (принцип Interface Segregation Principle). Метод «про запас» змушує кожну реалізацію (пам'ять, файл, база даних, mock) писати й підтримувати зайвий код або викидати `NotImplementedException`.
8. **Як перевірити сервіс тестом, не торкаючись диска?**  
   Створити екземпляр `InMemoryBookStore`, передати його в конструктор `LendingService(store)` і протестувати бізнес-методи сервісу без жодних операцій введення/виведення з файловою системою.
9. **Чому FileBookStore кешує дані, а не читає файл на кожен виклик?**  
   Операції з диском (читання, парсинг JSON) повільні на кілька порядків порівняно з операціями в RAM. Кеш у `Dictionary` забезпечує миттєвий пошук за ID $O(1)$ та доступ до списку.
10. **Чи потрібен контейнер DI у консольному застосунку?**  
    Ні. Для консольного застосунку з невеликою кількістю сервісів ручне впровадження (Pure DI) через конструктор у `Program.cs` є простішим, наочнішим, не вимагає додаткових бібліотек і забезпечує сувору перевірку типів під час компіляції.