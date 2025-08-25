using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Utilidades.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ApiControllerBase : ControllerBase {
    protected string GetRoute(string controllerName, string details) =>
        $"api/v1/{controllerName.Replace("Controller", "")}/{details}";
}