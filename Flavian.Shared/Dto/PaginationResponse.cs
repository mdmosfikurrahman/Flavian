namespace Flavian.Shared.Dto;

public class PaginationResponse<T>
{
    public IEnumerable<T>? Data { get; set; }
    public int TotalRecords { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalRecords / PageSize);
}
