namespace Utilidades.Api.Models.Identity.Dto;

public record UserLoginResponse : UserResponse {
    public string? Token { get; set; }
    public DateTime? Expires { get; set; }

    public UserLoginResponse() { }
    public UserLoginResponse(User user) : base(user) { }
}