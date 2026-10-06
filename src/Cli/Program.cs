using System.Text;
using Core;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

if (args.Contains("--env"))
{
    PrintEnvironment();
    return 0;
}

bool mixed = args.Contains("--mixed");
string[] positional = args.Where(a => !a.StartsWith("--")).ToArray();
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

static void PrintBooks(ImportResult<BookDto> result)
{
    Console.WriteLine($"Завантажено записів: {result.Items.Count}");
    foreach (BookDto b in result.Items.Take(5))
        Console.WriteLine($"  {b.Id,-6} {b.Isbn,-17} {b.Title,-30} {b.Year,4}  {b.Author ?? "—"}");

    PrintErrors(result.Errors);
    
    // Статистика (дод. завдання 3) рахується тут у Cli з даних результату
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