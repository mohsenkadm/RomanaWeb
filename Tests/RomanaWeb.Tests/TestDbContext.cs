using Microsoft.EntityFrameworkCore;
using RomanaWeb.Model;

namespace RomanaWeb.Tests;

internal static class TestDbContext
{
    /// <summary>Builds a fresh in-memory DB_Context per test (uniqueDb=true ⇒ new GUID name).</summary>
    public static DB_Context New(string? name = null)
    {
        var options = new DbContextOptionsBuilder<DB_Context>()
            .UseInMemoryDatabase(name ?? Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new DB_Context(options);
    }
}
