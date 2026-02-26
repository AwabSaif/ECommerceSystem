namespace SharedKernal.Models;

public class PaginationRequest
{
    public int PageIndex { get; set; } = 0;
    public int PageSize { get; set; } = 10;
}

public class SystemRequest : PaginationRequest
{
    public string? SortField { get; set; }
    public string? SortOrder { get; set; }
    public string? FilterValue { get; set; }
}

public class SystemRequest<T> : SystemRequest
{
    public T? MoreFilters { get; set; }
}