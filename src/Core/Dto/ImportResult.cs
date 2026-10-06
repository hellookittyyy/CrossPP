namespace Core.Dto;

/// <summary>
/// Результат імпорту: успішно розібрані записи РАЗОМ із переліком помилок.
/// </summary>
public sealed record ImportResult<T>(IReadOnlyList<T> Items, IReadOnlyList<string> Errors);
