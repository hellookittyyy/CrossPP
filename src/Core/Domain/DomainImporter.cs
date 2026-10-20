using Core.Dto;

namespace Core.Domain;

public static class DomainImporter
{
    public static ImportResult<BookCopy> ImportBookCopies(ImportResult<BookDto> importResult)
    {
        var items = new List<BookCopy>();
        var errors = new List<string>(importResult.Errors);

        foreach (var dto in importResult.Items)
        {
            try
            {
                var copy = BookCopy.Create(dto.Id, dto.Isbn, title: dto.Title, author: dto.Author);
                items.Add(copy);
            }
            catch (Exception ex)
            {
                errors.Add($"BookCopy '{dto.Id}': {ex.GetType().Name} — {ex.Message}");
            }
        }

        return new ImportResult<BookCopy>(items, errors);
    }
}
