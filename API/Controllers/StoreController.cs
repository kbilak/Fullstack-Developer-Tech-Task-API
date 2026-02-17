using Data.Entities.Common;
using Data.Entities.Store;
using Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for managing stores.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class StoreController : ControllerBase
{
    private readonly IStoreRepository _storeRepository;

    public StoreController(IStoreRepository storeRepository)
    {
        _storeRepository = storeRepository;
    }

    /// <summary>
    /// Creates a new store.
    /// </summary>
    /// <param name="newStore">Store data containing name, city and country.</param>
    /// <returns>Created store ID with status.</returns>
    [HttpPost]
    public async Task<ActionResult<PostStoreCreateResponseDTO>> Create([FromBody] PostStoreCreateDTO newStore)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _storeRepository.CreateStoreAsync(newStore);
        return CreatedAtAction(nameof(Get), new { ID = result.ID }, result);
    }

    /// <summary>
    /// Gets all stores with pagination, sorting and search.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <param name="sort">Sort expression as "field:direction", e.g. "name:asc" or "id:desc". Default: "id:asc".</param>
    /// <param name="search">Case-insensitive search query applied to store name.</param>
    /// <returns>Paginated list of stores with status.</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<StoreItemDTO>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? sort = null,
        [FromQuery] string? search = null)
    {
        var pagination = new PaginationParams
        {
            Page = page,
            PageSize = pageSize,
            Sort = sort,
            Search = search
        };
        var result = await _storeRepository.GetAllStoresAsync(pagination);
        return Ok(result);
    }

    /// <summary>
    /// Gets a store by ID.
    /// </summary>
    /// <param name="ID">Store ID.</param>
    /// <returns>Store data with status.</returns>
    [HttpGet]
    [Route("{ID}")]
    public async Task<ActionResult<GetStoreResponseDTO>> Get(int ID)
    {
        var result = await _storeRepository.GetStoreByIDAsync(ID);

        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Updates an existing store.
    /// </summary>
    /// <param name="ID">Store ID.</param>
    /// <param name="storeData">Updated store data containing name, city and country.</param>
    /// <returns>Updated store ID with status.</returns>
    [HttpPut]
    [Route("{ID}")]
    public async Task<ActionResult<PostStoreCreateResponseDTO>> Update(int ID, [FromBody] PutStoreUpdateDTO storeData)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _storeRepository.UpdateStoreAsync(ID, storeData);

        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Deletes a store by ID.
    /// </summary>
    /// <param name="ID">Store ID.</param>
    /// <returns>Operation status.</returns>
    [HttpDelete]
    [Route("{ID}")]
    public async Task<ActionResult<ApiResponse>> Delete(int ID)
    {
        var result = await _storeRepository.DeleteStoreAsync(ID);

        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Deletes multiple stores by IDs.
    /// </summary>
    /// <param name="ids">List of Store IDs to delete.</param>
    /// <returns>Operation status.</returns>
    [HttpDelete]
    [Route("bulk")]
    public async Task<ActionResult<ApiResponse>> DeleteBulk([FromBody] List<int> ids)
    {
        if (ids == null || ids.Count == 0)
            return BadRequest(new ApiResponse { Status = false, Message = "No IDs provided" });

        var result = await _storeRepository.DeleteStoresAsync(ids);

        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }

    /// <summary>
    /// Gets visit statistics for a store within a date range.
    /// </summary>
    /// <param name="IDStore">Store ID.</param>
    /// <param name="startDate">Start date of the range (yyyy-MM-dd).</param>
    /// <param name="endDate">End date of the range (yyyy-MM-dd).</param>
    /// <returns>Store data with daily visit statistics.</returns>
    [HttpGet]
    [Route("statistics/{IDStore}")]
    public async Task<ActionResult<GetStoreStatisticsResponseDTO>> GetStatistics(
        int IDStore,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(new ApiResponse
            {
                Status = false,
                Message = "startDate must be less than or equal to endDate"
            });
        }

        var result = await _storeRepository.GetStatisticsAsync(IDStore, startDate, endDate);

        if (!result.Status)
            return NotFound(result);

        return Ok(result);
    }
}
