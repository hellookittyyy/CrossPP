# CrossApp
Наскрізний проєкт з крос-платформного програмування.

**Предметна область:** Бібліотека. 
**Сутності:** Book, BookCopy, Reader, Loan.
**Призначення:** облік видач примірників книг читачам 

## Запуск
```bash
dotnet build
dotnet run --project src/Cli 
dotnet run --project src/Cli -- --json
```
**Середовище:** .NET SDK 10.0, macOS
## Додаткове завдання
Розмір каталогу publish для osx-arm64: 83 MB
Розмір каталогу publish для win-x64: 77 MB