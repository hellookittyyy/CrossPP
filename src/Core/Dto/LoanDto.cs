namespace Core.Dto;

public record LoanDto(
    string Id,
    string CopyId,
    string ReaderId,
    DateTime IssuedOn,
    DateTime? ReturnedOn,
    string Status,
    IReadOnlyList<LoanNoteDto> Notes);
