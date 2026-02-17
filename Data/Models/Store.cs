namespace Data.Models;

/// <summary>
/// Represents a physical store location.
/// </summary>
public class Store
{
    public int ID { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
