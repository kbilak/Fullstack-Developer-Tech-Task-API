using Data.Entities.Common;
using Data.Entities.Entry;
using Data.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

/// <summary>
/// Controller for managing store entries (visits).
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class EntryController : ControllerBase
{
    private readonly IEntryRepository _entryRepository;

    public EntryController(IEntryRepository entryRepository)
    {
        _entryRepository = entryRepository;
    }

    /// <summary>
    /// Gets all entries with pagination.
    /// </summary>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of entries with status.</returns>
    [HttpGet]
    public async Task<ActionResult<PaginatedResponse<EntryListItemDTO>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _entryRepository.GetAllEntriesAsync(pagination);
        return Ok(result);
    }

    /// <summary>
    /// Gets entries for a specific store with pagination.
    /// </summary>
    /// <param name="IDStore">Store ID.</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of store entries with status.</returns>
    [HttpGet]
    [Route("store/{IDStore}")]
    public async Task<ActionResult<PaginatedResponse<StoreEntryItemDTO>>> GetByStore(
        int IDStore,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _entryRepository.GetEntriesByStoreAsync(IDStore, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Gets entries for a specific store on a specific date with pagination.
    /// </summary>
    /// <param name="IDStore">Store ID.</param>
    /// <param name="date">Date to filter by (yyyy-MM-dd).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of store entries with status.</returns>
    [HttpGet]
    [Route("store/{IDStore}/date/{date}")]
    public async Task<ActionResult<PaginatedResponse<StoreEntryItemDTO>>> GetByStoreAndDate(
        int IDStore,
        DateTime date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _entryRepository.GetEntriesByStoreAndDateAsync(IDStore, date, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Gets entries for a specific date with pagination.
    /// </summary>
    /// <param name="date">Date to filter by (yyyy-MM-dd).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of entries with status.</returns>
    [HttpGet]
    [Route("date/{date}")]
    public async Task<ActionResult<PaginatedResponse<EntryListItemDTO>>> GetByDate(
        DateTime date,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _entryRepository.GetEntriesByDateAsync(date, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Gets entries within a date range with pagination.
    /// </summary>
    /// <param name="startDate">Start date of the range (yyyy-MM-dd).</param>
    /// <param name="endDate">End date of the range (yyyy-MM-dd).</param>
    /// <param name="page">Page number (default: 1).</param>
    /// <param name="pageSize">Number of items per page (default: 10, max: 50).</param>
    /// <returns>Paginated list of entries with status.</returns>
    [HttpGet]
    [Route("date")]
    public async Task<ActionResult<PaginatedResponse<EntryListItemDTO>>> GetByDateRange(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (startDate > endDate)
            return BadRequest(new ApiResponse
            {
                Status = false,
                Message = "startDate must be less than or equal to endDate"
            });

        var pagination = new PaginationParams { Page = page, PageSize = pageSize };
        var result = await _entryRepository.GetEntriesByDateRangeAsync(startDate, endDate, pagination);
        return Ok(result);
    }

    /// <summary>
    /// Adds a new entry (visit) to a store.
    /// </summary>
    /// <param name="newEntry">Entry data containing store ID and entry date.</param>
    /// <returns>Created entry ID with status.</returns>
    [HttpPost]
    public async Task<ActionResult<PostEntryCreateResponseDTO>> Create([FromBody] PostEntryCreateDTO newEntry)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _entryRepository.AddEntryAsync(newEntry);

        if (!result.Status)
            return NotFound(result);

        return Created(string.Empty, result);
    }
}
