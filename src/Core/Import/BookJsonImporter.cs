using System.Text;
using System.Text.Json;
using Core.Dto;

namespace Core.Import;

/// <summary>
/// Імпорт книг з JSON (додаткове завдання 1) на тих самих типах BookDto.
/// Формат: UTF-8, масив об'єктів { "id", "isbn", "title", "year", "author"? }.
/// Кожен елемент розбирається окремо, тож один зламаний елемент не зупиняє весь імпорт.
/// </summary>
public static class BookJsonImporter
{
    private static readonly JsonSerializerOptions Options = new() { PropertyNameCaseInsensitive = true };

    public static ImportResult<BookDto> Load(string path)
    {
        var items = new List<BookDto>();
        var errors = new List<string>();

        string json = File.ReadAllText(path, Encoding.UTF8);

        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException ex)
        {
            errors.Add($"рядок {ex.LineNumber + 1}: файл не є коректним JSON");
            return new ImportResult<BookDto>(items, errors);
        }

        using (document)
        {
            if (document.RootElement.ValueKind != JsonValueKind.Array)
            {
                errors.Add("корінь JSON має бути масивом [ ... ]");
                return new ImportResult<BookDto>(items, errors);
            }

            int number = 0;
            foreach (JsonElement element in document.RootElement.EnumerateArray())
            {
                number++;
                switch (ParseElement(element))
                {
                    case ParseOk ok:
                        items.Add(ok.Value);
                        break;
                    case ParseFailed failed:
                        errors.Add($"елемент {number}: {failed.Reason}");
                        break;
                }
            }
        }

        return new ImportResult<BookDto>(items, errors);
    }

    private static ParseOutcome ParseElement(JsonElement element)
    {
        BookDto? book;
        try
        {
            book = element.Deserialize<BookDto>(Options);
        }
        catch (JsonException ex)
        {
            return new ParseFailed($"поле {ex.Path} має неправильний тип");
        }

        return book switch
        {
            null => new ParseFailed("елемент дорівнює null"),
            { Id: null or "" } or { Isbn: null or "" } or { Title: null or "" }
                => new ParseFailed("id, ISBN або назва порожні"),
            { Year: var year } when !FieldRules.IsPlausibleYear(year)
                => new ParseFailed($"рік {year} поза межами {FieldRules.MinYear}–{DateTime.Today.Year}"),
            _ => new ParseOk(book with { Author = FieldRules.NullIfEmpty(book.Author) })
        };
    }

    private abstract record ParseOutcome;
    private sealed record ParseOk(BookDto Value) : ParseOutcome;
    private sealed record ParseFailed(string Reason) : ParseOutcome;
}
