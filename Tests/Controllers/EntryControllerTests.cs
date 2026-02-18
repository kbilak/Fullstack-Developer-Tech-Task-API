using API.Controllers;
using Data;
using Data.Entities.Common;
using Data.Entities.Entry;
using Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers;

public class EntryControllerTests
{
    private (AppDbContext db, EntryController ctrl) Create()
    {
        var (db, _) = DbHelper.CreateContext();
        DbHelper.Seed(db);
        var repo = new EntryRepository(db);
        return (db, new EntryController(repo));
    }

    // #region GetAll

    [Fact]
    public async Task GetAll_Returns200()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetAll();

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<EntryListItemDTO>>(ok.Value);
        Assert.True(body.Status);
        Assert.Equal(6, body.TotalItems);
    }

    [Fact]
    public async Task GetAll_RespectsPageSize()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetAll(pageSize: 2);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<EntryListItemDTO>>(ok.Value);
        Assert.Equal(2, body.Items.Count);
    }

    // #endregion

    // #region GetByStore

    [Fact]
    public async Task GetByStore_Returns200()
    {
        var (db, ctrl) = Create();
        var warsawId = db.Stores.First(s => s.Name == "Store Warsaw").ID;

        var actionResult = await ctrl.GetByStore(warsawId);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<StoreEntryItemDTO>>(ok.Value);
        Assert.Equal(3, body.TotalItems);
    }

    // #endregion

    // #region GetByDate

    [Fact]
    public async Task GetByDate_Returns200()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetByDate(new DateTime(2026, 2, 3));

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<EntryListItemDTO>>(ok.Value);
        Assert.Equal(2, body.TotalItems); // Warsaw + Paris on Feb 3
    }

    // #endregion

    // #region GetByStoreAndDate

    [Fact]
    public async Task GetByStoreAndDate_Returns200()
    {
        var (db, ctrl) = Create();
        var warsawId = db.Stores.First(s => s.Name == "Store Warsaw").ID;

        var actionResult = await ctrl.GetByStoreAndDate(warsawId, new DateTime(2026, 2, 1));

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<StoreEntryItemDTO>>(ok.Value);
        Assert.Single(body.Items);
    }

    // #endregion

    // #region GetByDateRange

    [Fact]
    public async Task GetByDateRange_Returns200()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetByDateRange(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 2));

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<EntryListItemDTO>>(ok.Value);
        Assert.Equal(3, body.TotalItems);
    }

    [Fact]
    public async Task GetByDateRange_InvalidRange_ReturnsBadRequest()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetByDateRange(
            new DateTime(2026, 3, 1),
            new DateTime(2026, 2, 1));

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region GetStatistics

    [Fact]
    public async Task GetStatistics_Returns200()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetStatistics(
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<EntryStatisticsResponseDTO>(ok.Value);
        Assert.True(body.Status);
        Assert.True(body.DailyCounts.Count > 0);
        Assert.True(body.StoreCounts.Count > 0);
    }

    [Fact]
    public async Task GetStatistics_InvalidRange_ReturnsBadRequest()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetStatistics(
            new DateTime(2026, 3, 1),
            new DateTime(2026, 2, 1));

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region Create

    [Fact]
    public async Task Create_Returns201()
    {
        var (db, ctrl) = Create();
        var warsawId = db.Stores.First().ID;

        var actionResult = await ctrl.Create(new PostEntryCreateDTO
        {
            IDStore = warsawId,
            EntryDate = new DateTime(2026, 3, 1, 10, 0, 0)
        });

        var created = Assert.IsType<CreatedResult>(actionResult.Result);
        var body = Assert.IsType<PostEntryCreateResponseDTO>(created.Value);
        Assert.True(body.Status);
        Assert.True(body.ID > 0);
    }

    [Fact]
    public async Task Create_StoreNotFound_Returns404()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.Create(new PostEntryCreateDTO
        {
            IDStore = 99999,
            EntryDate = DateTime.UtcNow
        });

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region Update

    [Fact]
    public async Task Update_Returns200()
    {
        var (db, ctrl) = Create();
        var entryId = db.Entries.First().ID;

        var actionResult = await ctrl.Update(entryId, new PutEntryUpdateDTO
        {
            EntryDate = new DateTime(2026, 6, 1, 10, 0, 0)
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<ApiResponse>(ok.Value);
        Assert.True(body.Status);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.Update(99999, new PutEntryUpdateDTO
        {
            EntryDate = DateTime.UtcNow
        });

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region Delete

    [Fact]
    public async Task Delete_Returns200()
    {
        var (db, ctrl) = Create();
        var entryId = db.Entries.First().ID;

        var actionResult = await ctrl.Delete(entryId);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.True(((ApiResponse)ok.Value!).Status);
    }

    [Fact]
    public async Task Delete_NotFound_Returns404()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.Delete(99999);

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region DeleteBulk

    [Fact]
    public async Task DeleteBulk_Returns200()
    {
        var (db, ctrl) = Create();
        var ids = db.Entries.Take(2).Select(e => e.ID).ToList();

        var actionResult = await ctrl.DeleteBulk(ids);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        Assert.True(((ApiResponse)ok.Value!).Status);
    }

    [Fact]
    public async Task DeleteBulk_EmptyList_ReturnsBadRequest()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.DeleteBulk(new List<int>());

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    // #endregion
}
