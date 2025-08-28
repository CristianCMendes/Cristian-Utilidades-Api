using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
[Produces("application/json")]
// Add 8 seconds of cache to all api routes
[ResponseCache(Duration = 8, Location = ResponseCacheLocation.Any, NoStore = false, VaryByQueryKeys = ["*"])]
public class ApiControllerBase : Controller {
    private IApiResponse _apiResponse = new ApiResponse();

    protected IApiResponse ApiResponse {
        get => _apiResponse;
        set {
            if (_apiResponse.Links is { Count: > 0 }) {
                value.Links.AddRange(_apiResponse.Links);
            }

            if (_apiResponse.Messages is { Count: > 0 }) {
                value.Messages.AddRange(_apiResponse.Messages);
            }

            _apiResponse = value;
        }
    }

    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context) {
        if (HttpContext.User.Claims.Any(x => x.Type == AppClaimTypes.NoPassword && x.Value == true.ToString())) {
            if (context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute)) return;
            context.Result = new ApiResponse() {
                Messages = {
                    new() {
                        Message = "O usuario deve definir uma senha para acessar",
                        Type = MessageType.warning
                    }
                },
                StatusCode = StatusCodes.Status401Unauthorized,
            };

            return;
        }

        base.OnActionExecuting(context);
    }


    protected LinkReference LinkRef(string actionName, string? controllerName = null, object? routeData = null,
        string? rel = null,
        Method? method = null) {
        return new(Url.Action(actionName, controller: controllerName, routeData), method, rel);
    }
}