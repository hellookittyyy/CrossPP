namespace Core.Dto;

/// <summary>
/// Рядок файлу з читачем (для додаткового завдання 2): id;fullName;email[;phone].
/// Без Id, ПІБ та email читача не можна ідентифікувати й повідомити, тому string.
/// Телефон читач може не залишити, тому string?.
/// </summary>
public record ReaderDto(
    string Id,
    string FullName,
    string Email,
    string? Phone = null);
