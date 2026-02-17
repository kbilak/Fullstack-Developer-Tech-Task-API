using System.ComponentModel.DataAnnotations;

namespace Data.Entities.Store;

public class PostStoreCreateDTO
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Name { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string City { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public required string Country { get; set; }
}
