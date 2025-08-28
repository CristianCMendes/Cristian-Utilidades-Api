namespace Utilidades.Api.Models.Identity.Dto;

public record UserConfirmMail {
    public string Email { get; set; }
    public int Token { get; set; }
}