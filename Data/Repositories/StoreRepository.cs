using Data.Entities.Common;
using Data.Entities.Store;
using Data.Interfaces;
using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories;

public class StoreRepository : IStoreRepository
{
    private readonly AppDbContext _db;

    public StoreRepository(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Creates a new store.
    /// </summary>
    /// <param name="newStore">Store data to create.</param>
    /// <returns>New store ID.</returns>
    public async Task<PostStoreCreateResponseDTO> CreateStoreAsync(PostStoreCreateDTO newStore)
    {
        var store = new Store
        {
            Name = newStore.Name,
            City = newStore.City,
            Country = newStore.Country
        };

        await _db.Stores.AddAsync(store);
        await _db.SaveChangesAsync();

        return new PostStoreCreateResponseDTO
        {
            ID = store.ID,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets all stores with pagination, sorting and search.
    /// </summary>
    /// <param name="pagination">Pagination, sort and search parameters.</param>
    /// <returns>Paginated list of stores.</returns>
    public async Task<PaginatedResponse<StoreItemDTO>> GetAllStoresAsync(PaginationParams pagination)
    {
        IQueryable<Store> query = _db.Stores;

        // Apply search filter (case-insensitive on Name)
        if (!string.IsNullOrWhiteSpace(pagination.Search))
        {
            var search = pagination.Search.Trim().ToLower();
            query = query.Where(s => s.Name.ToLower().Contains(search));
        }

        // Count after filtering
        var totalItems = await query.CountAsync();
        var totalPages = (int)Math.Ceiling(totalItems / (double)pagination.PageSize);

        // Apply ordering
        var (sortField, sortAsc) = ParseSort(pagination.Sort);
        query = sortField switch
        {
            "name" => sortAsc ? query.OrderBy(s => s.Name) : query.OrderByDescending(s => s.Name),
            _ => sortAsc ? query.OrderBy(s => s.ID) : query.OrderByDescending(s => s.ID),
        };

        // Paginate and project
        var stores = await query
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(s => new StoreItemDTO
            {
                ID = s.ID,
                Name = s.Name,
                EntryCount = s.Entries.Count
            })
            .ToListAsync();

        return new PaginatedResponse<StoreItemDTO>
        {
            Items = stores,
            Page = pagination.Page,
            PageSize = pagination.PageSize,
            TotalItems = totalItems,
            TotalPages = totalPages,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Parses a sort string like "name:desc" into field name and direction.
    /// </summary>
    private static (string field, bool ascending) ParseSort(string? sort)
    {
        if (string.IsNullOrWhiteSpace(sort))
            return ("id", true);

        var parts = sort.Split(':');
        var field = parts[0].ToLower();
        var asc = parts.Length < 2 || !parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

        if (field != "name" && field != "id")
            field = "id";

        return (field, asc);
    }

    /// <summary>
    /// Gets a store by ID.
    /// </summary>
    /// <param name="ID">Store ID.</param>
    /// <returns>Store data.</returns>
    public async Task<GetStoreResponseDTO> GetStoreByIDAsync(int ID)
    {
        var store = await _db.Stores.FindAsync(ID);

        if (store == null)
            return new GetStoreResponseDTO
            {
                Name = string.Empty,
                City = string.Empty,
                Country = string.Empty,
                Status = false,
                Message = "Store not found"
            };

        return new GetStoreResponseDTO
        {
            ID = store.ID,
            Name = store.Name,
            City = store.City,
            Country = store.Country,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Updates an existing store.
    /// </summary>
    /// <param name="ID">Store ID.</param>
    /// <param name="storeData">Updated store data.</param>
    /// <returns>Updated store ID.</returns>
    public async Task<PostStoreCreateResponseDTO> UpdateStoreAsync(int ID, PutStoreUpdateDTO storeData)
    {
        var store = await _db.Stores.FindAsync(ID);

        if (store == null)
            return new PostStoreCreateResponseDTO
            {
                ID = 0,
                Status = false,
                Message = "Store not found"
            };

        store.Name = storeData.Name;
        store.City = storeData.City;
        store.Country = storeData.Country;

        _db.Stores.Update(store);
        await _db.SaveChangesAsync();

        return new PostStoreCreateResponseDTO
        {
            ID = store.ID,
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Deletes a store by ID.
    /// </summary>
    /// <param name="ID">Store ID.</param>
    /// <returns>Operation status.</returns>
    public async Task<ApiResponse> DeleteStoreAsync(int ID)
    {
        var store = await _db.Stores.FindAsync(ID);

        if (store == null)
            return new ApiResponse
            {
                Status = false,
                Message = "Store not found"
            };

        _db.Stores.Remove(store);
        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Deletes multiple stores by IDs.
    /// </summary>
    /// <param name="ids">List of Store IDs to delete.</param>
    /// <returns>Operation status.</returns>
    public async Task<ApiResponse> DeleteStoresAsync(List<int> ids)
    {
        var stores = await _db.Stores
            .Where(s => ids.Contains(s.ID))
            .ToListAsync();

        if (stores.Count == 0)
            return new ApiResponse
            {
                Status = false,
                Message = "No stores found"
            };

        _db.Stores.RemoveRange(stores);
        await _db.SaveChangesAsync();

        return new ApiResponse
        {
            Status = true,
            Message = null
        };
    }

    /// <summary>
    /// Gets store statistics within a date range.
    /// </summary>
    /// <param name="IDStore">Store ID.</param>
    /// <param name="startDate">Start date of the range.</param>
    /// <param name="endDate">End date of the range.</param>
    /// <returns>Store data with daily visit statistics.</returns>
    public async Task<GetStoreStatisticsResponseDTO> GetStatisticsAsync(int IDStore, DateTime startDate, DateTime endDate)
    {
        var store = await _db.Stores
            .Include(s => s.Entries.Where(e => e.EntryDate >= startDate && e.EntryDate <= endDate))
            .FirstOrDefaultAsync(s => s.ID == IDStore);

        if (store == null)
            return new GetStoreStatisticsResponseDTO
            {
                Name = string.Empty,
                City = string.Empty,
                Country = string.Empty,
                Status = false,
                Message = "Store not found"
            };

        var statistics = store.Entries
            .GroupBy(e => e.EntryDate.Date)
            .Select(g => new DailyStatisticDTO
            {
                Date = g.Key.ToString("yyyy-MM-dd"),
                Count = g.Count()
            })
            .OrderBy(s => s.Date)
            .ToList();

        return new GetStoreStatisticsResponseDTO
        {
            ID = store.ID,
            Name = store.Name,
            City = store.City,
            Country = store.Country,
            Statistics = statistics,
            Status = true,
            Message = null
        };
    }
}
