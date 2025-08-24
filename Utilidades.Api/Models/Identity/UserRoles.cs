namespace Utilidades.Api.Models.Identity;

public record UserRoles {
    public int UserId { get; set; }
    public virtual User? User { get; set; } 
    public RoleType Role { get; set; }
}