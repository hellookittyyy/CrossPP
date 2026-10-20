using Core.Abstractions;
using Core.Domain;

namespace Core.Storage;
public sealed class CachingBookStore(IBookStore inner) : IBookStore
{
    private readonly IBookStore _inner = inner
        ?? throw new ArgumentNullException(nameof(inner));

    private Dictionary<string, BookCopy>? _cache;

    private void InvalidateCache() => _cache = null;

    private Dictionary<string, BookCopy> EnsureCache()
    {
        if (_cache is not null) return _cache;
        _cache = _inner.List().ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);
        return _cache;
    }

    public IReadOnlyList<BookCopy> List() => EnsureCache().Values.ToList();

    public BookCopy? GetById(string id) => EnsureCache().GetValueOrDefault(id);

    public void Add(BookCopy item)
    {
        _inner.Add(item);
        InvalidateCache();
    }

    public void Update(BookCopy item)
    {
        _inner.Update(item);
        InvalidateCache();
    }

    public bool Remove(string id)
    {
        bool removed = _inner.Remove(id);
        if (removed) InvalidateCache();
        return removed;
    }
}
