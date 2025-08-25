using Microsoft.AspNetCore.Mvc;
using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Models.Response;

public class Response<T> : JsonResult, IResponse<T> {
    public T? Data { get; }
    public PaginationResponse Pagination { get; set; } = new();

    /// <inheritdoc />
    public List<ResponseMessage> Messages { get; } = [];

    /// <inheritdoc />
    public List<LinkReference> Links { get; } = [];
    
    public Response() : base(null) { }

    public Response(T? data) : this() {
        Data = data;
    }

    public Response(T? data, PaginationResponse pagination) : this(data) {
        Data = data;
        Pagination = pagination;
    }

    public Response((T? Data, PaginationResponse Pagination) data) : this(data.Data, data.Pagination) { }

    /// <inheritdoc />
    public override Task ExecuteResultAsync(ActionContext context) {
        Value = new {
            Data, 
            Pagination, 
            Messages,
            Links
        };

        return base.ExecuteResultAsync(context);
    }

    /// <inheritdoc />
    public override void ExecuteResult(ActionContext context) {
        Value = new {
            Data, 
            Pagination, 
            Messages,
            Links
        };
        base.ExecuteResult(context);
    }
}

public class Response : Response<object> {
    public Response() : base(null) { }
    public Response(object data) : base(data) { }
    public Response(object data, PaginationResponse pagination) : base(data, pagination) { }
    public Response((object Data, PaginationResponse Pagination) data) : base(data) { }
}