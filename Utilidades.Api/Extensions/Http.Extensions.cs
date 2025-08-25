using System.Security.Claims;

namespace Utilidades.Api.Extensions;

public static class HttpExtensions {
    public static int GetCurrentUserId(this HttpContext context) {
        var id = int.Parse(context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        return id;
    }
}