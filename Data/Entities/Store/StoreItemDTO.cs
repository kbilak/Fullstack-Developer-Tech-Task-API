namespace Data.Entities.Store;

public class StoreItemDTO
{
    public int ID { get; set; }
    public required string Name { get; set; }
    public int EntryCount { get; set; }
}
