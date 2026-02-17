namespace Data.Models;

/// <summary>
/// Represents a visitor entry (visit) to a store.
/// </summary>
public class Entry
{
    public int ID { get; set; }
    public int IDStore { get; set; }
    public DateTime EntryDate { get; set; }

    public Store Store { get; set; } = null!;
}
