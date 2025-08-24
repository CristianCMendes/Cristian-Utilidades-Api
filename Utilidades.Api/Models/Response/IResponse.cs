using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Models.Response;

public interface IResponse<out T> {
    T? Data { get; }
    PaginationResponse Pagination { get; set; }
    List<ResponseMessage> Messages { get; }
    List<LinkReference> Links { get; }
}