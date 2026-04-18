using BankProfiles.Web.Data;
using BankProfiles.Web.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace BankProfiles.Web.Services;

public class EventStoreService : IEventStoreService
{
    private readonly IDbContextFactory<BankDbContext> _contextFactory;
    private readonly ILogger<EventStoreService> _logger;

    public EventStoreService(
        IDbContextFactory<BankDbContext> contextFactory,
        ILogger<EventStoreService> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    public async Task<MetricEvent> AppendEventAsync(MetricEvent evt)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();

        var nextSequence = await GetNextSequenceAsync(context, evt.BankCode);
        evt.EventSequence = nextSequence;
        evt.CreatedDate = DateTime.UtcNow;

        context.MetricEvents.Add(evt);
        await context.SaveChangesAsync();

        _logger.LogDebug("Appended event {EventId} for bank {BankCode}: {MetricName}",
            evt.EventId, evt.BankCode, evt.MetricName);

        return evt;
    }

    public async Task<List<MetricEvent>> AppendEventsAsync(List<MetricEvent> events)
    {
        if (events.Count == 0) return events;

        await using var context = await _contextFactory.CreateDbContextAsync();
        await using var transaction = await context.Database.BeginTransactionAsync();

        try
        {
            var bankCode = events[0].BankCode;
            var nextSequence = await GetNextSequenceAsync(context, bankCode);
            var now = DateTime.UtcNow;

            foreach (var evt in events)
            {
                evt.EventSequence = nextSequence++;
                evt.CreatedDate = now;
                context.MetricEvents.Add(evt);
            }

            await context.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation("Appended {Count} events for bank {BankCode}",
                events.Count, bankCode);

            return events;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<List<MetricEvent>> GetEventsForBankAsync(string bankCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MetricEvents
            .Where(e => e.BankCode == bankCode)
            .OrderBy(e => e.EventSequence)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<MetricEvent>> GetEventsByMetricAsync(string bankCode, string metricName)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MetricEvents
            .Where(e => e.BankCode == bankCode && e.MetricName == metricName)
            .OrderBy(e => e.EventSequence)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<MetricEvent>> GetEventsInRangeAsync(string bankCode, DateTime from, DateTime to)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MetricEvents
            .Where(e => e.BankCode == bankCode && e.CreatedDate >= from && e.CreatedDate <= to)
            .OrderBy(e => e.EventSequence)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<List<string>> GetAllBankCodesAsync()
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MetricEvents
            .Select(e => e.BankCode)
            .Distinct()
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<long> GetLatestSequenceAsync(string bankCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        var maxSeq = await context.MetricEvents
            .Where(e => e.BankCode == bankCode)
            .MaxAsync(e => (long?)e.EventSequence);
        return maxSeq ?? 0;
    }

    public async Task<bool> HasEventsAsync(string bankCode)
    {
        await using var context = await _contextFactory.CreateDbContextAsync();
        return await context.MetricEvents
            .AnyAsync(e => e.BankCode == bankCode);
    }

    private static async Task<long> GetNextSequenceAsync(BankDbContext context, string bankCode)
    {
        var maxSeq = await context.MetricEvents
            .Where(e => e.BankCode == bankCode)
            .MaxAsync(e => (long?)e.EventSequence);
        return (maxSeq ?? 0) + 1;
    }
}
