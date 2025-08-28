using Utilidades.Api.Models.Identity.Interface;

namespace Utilidades.Api.Models.Identity.Dto;

public record UserInviteDto {
    public string Email { get; set; }
    public string? Name { get; set; }
    public int SecretFriendId { get; set; } 
}