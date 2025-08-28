using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Models.Response;

public interface IResponse : IStatusCodeActionResult {
    PaginationResponse Pagination { get; set; }
    List<ResponseMessage> Messages { get; }
    List<LinkReference> Links { get; }
}

public interface IResponse<out T> : IResponse {
    T? Data { get; }
   
}