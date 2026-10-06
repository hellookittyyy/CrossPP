using System.Text;
using Core.Dto;

namespace Core.Import;

/// <summary>
/// Імпорт книг з CSV.
/// Формат: UTF-8, роздільник ';', заголовок "id;..." необов'язковий,
/// колонки id;isbn;title;year[;author], рядки з '#' – коментарі.
/// </summary>
public static class BookCsvImporter
{
    private const char Separator = ';';

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path, Encoding.UTF8);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;
            if (number == 1 && line.StartsWith("id", StringComparison.OrdinalIgnoreCase))
                continue;                       // рядок заголовків

            switch (ParseLine(line))
            {
                case ParseOk ok:
                    items.Add(ok.Value);
                    break;
                case ParseFailed failed:
                    errors.Add($"рядок {number}: {failed.Reason}");
                    break;
            }
        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static ParseOutcome ParseLine(string line)
    {
        string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

        return parts switch
        {
            { Length: < 4 } => new ParseFailed($"очікую 4-5 колонок, отримав {parts.Length}"),
            { Length: > 5 } => new ParseFailed($"занадто багато колонок: {parts.Length}"),
            ["", ..] or [_, "", ..] or [_, _, "", ..]
                => new ParseFailed("id, ISBN або назва порожні"),
            [_, _, _, var year, ..] when !FieldRules.IsInteger(year)
                => new ParseFailed($"рік '{year}' не є цілим числом"),
            [_, _, _, var year, ..] when !FieldRules.IsPlausibleYear(FieldRules.ParseInteger(year))
                => new ParseFailed($"рік {year} поза межами {FieldRules.MinYear}–{DateTime.Today.Year}"),
            [var id, var isbn, var title, var year]
                => new ParseOk(new BookDto(id, isbn, title, FieldRules.ParseInteger(year))),
            [var id, var isbn, var title, var year, var author]
                => new ParseOk(new BookDto(id, isbn, title, FieldRules.ParseInteger(year), FieldRules.NullIfEmpty(author))),
            _ => new ParseFailed($"нерозпізнаний формат рядка ({parts.Length} колонок)")
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(BookDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
