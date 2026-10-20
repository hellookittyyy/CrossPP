using System.Text;
using Cli;
using Core;
using Core.Abstractions;
using Core.Domain;
using Core.Dto;
using Core.Import;
using Core.Services;
using Core.Storage;

Console.OutputEncoding = Encoding.UTF8;

if (args.Contains("--env"))
{
    PrintEnvironment();
    return 0;
}

if (args.Contains("--domain"))
{
    RunDomainScenarios();
    return 0;
}

bool mixed = args.Contains("--mixed");
string[] positional = args.Where(a => !a.StartsWith("--")).ToArray();
bool hasDataFile = positional.Length > 0 &&
    (positional[0].EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
     positional[0].EndsWith(".json", StringComparison.OrdinalIgnoreCase));

if (mixed || hasDataFile || args.Contains("--import"))
{
    string path = positional.Length > 0
        ? positional[0]
        : Path.Combine("data", mixed ? "mixed.csv" : "sample.csv");

    if (!File.Exists(path))
    {
        Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
        return 1;
    }

    if (mixed)
    {
        PrintLibrary(LibraryCsvImporter.Load(path));
        return 0;
    }

    ImportResult<BookDto>? result = Path.GetExtension(path).ToLowerInvariant() switch
    {
        ".csv" => BookCsvImporter.Load(path),
        ".json" => BookJsonImporter.Load(path),
        _ => null
    };

    if (result is null)
    {
        Console.WriteLine($"Непідтримуваний формат '{Path.GetExtension(path)}': очікую .csv або .json");
        return 2;
    }

    PrintBooks(result);
    return 0;
}

RunStoreScenarios(args);
return 0;

static void RunStoreScenarios(string[] args)
{
    Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
    Console.WriteLine("║   ЛАБОРАТОРНА 5 — Сховище + Сервіс: Бібліотека           ║");
    Console.WriteLine("╚══════════════════════════════════════════════════════════╝");
    Console.WriteLine();

    bool useFile = args.Contains("--file");
    string dataPath = StoreFactory.DataPath();

    IBookStore store = useFile
        ? new FileBookStore(dataPath)
        : new InMemoryBookStore(SampleData.BookCopies());

    var service = new LendingService(store);

    Console.WriteLine($"Сховище: {store.GetType().Name}");
    if (useFile)
    {
        Console.WriteLine($"Файл сховища: {dataPath}");
        Console.WriteLine($"Завантажено з диска примірників: {service.All().Count}");
    }
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 1: Додавання, видача та повернення примірника ===");
    Console.WriteLine();

    var created = service.AddBook("978-966-97166-0-2", "Кобзар", "Тарас Шевченко");
    Console.WriteLine($"  [+] Додано новий примірник: {created}");

    service.IssueCopy(created.Id);
    var issued = service.Find(created.Id);
    Console.WriteLine($"  [→] Після видачі читачу:  {issued}");

    service.ReturnCopy(created.Id);
    var returned = service.Find(created.Id);
    Console.WriteLine($"  [←] Після повернення:     {returned}");
    Console.WriteLine();

    Console.WriteLine($"=== Список примірників у сховищі (усього: {service.All().Count}) ===");
    Console.WriteLine();
    foreach (var p in service.All())
        Console.WriteLine($"  {p.Id,-10} {p.Isbn,-20} {p.Title ?? "—",-34} {(p.IsIssued ? "видано" : "в наявності")}");
    Console.WriteLine();

    Console.WriteLine($"=== Пошук за id={created.Id} ===");
    Console.WriteLine();
    var found = service.Find(created.Id);
    Console.WriteLine(found is not null
        ? $"  [✓] Знайдено: {found}"
        : $"  [✗] Не знайдено запис з id={created.Id}");
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 2: Відмова та обробка помилок ===");
    Console.WriteLine();

    TryDo("Спроба додати дублікат id",
        () => service.AddBook("978-0-00-000001-0", "Дублікат", id: created.Id));

    TryDo("Спроба видачі неіснуючого примірника",
        () => service.IssueCopy("NONEXISTENT-999"));

    TryDo("Спроба повернення неіснуючого примірника",
        () => service.ReturnCopy("NONEXISTENT-999"));

    service.IssueCopy(created.Id);
    TryDo("Повторна видача вже виданого примірника",
        () => service.IssueCopy(created.Id));

    service.ReturnCopy(created.Id);

    TryDo("Повернення невиданого примірника",
        () => service.ReturnCopy(created.Id));

    Console.WriteLine();

    Console.WriteLine("=== Додаткові завдання ===");
    Console.WriteLine();

    Console.WriteLine("1. Декоратор CachingBookStore (окреме демо-сховище в пам'яті):");
    var innerStore = new InMemoryBookStore(SampleData.BookCopies());
    var cachingStore = new CachingBookStore(innerStore);
    var cachedService = new LendingService(cachingStore);

    var c1 = cachedService.AddBook("978-0-00-000099-0", "Тестове видання");
    Console.WriteLine($"   • Додано через кешоване сховище: {c1}");
    Console.WriteLine($"   • Примірників у кешованому сховищі: {cachedService.All().Count}");
    Console.WriteLine($"   • Тип сховища: {cachingStore.GetType().Name} (обгортає {innerStore.GetType().Name})");
    Console.WriteLine();

    Console.WriteLine("2. Пошук з предикатом Func<BookCopy, bool> (ISBN починається з '978-966'):");
    var isbn978 = service.Search(c => c.Isbn.StartsWith("978-966"));
    Console.WriteLine($"   • Знайдено примірників: {isbn978.Count}");
    foreach (var c in isbn978)
        Console.WriteLine($"     - {c}");
    Console.WriteLine();

    Console.WriteLine("3. Фабрика StoreFactory.Create(args):");
    Console.WriteLine($"   • без --file → {StoreFactory.Create([]).GetType().Name}");
    Console.WriteLine($"   • з --file   → {StoreFactory.Create(["--file"]).GetType().Name}");
}

