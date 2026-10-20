using Core;
using Core.Abstractions;
using Core.Storage;

namespace Cli;
public static class StoreFactory
{
    public static IBookStore Create(string[] args) =>
        args.Contains("--file")
            ? new FileBookStore(DataPath())
            : new InMemoryBookStore(SampleData.BookCopies());
    public static string DataPath()
    {
        string cwdData = Path.Combine(Directory.GetCurrentDirectory(), "data");
        string dataDir = Directory.Exists(cwdData)
            ? cwdData
            : Path.Combine(AppContext.BaseDirectory, "data");
        return Path.Combine(dataDir, "catalog.json");
    }
}
