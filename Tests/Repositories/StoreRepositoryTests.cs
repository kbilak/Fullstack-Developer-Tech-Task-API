using Data;
using Data.Entities.Common;
using Data.Entities.Store;
using Data.Repositories;

namespace Tests.Repositories;

public class StoreRepositoryTests
{
    private (AppDbContext db, StoreRepository repo) Create()
    {
        var (db, _) = DbHelper.CreateContext();
        DbHelper.Seed(db);
        return (db, new StoreRepository(db));
    }

    // #region GetAllStoresAsync

    [Fact]
    public async Task GetAllStores_ReturnsPaginatedResults()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });

        Assert.True(result.Status);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(3, result.Items.Count);
        Assert.Equal(1, result.Page);
    }

    [Fact]
    public async Task GetAllStores_RespectsPageSize()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 2 });

        Assert.Equal(2, result.Items.Count);
        Assert.Equal(3, result.TotalItems);
        Assert.Equal(2, result.TotalPages);
    }

    [Fact]
    public async Task GetAllStores_ReturnsPage2()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 2, PageSize = 2 });

        Assert.Single(result.Items);
        Assert.Equal(2, result.Page);
    }

    [Fact]
    public async Task GetAllStores_IncludesEntryCount()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });

        // Warsaw has 3 entries
        var warsaw = result.Items.First(s => s.Name == "Store Warsaw");
        Assert.Equal(3, warsaw.EntryCount);
    }

    [Fact]
    public async Task GetAllStores_SortsByNameAsc()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Sort = "name:asc" });

        var names = result.Items.Select(s => s.Name).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task GetAllStores_SortsByNameDesc()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Sort = "name:desc" });

        var names = result.Items.Select(s => s.Name).ToList();
        Assert.Equal(names.OrderByDescending(n => n).ToList(), names);
    }

    [Fact]
    public async Task GetAllStores_SortsByEntriesDesc()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Sort = "entries:desc" });

        var counts = result.Items.Select(s => s.EntryCount).ToList();
        Assert.Equal(counts.OrderByDescending(c => c).ToList(), counts);
    }

    [Fact]
    public async Task GetAllStores_SearchByName()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Search = "berlin" });

        Assert.Single(result.Items);
        Assert.Equal("Store Berlin", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAllStores_SearchIsCaseInsensitive()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Search = "PARIS" });

        Assert.Single(result.Items);
        Assert.Equal("Store Paris", result.Items[0].Name);
    }

    [Fact]
    public async Task GetAllStores_SearchNoMatch_ReturnsEmpty()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Search = "nonexistent" });

        Assert.Empty(result.Items);
        Assert.Equal(0, result.TotalItems);
    }

    [Fact]
    public async Task GetAllStores_CombinedSortAndSearch()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10, Sort = "name:asc", Search = "store" });

        Assert.Equal(3, result.Items.Count);
        var names = result.Items.Select(s => s.Name).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Fact]
    public async Task GetAllStores_DefaultSortIsIdAsc()
    {
        var (db, repo) = Create();
        var result = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });

        var ids = result.Items.Select(s => s.ID).ToList();
        Assert.Equal(ids.OrderBy(id => id).ToList(), ids);
    }

    // #endregion

    // #region GetStoreByIDAsync

    [Fact]
    public async Task GetStoreById_Found()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var storeId = allStores.Items[0].ID;

        var result = await repo.GetStoreByIDAsync(storeId);

        Assert.True(result.Status);
        Assert.Equal("Store Warsaw", result.Name);
        Assert.Equal("Warsaw", result.City);
        Assert.Equal("Poland", result.Country);
    }

    [Fact]
    public async Task GetStoreById_NotFound()
    {
        var (db, repo) = Create();
        var result = await repo.GetStoreByIDAsync(99999);

        Assert.False(result.Status);
        Assert.Equal("Store not found", result.Message);
    }

    // #endregion

    // #region CreateStoreAsync

    [Fact]
    public async Task CreateStore_Success()
    {
        var (db, repo) = Create();
        var result = await repo.CreateStoreAsync(new PostStoreCreateDTO
        {
            Name = "Store London",
            City = "London",
            Country = "United Kingdom"
        });

        Assert.True(result.Status);
        Assert.True(result.ID > 0);

        // Verify in DB
        var stored = await repo.GetStoreByIDAsync(result.ID);
        Assert.Equal("Store London", stored.Name);
    }

    // #endregion

    // #region UpdateStoreAsync

    [Fact]
    public async Task UpdateStore_Success()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var storeId = allStores.Items[0].ID;

        var result = await repo.UpdateStoreAsync(storeId, new PutStoreUpdateDTO
        {
            Name = "Updated Name",
            City = "Updated City",
            Country = "Updated Country"
        });

        Assert.True(result.Status);
        Assert.Equal(storeId, result.ID);

        var updated = await repo.GetStoreByIDAsync(storeId);
        Assert.Equal("Updated Name", updated.Name);
    }

    [Fact]
    public async Task UpdateStore_NotFound()
    {
        var (db, repo) = Create();
        var result = await repo.UpdateStoreAsync(99999, new PutStoreUpdateDTO
        {
            Name = "X",
            City = "X",
            Country = "X"
        });

        Assert.False(result.Status);
    }

    // #endregion

    // #region DeleteStoreAsync

    [Fact]
    public async Task DeleteStore_Success()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var storeId = allStores.Items[0].ID;

        var result = await repo.DeleteStoreAsync(storeId);
        Assert.True(result.Status);

        var after = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        Assert.Equal(2, after.TotalItems);
    }

    [Fact]
    public async Task DeleteStore_NotFound()
    {
        var (db, repo) = Create();
        var result = await repo.DeleteStoreAsync(99999);
        Assert.False(result.Status);
    }

    // #endregion

    // #region DeleteStoresAsync (bulk)

    [Fact]
    public async Task DeleteStoresBulk_RemovesMultiple()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var ids = allStores.Items.Take(2).Select(s => s.ID).ToList();

        var result = await repo.DeleteStoresAsync(ids);
        Assert.True(result.Status);

        var after = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        Assert.Equal(1, after.TotalItems);
    }

    [Fact]
    public async Task DeleteStoresBulk_NoMatch_ReturnsFalse()
    {
        var (db, repo) = Create();
        var result = await repo.DeleteStoresAsync(new List<int> { 99999 });
        Assert.False(result.Status);
    }

    // #endregion

    // #region GetStatisticsAsync

    [Fact]
    public async Task GetStatistics_ReturnsDaily()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var warsawId = allStores.Items.First(s => s.Name == "Store Warsaw").ID;

        var result = await repo.GetStatisticsAsync(
            warsawId,
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        Assert.True(result.Status);
        Assert.Equal("Store Warsaw", result.Name);
        Assert.Equal(3, result.Statistics.Count); // 3 different days
        Assert.True(result.Statistics.All(s => s.Count > 0));
    }

    [Fact]
    public async Task GetStatistics_DatesAreSorted()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var warsawId = allStores.Items.First(s => s.Name == "Store Warsaw").ID;

        var result = await repo.GetStatisticsAsync(warsawId, new DateTime(2026, 1, 1), new DateTime(2026, 3, 1));

        var dates = result.Statistics.Select(s => s.Date).ToList();
        Assert.Equal(dates.OrderBy(d => d).ToList(), dates);
    }

    [Fact]
    public async Task GetStatistics_StoreNotFound()
    {
        var (db, repo) = Create();
        var result = await repo.GetStatisticsAsync(99999, new DateTime(2026, 1, 1), new DateTime(2026, 3, 1));

        Assert.False(result.Status);
        Assert.Equal("Store not found", result.Message);
    }

    [Fact]
    public async Task GetStatistics_ReturnsSuccessForAnyDateRange()
    {
        var (db, repo) = Create();
        var allStores = await repo.GetAllStoresAsync(new PaginationParams { Page = 1, PageSize = 10 });
        var warsawId = allStores.Items.First(s => s.Name == "Store Warsaw").ID;

        var result = await repo.GetStatisticsAsync(warsawId, new DateTime(2026, 2, 1), new DateTime(2026, 2, 1));

        Assert.True(result.Status);
        Assert.Equal("Store Warsaw", result.Name);
        Assert.True(result.Statistics.All(s => s.Count > 0));
    }

    // #endregion
}
