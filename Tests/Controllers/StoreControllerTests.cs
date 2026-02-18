using API.Controllers;
using Data;
using Data.Entities.Common;
using Data.Entities.Store;
using Data.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Tests.Controllers;

public class StoreControllerTests
{
    private (AppDbContext db, StoreController ctrl) Create()
    {
        var (db, _) = DbHelper.CreateContext();
        DbHelper.Seed(db);
        var repo = new StoreRepository(db);
        return (db, new StoreController(repo));
    }

    // #region GetAll

    [Fact]
    public async Task GetAll_Returns200WithPaginatedStores()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetAll();

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<StoreItemDTO>>(ok.Value);
        Assert.True(body.Status);
        Assert.Equal(3, body.TotalItems);
    }

    [Fact]
    public async Task GetAll_WithSortAndSearch()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetAll(sort: "name:asc", search: "store");

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PaginatedResponse<StoreItemDTO>>(ok.Value);
        Assert.Equal(3, body.Items.Count);

        var names = body.Items.Select(s => s.Name).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    // #endregion

    // #region Get (by ID)

    [Fact]
    public async Task Get_Found_Returns200()
    {
        var (db, ctrl) = Create();
        var storeId = db.Stores.First().ID;

        var actionResult = await ctrl.Get(storeId);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<GetStoreResponseDTO>(ok.Value);
        Assert.True(body.Status);
    }

    [Fact]
    public async Task Get_NotFound_Returns404()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.Get(99999);

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region Create

    [Fact]
    public async Task Create_Returns201WithId()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.Create(new PostStoreCreateDTO
        {
            Name = "New Store",
            City = "London",
            Country = "UK"
        });

        var created = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
        var body = Assert.IsType<PostStoreCreateResponseDTO>(created.Value);
        Assert.True(body.Status);
        Assert.True(body.ID > 0);
    }

    // #endregion

    // #region Update

    [Fact]
    public async Task Update_Returns200()
    {
        var (db, ctrl) = Create();
        var storeId = db.Stores.First().ID;

        var actionResult = await ctrl.Update(storeId, new PutStoreUpdateDTO
        {
            Name = "Updated",
            City = "Updated",
            Country = "Updated"
        });

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<PostStoreCreateResponseDTO>(ok.Value);
        Assert.True(body.Status);
    }

    [Fact]
    public async Task Update_NotFound_Returns404()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.Update(99999, new PutStoreUpdateDTO
        {
            Name = "X",
            City = "X",
            Country = "X"
        });

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region Delete

    [Fact]
    public async Task Delete_Returns200()
    {
        var (db, ctrl) = Create();
        var storeId = db.Stores.First().ID;

        var actionResult = await ctrl.Delete(storeId);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<ApiResponse>(ok.Value);
        Assert.True(body.Status);
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
        var ids = db.Stores.Take(2).Select(s => s.ID).ToList();

        var actionResult = await ctrl.DeleteBulk(ids);

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<ApiResponse>(ok.Value);
        Assert.True(body.Status);
    }

    [Fact]
    public async Task DeleteBulk_EmptyList_ReturnsBadRequest()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.DeleteBulk(new List<int>());

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    // #endregion

    // #region GetStatistics

    [Fact]
    public async Task GetStatistics_Returns200()
    {
        var (db, ctrl) = Create();
        var storeId = db.Stores.First().ID;

        var actionResult = await ctrl.GetStatistics(
            storeId,
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        var ok = Assert.IsType<OkObjectResult>(actionResult.Result);
        var body = Assert.IsType<GetStoreStatisticsResponseDTO>(ok.Value);
        Assert.True(body.Status);
        Assert.True(body.Statistics.Count > 0);
    }

    [Fact]
    public async Task GetStatistics_InvalidDateRange_ReturnsBadRequest()
    {
        var (db, ctrl) = Create();
        var storeId = db.Stores.First().ID;

        var actionResult = await ctrl.GetStatistics(
            storeId,
            new DateTime(2026, 3, 1),
            new DateTime(2026, 2, 1)); // end < start

        Assert.IsType<BadRequestObjectResult>(actionResult.Result);
    }

    [Fact]
    public async Task GetStatistics_StoreNotFound_Returns404()
    {
        var (db, ctrl) = Create();
        var actionResult = await ctrl.GetStatistics(
            99999,
            new DateTime(2026, 2, 1),
            new DateTime(2026, 2, 7));

        Assert.IsType<NotFoundObjectResult>(actionResult.Result);
    }

    // #endregion
}
