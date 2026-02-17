using Data.Entities.Common;

namespace Data.Entities.Entry;

public class EntryStatisticsResponseDTO : ApiResponse
{
    public List<EntryDailyCountDTO> DailyCounts { get; set; } = new();
    public List<EntryStoreCountDTO> StoreCounts { get; set; } = new();
}

public class EntryDailyCountDTO
{
    public required string Date { get; set; }
    public int Count { get; set; }
}

public class EntryStoreCountDTO
{
    public int IDStore { get; set; }
    public string? StoreName { get; set; }
    public int Count { get; set; }
}
