namespace SharedKernal.Models;

public class SystemResponse
{
    public bool IsSuccess { get; set; }
    public string? Message { get; set; }
}

public class SystemResponse<T> : SystemResponse
{
    public T? ReturnedValue { get; set; }
}

public class SystemResponseList<T> : SystemResponse<T> 
{
    public int TotalCount { get; set; }
}