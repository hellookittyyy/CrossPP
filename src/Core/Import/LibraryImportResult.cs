using Core.Dto;

namespace Core.Import;

/// <summary>
/// Результат імпорту різнорідного файлу (додаткове завдання 2):
/// рядки "B;..." – книги, "R;..." – читачі.
/// </summary>
public sealed record LibraryImportResult(
    IReadOnlyList<BookDto> Books,
    IReadOnlyList<ReaderDto> Readers,
    IReadOnlyList<string> Errors)
{
    public ImportStats Stats => new(
        Books.Count + Readers.Count + Errors.Count,
        Books.Count + Readers.Count,
        Errors.Count);
}
