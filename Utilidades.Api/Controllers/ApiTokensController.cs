using Microsoft.AspNetCore.Mvc;
using Utilidades.Api.Controllers.Attributes;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Controllers;

public class ApiTokensController : ApiControllerBase {
    [NeedPermission(RoleType.Master)]
    [HttpPost(nameof(Create))]
    public IApiResponse Create(string? token) {
        token ??= Guid.NewGuid().ToString();
        AddApiKey(token);

        return ApiResponse;
    }

    [NeedPermission(RoleType.Master)]
    [HttpDelete(nameof(Delete))]
    public IApiResponse Delete(string token) {
        RemoveApiKey(token);

        return ApiResponse;
    }

    [NeedPermission(RoleType.Master)]
    [HttpGet(nameof(Get))]
    public IApiResponse Get() {
        ApiResponse.SetData(GetAllowedApiKeys());

        return ApiResponse;
    }
}