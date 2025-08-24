using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Utilidades.Api.Context;

namespace Utilidades.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/[controller]")]
public class ApiControllerBase(UtilDbContext dbContext) : ControllerBase {
    protected int UserId => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
}