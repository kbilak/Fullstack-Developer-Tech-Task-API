namespace Data.Entities.Common;

public class PaginationParams
{
    private const int MaxPageSize = 50;
    private const int DefaultPageSize = 10;

    public int Page { get; set; } = 1;

    private int _pageSize = DefaultPageSize;
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value > MaxPageSize ? MaxPageSize : value < 1 ? DefaultPageSize : value;
    }

    /// <summary>Sort expression as "field:direction", e.g. "name:asc", "id:desc". Default: "id:asc".</summary>
    public string? Sort { get; set; }

    /// <summary>Case-insensitive search query applied to store name.</summary>
    public string? Search { get; set; }
}

public class PaginatedResponse<T> : ApiResponse
{
    public List<T> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages { get; set; }
}
