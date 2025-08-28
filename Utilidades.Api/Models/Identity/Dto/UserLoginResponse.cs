using Utilidades.Api.Models.Identity.Interface;

namespace Utilidades.Api.Models.Identity.Dto;

public record UserLoginResponse : UserResponse, IUserLogin {
    public string? Token { get; set; }
    public DateTime? Expires { get; set; }
    public Guid? RefreshToken { get; set; }

    public UserLoginResponse() { }
    public UserLoginResponse(User user) : base(user) { }

    /// <inheritdoc />
    public UserLoginResponse(UserResponse user) : base(user) { }

    public UserLoginResponse(IUser user) : this() { }
}