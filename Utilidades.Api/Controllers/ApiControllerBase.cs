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
    /// <inheritdoc />
    public override void OnActionExecuting(ActionExecutingContext context) {
        if (HttpContext.User.Claims.Any(x => x.Type == AppClaimTypes.NoPassword && x.Value == true.ToString())) {
            if (context.ActionDescriptor.EndpointMetadata.Any(x => x is AllowAnonymousAttribute)) return;
            context.Result = new Response() {
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


    protected string GetRoute(string controllerName, string details) =>
        $"api/v1/{controllerName.Replace("Controller", "")}/{details}";
}