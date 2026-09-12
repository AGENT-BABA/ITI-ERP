namespace ITI.ERP.Application.Common.Models;

public class PaginationRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { 
        get => _pageSize;
        set => _pageSize = Math.Clamp(value, 1, 100); 
    }

    private int _pageSize = 10; 
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}
