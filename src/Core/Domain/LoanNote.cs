using Core.Dto;

namespace Core.Domain;

public sealed class LoanNote
{
    public string Text { get; }
    public DateTime CreatedAt { get; }

    private LoanNote(string text, DateTime createdAt)
    {
        Text = text;
        CreatedAt = createdAt;
    }

    public static LoanNote Create(string text, DateTime createdAt)
    {
        if (string.IsNullOrWhiteSpace(text))
            throw new ArgumentException("Текст нотатки не може бути порожнім", nameof(text));

        return new LoanNote(text.Trim(), createdAt);
    }

    public LoanNoteDto ToDto() => new(Text, CreatedAt);

    public static LoanNote FromDto(LoanNoteDto dto) =>
        Create(dto.Text, dto.CreatedAt);

    public override string ToString() => $"[{CreatedAt:yyyy-MM-dd}] {Text}";
}
