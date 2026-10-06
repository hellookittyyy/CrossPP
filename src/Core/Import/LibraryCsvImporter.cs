using System.Text;
using Core.Dto;

namespace Core.Import;

/// <summary>
/// Імпорт різнорідного файлу (додаткове завдання 2): тип запису визначає префікс.
///   B;id;isbn;title;year[;author]   – книга
///   R;id;fullName;email[;phone]     – читач
/// Формат: UTF-8, роздільник ';', рядки з '#' – коментарі.
/// Один switch – два різні типи результату.
/// </summary>
public static class LibraryCsvImporter
{
    private const char Separator = ';';

    public static LibraryImportResult Load(string path)
    {
        var books = new List<BookDto>();
        var readers = new List<ReaderDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;

            switch (ParseLine(line))
            {
                case BookParsed book:
                    books.Add(book.Value);
                    break;
                case ReaderParsed reader:
                    readers.Add(reader.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new LibraryImportResult(books, readers, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            // ---- книги: B;id;isbn;title;year[;author]
            ["B", ..] and ({ Length: < 5 } or { Length: > 6 })
                => new ParseFailed($"книга: очікую 5-6 колонок, отримав {parts.Length}"),
            ["B", "", ..] or ["B", _, "", ..] or ["B", _, _, "", ..]
                => new ParseFailed("книга: id, ISBN або назва порожні"),
            ["B", _, _, _, var year, ..] when !FieldRules.IsInteger(year)
                => new ParseFailed($"книга: рік '{year}' не є цілим числом"),
            ["B", _, _, _, var year, ..] when !FieldRules.IsPlausibleYear(FieldRules.ParseInteger(year))
                => new ParseFailed($"книга: рік {year} поза межами {FieldRules.MinYear}–{DateTime.Today.Year}"),
            ["B", var id, var isbn, var title, var year]
                => new BookParsed(new BookDto(id, isbn, title, FieldRules.ParseInteger(year))),
            ["B", var id, var isbn, var title, var year, var author]
                => new BookParsed(new BookDto(id, isbn, title, FieldRules.ParseInteger(year), FieldRules.NullIfEmpty(author))),

            // ---- читачі: R;id;fullName;email[;phone]
            ["R", ..] and ({ Length: < 4 } or { Length: > 5 })
                => new ParseFailed($"читач: очікую 4-5 колонок, отримав {parts.Length}"),
            ["R", "", ..] or ["R", _, "", ..]
                => new ParseFailed("читач: id або ПІБ порожні"),
            ["R", _, _, var email, ..] when !email.Contains('@')
                => new ParseFailed($"читач: email '{email}' некоректний"),
            ["R", var id, var name, var email]
                => new ReaderParsed(new ReaderDto(id, name, email)),
            ["R", var id, var name, var email, var phone]
                => new ReaderParsed(new ReaderDto(id, name, email, FieldRules.NullIfEmpty(phone))),

            // ---- усе інше
            [var kind, ..] => new ParseFailed($"невідомий тип запису '{kind}' (очікую B або R)"),
            _ => new ParseFailed("порожній рядок")
        };
    }

    private abstract record ParseOutcome;
    private sealed record BookParsed(BookDto Value) : ParseOutcome;
    private sealed record ReaderParsed(ReaderDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
