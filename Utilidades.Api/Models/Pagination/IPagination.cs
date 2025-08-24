namespace Utilidades.Api.Models.Pagination;

public interface IPagination {
    int Page { get; set; }
    int PageSize { get; set; }
}