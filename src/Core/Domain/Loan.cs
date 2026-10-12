using Core.Dto;

namespace Core.Domain;

public sealed class Loan
{
    private readonly List<LoanNote> _notes = [];

    public string Id { get; }
    public string CopyId { get; }
    public string ReaderId { get; }
    public DateTime IssuedOn { get; }
    public DateTime? ReturnedOn { get; private set; }
    public LoanStatus Status { get; private set; }
    public IReadOnlyList<LoanNote> Notes => _notes.AsReadOnly();

    private Loan(string id, string copyId, string readerId,
                 DateTime issuedOn, DateTime? returnedOn,
                 LoanStatus status, List<LoanNote> notes)
    {
        Id = id;
        CopyId = copyId;
        ReaderId = readerId;
        IssuedOn = issuedOn;
        ReturnedOn = returnedOn;
        Status = status;
        _notes = notes;
    }

    public static Loan Open(string id, string copyId, string readerId, DateTime issuedOn)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("Ідентифікатор видачі обов'язковий", nameof(id));
        if (string.IsNullOrWhiteSpace(copyId))
            throw new ArgumentException("Ідентифікатор примірника обов'язковий", nameof(copyId));
        if (string.IsNullOrWhiteSpace(readerId))
            throw new ArgumentException("Ідентифікатор читача обов'язковий", nameof(readerId));

        return new Loan(id.Trim(), copyId.Trim(), readerId.Trim(),
                        issuedOn, returnedOn: null,
                        LoanStatus.Open, []);
    }

    public void Close(DateTime returnedOn)
    {
        _ = Status switch
        {
            LoanStatus.Open => true,
            LoanStatus.Closed => throw new InvalidOperationException(
                $"Видачу {Id} вже закрито, повторне закриття неможливе"),
            _ => throw new InvalidOperationException(
                $"Невідомий стан видачі {Id}: {Status}")
        };

        if (returnedOn < IssuedOn)
            throw new ArgumentOutOfRangeException(nameof(returnedOn), returnedOn,
                $"Дата повернення ({returnedOn:yyyy-MM-dd}) не може бути раніше " +
                $"дати видачі ({IssuedOn:yyyy-MM-dd}) для видачі {Id}");

        ReturnedOn = returnedOn;
        Status = LoanStatus.Closed;
    }

    public void AddNote(string text, DateTime createdAt)
    {
        if (Status != LoanStatus.Open)
            throw new InvalidOperationException(
                $"Видачу {Id} вже закрито, додавання нотаток неможливе");

        _notes.Add(LoanNote.Create(text, createdAt));
    }

    public LoanDto ToDto() => new(
        Id, CopyId, ReaderId, IssuedOn, ReturnedOn,
        Status.ToString(),
        _notes.Select(n => n.ToDto()).ToList());

    public static Loan FromDto(LoanDto dto)
    {
        if (!Enum.TryParse<LoanStatus>(dto.Status, ignoreCase: true, out var status))
            throw new ArgumentException(
                $"Невідомий статус видачі: '{dto.Status}'", nameof(dto));

        var notes = dto.Notes.Select(LoanNote.FromDto).ToList();

        var loan = Open(dto.Id, dto.CopyId, dto.ReaderId, dto.IssuedOn);

        if (status == LoanStatus.Closed)
        {
            if (dto.ReturnedOn is null)
                throw new ArgumentException(
                    "Закрита видача повинна мати дату повернення", nameof(dto));
            loan.Close(dto.ReturnedOn.Value);
        }

        foreach (var note in notes)
            loan._notes.Add(note);

        return loan;
    }

    public override string ToString()
    {
        string returned = ReturnedOn.HasValue
            ? $" → повернено {ReturnedOn.Value:yyyy-MM-dd}"
            : "";
        return $"Видача {Id}: примірник {CopyId} → читач {ReaderId}, " +
               $"{IssuedOn:yyyy-MM-dd}{returned} [{Status}] " +
               $"(нотаток: {_notes.Count})";
    }
}
