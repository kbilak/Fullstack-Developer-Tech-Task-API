using Data.Entities.Common;
using Data.Entities.Store;

namespace Data.Interfaces;

/// <summary>
/// Store repository contract for CRUD operations and statistics.
/// </summary>
public interface IStoreRepository
{
    Task<PostStoreCreateResponseDTO> CreateStoreAsync(PostStoreCreateDTO newStore);
    Task<PaginatedResponse<StoreItemDTO>> GetAllStoresAsync(PaginationParams pagination);
    Task<GetStoreResponseDTO> GetStoreByIDAsync(int ID);
    Task<PostStoreCreateResponseDTO> UpdateStoreAsync(int ID, PutStoreUpdateDTO storeData);
    Task<ApiResponse> DeleteStoreAsync(int ID);
    Task<ApiResponse> DeleteStoresAsync(List<int> ids);
    Task<GetStoreStatisticsResponseDTO> GetStatisticsAsync(int IDStore, DateTime startDate, DateTime endDate);
}
