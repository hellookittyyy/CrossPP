using System.Globalization;

namespace Core.Import;

/// <summary>
/// Спільні правила розбору полів для всіх імпортерів.
/// Числа завжди розбираються з InvariantCulture – результат не залежить від локалі ОС.
/// </summary>
internal static class FieldRules
{
    public const int MinYear = 1450;

    public static bool IsInteger(string text) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out _);
    public static int ParseInteger(string text) =>
        int.Parse(text, NumberStyles.None, CultureInfo.InvariantCulture);

    public static bool IsPlausibleYear(int year) =>
        year >= MinYear && year <= DateTime.Today.Year;

    public static string? NullIfEmpty(string? text) =>
        string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