static void TryDo(string title, Action action)
{
    try
    {
        action();
        Console.WriteLine($"  [!] {title}: виняток НЕ спрацював — операція завершилась успішно!");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"  [✓] {title}: перехоплено {ex.GetType().Name} — {ex.Message}");
    }
}

static void RunDomainScenarios()
{
    Console.WriteLine("╔══════════════════════════════════════════════════╗");
    Console.WriteLine("║   ЛАБОРАТОРНА 4 — Доменна модель: Бібліотека    ║");
    Console.WriteLine("╚══════════════════════════════════════════════════╝");
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 1: успіх ===");
    Console.WriteLine();

    BookCopy copy1 = BookCopy.Create("C-001", "978-3-16-148410-0");
    BookCopy copy2 = BookCopy.Create("C-002", "978-0-13-468599-1");
    BookCopy copy3 = BookCopy.Create("C-003", "978-1-23-456789-0");
    Console.WriteLine($"  Створено: {copy1}");
    Console.WriteLine($"  Створено: {copy2}");
    Console.WriteLine($"  Створено: {copy3}");
    Console.WriteLine();

    copy1.Issue();
    Console.WriteLine($"  Після видачі: {copy1}");

    DateTime issuedOn = new(2026, 10, 1);
    Loan loan1 = Loan.Open("L-001", "C-001", "R-001", issuedOn);
    Console.WriteLine($"  {loan1}");

    loan1.AddNote("Читач попросив продовження на тиждень", new DateTime(2026, 10, 5));
    Console.WriteLine($"  Додано нотатку: {loan1.Notes[0]}");
    Console.WriteLine($"  {loan1}");

    loan1.Close(new DateTime(2026, 10, 12));
    copy1.Return();
    Console.WriteLine($"  Після повернення: {loan1}");
    Console.WriteLine($"  Примірник: {copy1}");
    Console.WriteLine();

    Console.WriteLine("  --- ToDto / FromDto ---");
    BookCopyDto copyDto = copy1.ToDto();
    Console.WriteLine($"  BookCopy → DTO: {copyDto}");
    BookCopy restoredCopy = BookCopy.FromDto(copyDto);
    Console.WriteLine($"  DTO → BookCopy: {restoredCopy}");
    Console.WriteLine();

    LoanDto loanDto = loan1.ToDto();
    Console.WriteLine($"  Loan → DTO: Id={loanDto.Id}, Status={loanDto.Status}, Notes={loanDto.Notes.Count}");
    Loan restoredLoan = Loan.FromDto(loanDto);
    Console.WriteLine($"  DTO → Loan: {restoredLoan}");
    Console.WriteLine();

    Console.WriteLine($"  Стан copy1 після всіх операцій: {copy1}");
    Console.WriteLine();

    Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");
    Console.WriteLine();

    TryDo("порожній Id примірника",
        () => BookCopy.Create("", "978-3-16-148410-0"));

    TryDo("порожній ISBN",
        () => BookCopy.Create("C-010", "   "));

    copy2.Issue();
    TryDo("повторна видача вже виданого примірника",
        () => copy2.Issue());

    TryDo("повернення невиданого примірника",
        () => copy3.Return());

    Console.WriteLine();

    TryDo("порожній Id видачі",
        () => Loan.Open("", "C-001", "R-001", DateTime.Now));

    TryDo("порожній ReaderId",
        () => Loan.Open("L-010", "C-001", "  ", DateTime.Now));

    Loan loan2 = Loan.Open("L-002", "C-002", "R-002", new DateTime(2026, 10, 5));

    TryDo("дата повернення раніше дати видачі",
        () => loan2.Close(new DateTime(2026, 9, 1)));

    loan2.Close(new DateTime(2026, 10, 10));
    TryDo("повторне закриття вже закритої видачі",
        () => loan2.Close(new DateTime(2026, 10, 11)));

    TryDo("нотатка до закритої видачі",
        () => loan2.AddNote("Спроба додати нотатку", DateTime.Now));

    TryDo("порожній текст нотатки",
        () =>
        {
            Loan tempLoan = Loan.Open("L-099", "C-099", "R-099", DateTime.Now);
            tempLoan.AddNote("   ", DateTime.Now);
        });

    Console.WriteLine();
    Console.WriteLine($"  Стан copy2 після відмов: {copy2}");
    Console.WriteLine($"  Стан copy3 після відмов: {copy3}");
    Console.WriteLine($"  Стан loan2 після відмов: {loan2}");
    Console.WriteLine();

    Console.WriteLine("=== Додаткове 1: ImportResult → доменні сутності ===");
    Console.WriteLine();

    var importItems = new List<BookDto>
    {
        new("C-100", "978-0-00-000001-0", "Кобзар", 1840),
        new("C-101", "978-0-00-000002-7", "Тигролови", 1944),
        new("", "978-0-00-000003-4", "Без ідентифікатора", 2000),
        new("C-103", "", "Без ISBN", 2001),
        new("C-104", "978-0-00-000004-1", "Лісова пісня", 1911),
    };
    var importErrors = new List<string> { "рядок 6: рік '1xyz' не є цілим числом" };
    var importResult = new ImportResult<BookDto>(importItems, importErrors);

    var domainResult = DomainImporter.ImportBookCopies(importResult);

    Console.WriteLine($"  Успішно створено: {domainResult.Items.Count} примірників");
    foreach (var item in domainResult.Items)
        Console.WriteLine($"    {item}");

    Console.WriteLine($"  Помилки ({domainResult.Errors.Count}):");
    foreach (var error in domainResult.Errors)
        Console.WriteLine($"    ! {error}");
    Console.WriteLine();

    Console.WriteLine("=== Додаткове 2: міжсутнісний інваріант (≥5 відкритих видач) ===");
    Console.WriteLine();

    string readerId = "R-100";
    var openLoans = new List<Loan>();

    for (int i = 1; i <= 5; i++)
    {
        var copy = BookCopy.Create($"C-2{i:D2}", $"978-0-00-00010{i}-0");
        copy.Issue();

        var loan = Loan.Open($"L-2{i:D2}", copy.Id, readerId,
                             new DateTime(2026, 10, i));
        openLoans.Add(loan);
        Console.WriteLine($"  Видача {i}: {loan.Id} → {copy.Id}");
    }

    Console.WriteLine($"  Відкритих видач у читача {readerId}: {openLoans.Count}");

    TryDo("видача 6-го примірника читачу з 5 відкритими видачами",
        () =>
        {
            int activeLoans = openLoans.Count(l => l.Status == LoanStatus.Open);
            if (activeLoans >= 5)
                throw new InvalidOperationException(
                    $"Читач {readerId} має {activeLoans} відкритих видач, " +
                    $"максимально дозволено 5. Спочатку поверніть хоча б один примірник.");

            var extraCopy = BookCopy.Create("C-299", "978-0-00-001099-0");
            extraCopy.Issue();
            Loan.Open("L-299", extraCopy.Id, readerId, DateTime.Now);
        });

    Console.WriteLine();
    Console.WriteLine("  ℹ Цей інваріант охоплює ДВІ сутності (Loan + Reader).");
    Console.WriteLine("    Його виносять у сервіс (тиждень 5), бо сутність");
    Console.WriteLine("    не має доступу до колекції видач іншого агрегату.");
    Console.WriteLine();

    Console.WriteLine("=== Додаткове 3: enum LoanStatus + switch expression ===");
    Console.WriteLine();

    Loan loan3 = Loan.Open("L-300", "C-300", "R-300", new DateTime(2026, 10, 1));
    Console.WriteLine($"  Стан: {loan3.Status}");

    loan3.Close(new DateTime(2026, 10, 10));
    Console.WriteLine($"  Стан: {loan3.Status}");

    TryDo("перехід Closed → Closed (заборонено)",
        () => loan3.Close(new DateTime(2026, 10, 11)));

    Console.WriteLine();
    Console.WriteLine("  ℹ Перевірка допустимих переходів виконується в методі Loan.Close()");
    Console.WriteLine("    через switch expression: Open → Closed (дозволено),");
    Console.WriteLine("    Closed → Closed (InvalidOperationException).");
}

