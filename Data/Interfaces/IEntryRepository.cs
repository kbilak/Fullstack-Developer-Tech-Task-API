using Data.Entities.Common;
using Data.Entities.Entry;

namespace Data.Interfaces;

/// <summary>
/// Entry repository contract for creating and querying store visit entries.
/// </summary>
public interface IEntryRepository
{
    Task<PostEntryCreateResponseDTO> AddEntryAsync(PostEntryCreateDTO newEntry);
    Task<PaginatedResponse<EntryListItemDTO>> GetAllEntriesAsync(PaginationParams pagination);
    Task<PaginatedResponse<StoreEntryItemDTO>> GetEntriesByStoreAsync(int IDStore, PaginationParams pagination);
    Task<PaginatedResponse<EntryListItemDTO>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate, PaginationParams pagination);
    Task<PaginatedResponse<EntryListItemDTO>> GetEntriesByDateAsync(DateTime date, PaginationParams pagination);
    Task<PaginatedResponse<StoreEntryItemDTO>> GetEntriesByStoreAndDateAsync(int IDStore, DateTime date, PaginationParams pagination);
    Task<ApiResponse> UpdateEntryAsync(int ID, PutEntryUpdateDTO entryData);
    Task<ApiResponse> DeleteEntryAsync(int ID);
    Task<ApiResponse> DeleteEntriesAsync(List<int> ids);
    Task<EntryStatisticsResponseDTO> GetEntryStatisticsAsync(DateTime startDate, DateTime endDate);
}
