using Data.Entities.Common;

namespace Data.Entities.Store;

public class GetStoreStatisticsResponseDTO : ApiResponse
{
    public int ID { get; set; }
    public required string Name { get; set; }
    public required string City { get; set; }
    public required string Country { get; set; }
    public List<DailyStatisticDTO> Statistics { get; set; } = new();
}
