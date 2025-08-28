using Microsoft.AspNetCore.Mvc;
using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Models.Response;

public class ApiResponse<T>() : JsonResult(null), IApiResponse<T>
    where T : class {
    public T? Data { get; set; }
    public PaginationResponse Pagination { get; set; } = new();

    /// <inheritdoc />
    public void SetData<T1>(T1? value) {
        Data = value as T;
    }

    public T1? GetData<T1>() where T1 : class {
        return Data as object as T1;
    }

    public object? GetData() {
        return Data;
    }

    /// <inheritdoc />
    public IEnumerable<ResponseMessage> GetMessages() {
        return Messages.DistinctBy(x => new { x.Message });
    }

    /// <inheritdoc />
    public IEnumerable<LinkReference> GetLinks() {
        return Links.DistinctBy(x => new { x.Rel, x.Href });
    }

    /// <inheritdoc />
    public List<ResponseMessage> Messages { get; set; } = [];

    /// <inheritdoc />
    public List<LinkReference> Links { get; set; } = [];

    public ApiResponse(T? data) : this() {
        Data = data;
    }

    public ApiResponse(T? data, PaginationResponse pagination) : this(data) {
        Data = data;
        Pagination = pagination;
    }

    public ApiResponse((T? Data, PaginationResponse Pagination) data) : this(data.Data, data.Pagination) { }

    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context) {

        Value = new {
            Data,
            Pagination,
            Messages = GetMessages(),
            Links = GetLinks(),
            StatusCode = StatusCode ?? context.HttpContext.Response.StatusCode
        };

        return base.ExecuteResultAsync(context);
    }

    /// <inheritdoc />
    public override void ExecuteResult(ActionContext context) {

        Value = new {
            Data,
            Pagination,
            Messages = GetMessages(),
            Links = GetLinks(),
            StatusCode = StatusCode ?? context.HttpContext.Response.StatusCode
        };
        base.ExecuteResult(context);
    }
}

public class ApiResponse : ApiResponse<object> {
    public ApiResponse() : base(null) { }
    public ApiResponse(object? data) : base(data) { }
    public ApiResponse(object? data, PaginationResponse pagination) : base(data, pagination) { }
    public ApiResponse((object? Data, PaginationResponse Pagination) data) : base(data) { }
}