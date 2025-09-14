using Microsoft.AspNetCore.Mvc;
using Utilidades.Api.Models.Identity;

namespace Utilidades.Api.Controllers.Attributes;

/// <summary>
/// Param to authorize user with permissions
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
public class NeedPermissionAttribute : ActionContextAttribute {
    public RoleType[] Permissions { get; }
    public bool AllowAny { get; set; } = true;

    public NeedPermissionAttribute(RoleType[] permissions, bool allowAny = true) {
        Permissions = permissions;
    }

    public NeedPermissionAttribute(RoleType permission, bool allowAny = true) {
        Permissions = [permission];
    }
}