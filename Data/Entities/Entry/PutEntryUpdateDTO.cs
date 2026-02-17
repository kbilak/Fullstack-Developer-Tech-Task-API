using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Entry;

public class PutEntryUpdateDTO
{
    [Required]
    public required DateTime EntryDate { get; set; }
}
