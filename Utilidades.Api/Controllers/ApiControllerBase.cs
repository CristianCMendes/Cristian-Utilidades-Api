using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Utilidades.Api.Controllers.Attributes;
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
    private static readonly List<string> AllowedApiKeys = [];
    public static void AddApiKey(string apiKey) => AllowedApiKeys.Add(apiKey);
    protected static void RemoveApiKey(string apiKey) => AllowedApiKeys.Remove(apiKey);
    protected static string[] GetAllowedApiKeys() => AllowedApiKeys.ToArray();

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


    // If you wondering why not use action filters, its because i cant keep the same ApiResponse object if i use action filters...
    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context) {
        if (AllowedApiKeys.All(x => x != Request.Headers["X-API-KEY"])) {
            ApiResponse.Messages.Add(new("Token da aplicação não autorizado", MessageType.error, important: true));
            ApiResponse.StatusCode = StatusCodes.Status401Unauthorized;
            context.Result = ApiResponse;

            return;
        }

        var requiredPermissions =
            context.ActionDescriptor.EndpointMetadata.OfType<NeedPermissionAttribute>().FirstOrDefault();

        // Check if the user has the required permissions
        if (requiredPermissions?.Permissions.Length > 0) {
            var permissions = requiredPermissions.Permissions;
            var autorized = 0;
            foreach (var perm in permissions) {
                if (User.IsInRole(perm.ToString())) {
                    if (requiredPermissions.AllowAny) break;

                    autorized++;
                }
                else if (!requiredPermissions.AllowAny) {
                    break;
                }
            }

            if (autorized <= 0) {
                ApiResponse.Messages.Add(
                    new("O usuário não possui permissão para acessar o recurso", MessageType.warning));
                ApiResponse.StatusCode = StatusCodes.Status401Unauthorized;
                context.Result = ApiResponse;

                return;
            }
        }


        if (HttpContext.User.Claims.Any(x => x.Type == AppClaimTypes.NoPassword && x.Value == true.ToString())) {
            if (context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute)) return;
            ApiResponse.Messages.Add(new("O usuario deve definir uma senha para acessar", MessageType.warning,
                important: true));
            ApiResponse.StatusCode = StatusCodes.Status401Unauthorized;

            context.Result = ApiResponse;

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