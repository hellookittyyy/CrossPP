using System.Globalization;

namespace Core.Import;

/// <summary>
/// Статистика імпорту (додаткове завдання 3) – заготовка під звіти тижня 7.
/// </summary>
public sealed record ImportStats(int Total, int Accepted, int Skipped)
{
    public double ErrorPercent => Total == 0 ? 0 : Skipped * 100.0 / Total;

    public override string ToString() => string.Create(
        CultureInfo.InvariantCulture,
        $"усього: {Total} / прийнято: {Accepted} / пропущено: {Skipped} / помилок: {ErrorPercent:F1}%");
}
