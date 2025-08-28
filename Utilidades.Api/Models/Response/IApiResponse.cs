using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Models.Response;

public interface IApiResponse : IStatusCodeActionResult {
    PaginationResponse Pagination { get; set; }
    List<ResponseMessage> Messages { get; }
    List<LinkReference> Links { get; }
#pragma warning disable CS0108, CS0114
    int? StatusCode { get; set; }
#pragma warning restore CS0108, CS0114
    void SetData<T>(T? value);
    T? GetData<T>() where T : class;
    object? GetData();
    IEnumerable<ResponseMessage> GetMessages();
    IEnumerable<LinkReference> GetLinks();
}

public interface IApiResponse<out T> : IApiResponse {
    T? Data { get; }
}