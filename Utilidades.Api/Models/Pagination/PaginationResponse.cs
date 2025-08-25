using System.Text.Json.Serialization;

namespace Utilidades.Api.Models.Pagination;

public record PaginationResponse : Pagination {
    public int Total { get; init; }
    public int Pages => (int)Math.Ceiling(Total / (double)PageSize);
    public bool HasNext => Page < Pages;

    public PaginationResponse() { }

    public PaginationResponse(IPagination from) {
        Page = from.Page;
        PageSize = from.PageSize;
    }
    [JsonIgnore]public int Skip => int.Max(0, (Page - 1) * PageSize);
    [JsonIgnore]public int Take => int.Max(1, PageSize);
};