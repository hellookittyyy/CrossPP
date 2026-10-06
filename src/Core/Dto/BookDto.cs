namespace Core.Dto;

/// <summary>
/// Рядок файлу з книгою: id;isbn;title;year[;author].
/// Id, Isbn і Title обов'язкові, тому string без «?».
/// Author буває відсутнім (збірники, довідники, народні казки), тому string?.
/// </summary>
public record BookDto(
    string Id,
    string Isbn,
    string Title,
    int Year,
    string? Author = null);