static void PrintBooks(ImportResult<BookDto> result)
{
    Console.WriteLine($"Завантажено записів: {result.Items.Count}");
    foreach (BookDto b in result.Items.Take(5))
        Console.WriteLine($"  {b.Id,-6} {b.Isbn,-17} {b.Title,-30} {b.Year,4}  {b.Author ?? "—"}");

    PrintErrors(result.Errors);
    
    var stats = new ImportStats(result.Items.Count + result.Errors.Count, result.Items.Count, result.Errors.Count);
    Console.WriteLine($"Статистика: {stats}");
}

static void PrintLibrary(LibraryImportResult result)
{
    Console.WriteLine($"Книг: {result.Books.Count}");
    foreach (BookDto b in result.Books.Take(5))
        Console.WriteLine($"  {b.Id,-6} {b.Isbn,-17} {b.Title,-30} {b.Year,4}  {b.Author ?? "—"}");

    Console.WriteLine($"Читачів: {result.Readers.Count}");
    foreach (ReaderDto r in result.Readers.Take(5))
        Console.WriteLine($"  {r.Id,-6} {r.FullName,-30} {r.Email,-28} {r.Phone ?? "—"}");

    PrintErrors(result.Errors);
    Console.WriteLine($"Статистика: {result.Stats}");
}

static void PrintErrors(IReadOnlyList<string> errors)
{
    if (errors.Count == 0)
        return;

    Console.WriteLine($"Пропущено рядків: {errors.Count}");
    foreach (string e in errors)
        Console.WriteLine($"  ! {e}");
}

static void PrintEnvironment()
{
    EnvironmentReport report = EnvironmentInfo.Collect();

    Console.WriteLine("CrossApp - Environment Information");
    Console.WriteLine(new string('-', 52));
    Console.WriteLine($"OS             : {report.OsDescription}");
    Console.WriteLine($"Runtime        : {report.FrameworkDescription}");
    Console.WriteLine($"Architecture   : {report.ProcessArchitecture}");
    Console.WriteLine($"RID (detected) : {report.DetectedRid}");
    Console.WriteLine($"RID (from .NET): {report.ReportedRid}");
    Console.WriteLine($"Directory      : {report.BaseDirectory}");
    Console.WriteLine($"Build Note     : {EnvironmentInfo.GetBuildNote()}");
}
