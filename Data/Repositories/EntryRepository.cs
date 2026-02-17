using Data.Entities.Common;
using Data.Entities.Entry;
using Data.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class EntryRepository : IEntryRepository
{
    private readonly AppDbContext _db;

    public EntryRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Creates a new entry for a store.
    /// </summary>
    /// <param name="newEntry">Entry data to create.</param>
    /// <returns>New entry ID.</returns>
    public async Task<PostEntryCreateResponseDTO> AddEntryAsync(PostEntryCreateDTO newEntry)
    {
        var storeExists = await _db.Stores.AnyAsync(s => s.ID == newEntry.IDStore);

        if (!storeExists)
            return new PostEntryCreateResponseDTO
            {
                ID = 0,
                Status = false,
                Message = "Store not found"
            };

        var entry = new Entry
        {
            IDStore = newEntry.IDStore,
            EntryDate = newEntry.EntryDate
        };

        await _db.Entries.AddAsync(entry);
        await _db.SaveChangesAsync();

        return new PostEntryCreateResponseDTO
        {
            ID = entry.ID,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets all entries with pagination.
    /// </summary>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paginated list of entries.</returns>
    public async Task<PaginatedResponse<EntryListItemDTO>> GetAllEntriesAsync(PaginationParams pagination)
    {
        var totalItems = await _db.Entries.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        var entries = await _db.Entries
            .OrderByDescending(e => e.EntryDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new EntryListItemDTO
            {
                ID = e.ID,
                IDStore = e.IDStore,
                EntryDate = e.EntryDate,
                StoreName = e.Store.Name
            })
            .ToListAsync();

        return new PaginatedResponse<EntryListItemDTO>
        {
            Items = entries,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets entries by store ID with pagination.
    /// </summary>
    /// <param name="IDStore">Store ID.</param>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paginated list of Store Entries.</returns>
    public async Task<PaginatedResponse<StoreEntryItemDTO>> GetEntriesByStoreAsync(int IDStore, PaginationParams pagination)
    {
        var query = _db.Entries.Where(e => e.IDStore == IDStore);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        var entries = await query
            .OrderByDescending(e => e.EntryDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new StoreEntryItemDTO
            {
                ID = e.ID,
                EntryDate = e.EntryDate
            })
            .ToListAsync();

        return new PaginatedResponse<StoreEntryItemDTO>
        {
            Items = entries,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets entries within a date range with pagination.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paginated list of Entries.</returns>
    public async Task<PaginatedResponse<EntryListItemDTO>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination)
    {
        var query = _db.Entries.Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        var entries = await query
            .OrderByDescending(e => e.EntryDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new EntryListItemDTO
            {
                ID = e.ID,
                IDStore = e.IDStore,
                EntryDate = e.EntryDate,
                StoreName = e.Store.Name
            })
            .ToListAsync();

        return new PaginatedResponse<EntryListItemDTO>
        {
            Items = entries,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets entries for a specific date with pagination.
    /// </summary>
    /// <param name="date">Date to filter by.</param>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paginated list of Entries.</returns>
    public async Task<PaginatedResponse<EntryListItemDTO>> GetEntriesByDateAsync(DateTime date, PaginationParams pagination)
    {
        var query = _db.Entries.Where(e => e.EntryDate.Date == date.Date);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        var entries = await query
            .OrderByDescending(e => e.EntryDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new EntryListItemDTO
            {
                ID = e.ID,
                IDStore = e.IDStore,
                EntryDate = e.EntryDate,
                StoreName = e.Store.Name
            })
            .ToListAsync();

        return new PaginatedResponse<EntryListItemDTO>
        {
            Items = entries,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets entries by store ID and date with pagination.
    /// </summary>
    /// <param name="IDStore">Store ID.</param>
    /// <param name="date">Date to filter by.</param>
    /// <param name="pagination">Pagination parameters.</param>
    /// <returns>Paginated list of Store Entries.</returns>
    public async Task<PaginatedResponse<StoreEntryItemDTO>> GetEntriesByStoreAndDateAsync(int IDStore, DateTime date, PaginationParams pagination)
    {
        var query = _db.Entries.Where(e => e.IDStore == IDStore && e.EntryDate.Date == date.Date);

        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        var entries = await query
            .OrderByDescending(e => e.EntryDate)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(e => new StoreEntryItemDTO
            {
                ID = e.ID,
                EntryDate = e.EntryDate
            })
            .ToListAsync();

        return new PaginatedResponse<StoreEntryItemDTO>
        {
            Items = entries,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Updates an existing entry's date.
    /// </summary>
    /// <param name="ID">Entry ID.</param>
    /// <param name="entryData">Updated entry data.</param>
    /// <returns>Operation status.</returns>
    public async Task<ApiResponse> UpdateEntryAsync(int ID, PutEntryUpdateDTO entryData)
    {
        var entry = await _db.Entries.FindAsync(ID);

        if (entry == null)
            return new ApiResponse
            {
                Status = false,
                Message = "Entry not found"
            };

        entry.EntryDate = entryData.EntryDate;
        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Deletes a single entry by ID.
    /// </summary>
    /// <param name="ID">Entry ID.</param>
    /// <returns>Operation status.</returns>
    public async Task<ApiResponse> DeleteEntryAsync(int ID)
    {
        var entry = await _db.Entries.FindAsync(ID);

        if (entry == null)
            return new ApiResponse
            {
                Status = false,
                Message = "Entry not found"
            };

        _db.Entries.Remove(entry);
        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Deletes multiple entries by IDs.
    /// </summary>
    /// <param name="ids">List of Entry IDs to delete.</param>
    /// <returns>Operation status.</returns>
    public async Task<ApiResponse> DeleteEntriesAsync(List<int> ids)
    {
        var entries = await _db.Entries
            .Where(e => ids.Contains(e.ID))
            .ToListAsync();

        if (entries.Count == 0)
            return new ApiResponse
            {
                Status = false,
                Message = "No entries found"
            };

        _db.Entries.RemoveRange(entries);
        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets aggregated entry statistics for a date range: daily counts + per-store counts.
    /// </summary>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <returns>Daily and per-store entry counts.</returns>
    public async Task<EntryStatisticsResponseDTO> GetEntryStatisticsAsync(DateTime startDate, DateTime endDate)
    {
        var query = _db.Entries.Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate);

        var entries = await query.Select(e => e.EntryDate).ToListAsync();

        var dailyCounts = entries
            .GroupBy(d => d.Date)
            .OrderBy(g => g.Key)
            .Select(g => new EntryDailyCountDTO
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count()
            })
            .ToList();

        var storeEntries = await query.Select(e => e.IDStore).ToListAsync();

        var storeIds = storeEntries.Distinct().ToList();
        var storeNames = await _db.Stores
            .Where(s => storeIds.Contains(s.ID))
            .ToDictionaryAsync(s => s.ID, s => s.Name);

        var storeCountDtos = storeEntries
            .GroupBy(id => id)
            .OrderByDescending(g => g.Count())
            .Select(g => new EntryStoreCountDTO
            {
                IDStore = g.Key,
                StoreName = storeNames.GetValueOrDefault(g.Key),
                Count = g.Count()
            })
            .ToList();

        return new EntryStatisticsResponseDTO
        {
            DailyCounts = dailyCounts,
            StoreCounts = storeCountDtos,
            Status = true,
            Message = null
        };
    }
}
