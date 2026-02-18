using Data;
using Data.Entities.Common;
using Data.Entities.Entry;
using Data.Repositories;

namespace Tests.Repositories;

public class EntryRepositoryTests
{
    private (AppDbContext db, EntryRepository repo) Create()
    {
        var (db, _) = DbHelper.CreateContext();
        DbHelper.Seed(db);
        return (db, new EntryRepository(db));
    }

    // #region GetAllEntriesAsync

    [Fact]
    public async Task GetAllEntries_ReturnsPaginated()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllEntriesAsync(new PaginationParams { Page = 1, PageSize = 10 });

        Assert.True(result.Status);
        Assert.Equal(6, result.TotalItems);
        Assert.Equal(6, result.Items.Count);
    }

    [Fact]
    public async Task GetAllEntries_RespectsPageSize()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllEntriesAsync(new PaginationParams { Page = 1, PageSize = 3 });

        Assert.Equal(3, result.Items.Count);
        Assert.Equal(6, result.TotalItems);
    }

    [Fact]
    public async Task GetAllEntries_IncludesStoreName()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllEntriesAsync(new PaginationParams { Page = 1, PageSize = 10 });

        Assert.True(result.Items.All(e => !string.IsNullOrEmpty(e.StoreName)));
    }

    [Fact]
    public async Task GetAllEntries_OrderedByDateDesc()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllEntriesAsync(new PaginationParams { Page = 1, PageSize = 10 });

        var dates = result.Items.Select(e => e.EntryDate).ToList();
        Assert.Equal(dates.OrderByDescending(d => d).ToList(), dates);
    }

    // #endregion

    // #region GetEntriesByStoreAsync

    [Fact]
    public async Task GetEntriesByStore_ReturnsOnlyMatchingStore()
    {
        var (db, repo) = Create();
        var warsawId = db.Stores.First(s => s.Name == "Store Warsaw").ID;

        var result = await repo.GetEntriesByStoreAsync(warsawId, new PaginationParams { Page = 1, PageSize = 10 });

        Assert.True(result.Status);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count);
    }

    [Fact]
    public async Task GetEntriesByStore_EmptyForNonExistentStore()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntriesByStoreAsync(99999, new PaginationParams { Page = 1, PageSize = 10 });

        Assert.Equal(0, result.TotalItems);
        Assert.Empty(result.Items);
    }

    // #endregion

    // #region GetEntriesByDateRangeAsync

    [Fact]
    public async Task GetEntriesByDateRange_FiltersCorrectly()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntriesByDateRangeAsync(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 2),
            new PaginationParams { Page = 1, PageSize = 50 });

        // Feb 1: Warsaw + Berlin = 2, Feb 2: Warsaw = 1 → total 3
        Assert.Equal(3, result.TotalItems);
        Assert.True(result.Items.All(e => e.EntryDate.Date >= new DateTime(2026, 2, 1) && e.EntryDate.Date <= new DateTime(2026, 2, 2)));
    }

    [Fact]
    public async Task GetEntriesByDateRange_EmptyForFuture()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntriesByDateRangeAsync(
            new DateTime(2099, 1, 1),
            new DateTime(2099, 12, 31),
            new PaginationParams { Page = 1, PageSize = 10 });

        Assert.Equal(0, result.TotalItems);
    }

    // #endregion

    // #region GetEntriesByDateAsync

    [Fact]
    public async Task GetEntriesByDate_ReturnsCorrectDay()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntriesByDateAsync(
            new DateTime(2026, 2, 3),
            new PaginationParams { Page = 1, PageSize = 10 });

        // Feb 3: Warsaw + Paris = 2
        Assert.Equal(2, result.TotalItems);
    }

    // #endregion

    // #region GetEntriesByStoreAndDateAsync

    [Fact]
    public async Task GetEntriesByStoreAndDate_FiltersCorrectly()
    {
        var (db, repo) = Create();
        var warsawId = db.Stores.First(s => s.Name == "Store Warsaw").ID;

        var result = await repo.GetEntriesByStoreAndDateAsync(
            warsawId,
            new DateTime(2026, 2, 1),
            new PaginationParams { Page = 1, PageSize = 10 });

        Assert.Single(result.Items);
    }

    // #endregion

    // #region AddEntryAsync

    [Fact]
    public async Task AddEntry_Success()
    {
        var (db, repo) = Create();
        var warsawId = db.Stores.First(s => s.Name == "Store Warsaw").ID;

        var result = await repo.AddEntryAsync(new PostEntryCreateDTO
        {
            IDStore = warsawId,
            EntryDate = new DateTime(2026, 3, 1, 10, 0, 0)
        });

        Assert.True(result.Status);
        Assert.True(result.ID > 0);

        // Verify count increased
        var entries = await repo.GetEntriesByStoreAsync(warsawId, new PaginationParams { Page = 1, PageSize = 50 });
        Assert.Equal(4, entries.TotalItems);
    }

    [Fact]
    public async Task AddEntry_StoreNotFound_Fails()
    {
        var (db, repo) = Create();
        var result = await repo.AddEntryAsync(new PostEntryCreateDTO
        {
            IDStore = 99999,
            EntryDate = DateTime.UtcNow
        });

        Assert.False(result.Status);
        Assert.Equal("Store not found", result.Message);
    }

    // #endregion

    // #region UpdateEntryAsync

    [Fact]
    public async Task UpdateEntry_Success()
    {
        var (db, repo) = Create();
        var entry = db.Entries.First();
        var newDate = new DateTime(2026, 6, 15, 10, 0, 0);

        var result = await repo.UpdateEntryAsync(entry.ID, new PutEntryUpdateDTO { EntryDate = newDate });

        Assert.True(result.Status);

        var updated = db.Entries.Find(entry.ID);
        Assert.Equal(newDate, updated!.EntryDate);
    }

    [Fact]
    public async Task UpdateEntry_NotFound()
    {
        var (db, repo) = Create();
        var result = await repo.UpdateEntryAsync(99999, new PutEntryUpdateDTO { EntryDate = DateTime.UtcNow });

        Assert.False(result.Status);
    }

    // #endregion

    // #region DeleteEntryAsync

    [Fact]
    public async Task DeleteEntry_Success()
    {
        var (db, repo) = Create();
        var entry = db.Entries.First();

        var result = await repo.DeleteEntryAsync(entry.ID);
        Assert.True(result.Status);

        var all = await repo.GetAllEntriesAsync(new PaginationParams { Page = 1, PageSize = 50 });
        Assert.Equal(5, all.TotalItems);
    }

    [Fact]
    public async Task DeleteEntry_NotFound()
    {
        var (db, repo) = Create();
        var result = await repo.DeleteEntryAsync(99999);
        Assert.False(result.Status);
    }

    // #endregion

    // #region DeleteEntriesAsync (bulk)

    [Fact]
    public async Task DeleteEntriesBulk_RemovesMultiple()
    {
        var (db, repo) = Create();
        var ids = db.Entries.Take(3).Select(e => e.ID).ToList();

        var result = await repo.DeleteEntriesAsync(ids);
        Assert.True(result.Status);

        var all = await repo.GetAllEntriesAsync(new PaginationParams { Page = 1, PageSize = 50 });
        Assert.Equal(3, all.TotalItems);
    }

    [Fact]
    public async Task DeleteEntriesBulk_NoMatch_ReturnsFalse()
    {
        var (db, repo) = Create();
        var result = await repo.DeleteEntriesAsync(new List<int> { 99999 });
        Assert.False(result.Status);
    }

    // #endregion

    // #region GetEntryStatisticsAsync

    [Fact]
    public async Task GetEntryStatistics_ReturnsDailyAndStoreCounts()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntryStatisticsAsync(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        Assert.True(result.Status);
        Assert.True(result.DailyCounts.Count > 0);
        Assert.True(result.StoreCounts.Count > 0);
    }

    [Fact]
    public async Task GetEntryStatistics_DailyCountsSortedByDate()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntryStatisticsAsync(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        var dates = result.DailyCounts.Select(d => d.Date).ToList();
        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }

    [Fact]
    public async Task GetEntryStatistics_StoreCountsSortedByCountDesc()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntryStatisticsAsync(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        var counts = result.StoreCounts.Select(s => s.Count).ToList();
        Assert.Equal(counts.OrderByDescending(c => c).ToList(), counts);
    }

    [Fact]
    public async Task GetEntryStatistics_StoreCountsIncludeStoreName()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntryStatisticsAsync(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        Assert.True(result.StoreCounts.All(s => !string.IsNullOrEmpty(s.StoreName)));
    }

    [Fact]
    public async Task GetEntryStatistics_EmptyForFuture()
    {
        var (db, repo) = Create();
        var result = await repo.GetEntryStatisticsAsync(
            new DateTime(2099, 1, 1),
            new DateTime(2099, 12, 31));

        Assert.Empty(result.DailyCounts);
        Assert.Empty(result.StoreCounts);
    }

    // #endregion
}
