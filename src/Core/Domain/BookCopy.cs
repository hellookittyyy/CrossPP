using Core.Dto;

namespace Core.Domain;

public sealed class BookCopy
{
    public string Id { get; }
    public string Isbn { get; }
    public string? Title { get; }
    public string? Author { get; }
    public bool IsIssued { get; private set; }

    private BookCopy(string id, string isbn, string? title, string? author, bool isIssued)
    {
        Id = id;
        Isbn = isbn;
        Title = title;
        Author = author;
        IsIssued = isIssued;
    }

    /// <summary>
    /// Title та Author — дані видання; необов'язкові, бо примірник може бути
    /// заведений лише за ISBN (лабораторна 4). Порожній рядок вважається відсутнім значенням.
    /// </summary>
    public static BookCopy Create(string id, string isbn, bool isIssued = false,
                                  string? title = null, string? author = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(isbn))
            throw new ArgumentException("ISBN не може бути порожнім", nameof(isbn));

        return new BookCopy(id.Trim(), isbn.Trim().ToUpperInvariant(),
                            Normalize(title), Normalize(author), isIssued);
    }

    public void Issue()
    {
        if (IsIssued)
            throw new InvalidOperationException(
                $"Примірник {Id} вже виданий, повторна видача неможлива");

        IsIssued = true;
    }

    public void Return()
    {
        if (!IsIssued)
            throw new InvalidOperationException(
                $"Примірник {Id} не виданий, повернення неможливе");

        IsIssued = false;
    }

    public BookCopyDto ToDto() => new(Id, Isbn, IsIssued, Title, Author);

    public static BookCopy FromDto(BookCopyDto dto) =>
        Create(dto.Id, dto.Isbn, dto.IsIssued, dto.Title, dto.Author);

    public override string ToString()
    {
        string edition = Title is null ? "" : $" «{Title}»{(Author is null ? "" : $", {Author}")}";
        return $"{Id} [{Isbn}]{edition} — {(IsIssued ? "видано" : "в наявності")}";
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
