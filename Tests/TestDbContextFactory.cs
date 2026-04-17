using BankProfiles.Web.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

namespace BankProfiles.Tests;

public static class TestDbContextFactory
{
    public static IDbContextFactory<BankDbContext> Create(string? dbName = null)
    {
        dbName ??= Guid.NewGuid().ToString();
        var options = new DbContextOptionsBuilder<BankDbContext>()
            .UseInMemoryDatabase(dbName)
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        return new InMemoryDbContextFactory(options);
    }

    public static ILogger<T> CreateLogger<T>()
    {
        return LoggerFactory.Create(b => b.AddDebug()).CreateLogger<T>();
    }

    private class InMemoryDbContextFactory : IDbContextFactory<BankDbContext>
    {
        private readonly DbContextOptions<BankDbContext> _options;
        public InMemoryDbContextFactory(DbContextOptions<BankDbContext> options) => _options = options;
        public BankDbContext CreateDbContext() => new(_options);
    }
}
