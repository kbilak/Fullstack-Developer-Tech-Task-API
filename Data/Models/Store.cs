namespace Data.Models;

/// <summary>
/// Represents a physical store location.
/// </summary>
public class Store
{
    public int ID { get; set; }
    public string Name { get; set; } = "";
    public string City { get; set; } = "";
    public string Country { get; set; } = "";

    public ICollection<Entry> Entries { get; set; } = new List<Entry>();
}
