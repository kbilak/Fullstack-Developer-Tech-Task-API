using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Entry;

public class PostEntryCreateDTO
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "IDStore must be greater than 0")]
    public required int IDStore { get; set; }

    [Required]
    public required DateTime EntryDate { get; set; }
}
