namespace Core.Dto;

/// <summary>
/// Формат зберігання примірника: дані примірника (Id, Isbn, IsIssued)
/// і дані видання (Title, Author) в одному записі списку.
/// Title/Author необов'язкові — старі файли з трьох полів теж читаються.
/// </summary>
public record BookCopyDto(
    string Id,
    string Isbn,
    bool IsIssued,
    string? Title = null,
    string? Author = null);
