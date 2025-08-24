namespace Utilidades.Api.Models.Pagination;

public class PaginationResponse : Pagination {
    public int Total { get; init; }
    public int Pages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasNext => Page < Pages;

    public PaginationResponse() { }

    public PaginationResponse(IPagination from) {
        Page = from.Page;
        PageSize = from.PageSize;
    }
};