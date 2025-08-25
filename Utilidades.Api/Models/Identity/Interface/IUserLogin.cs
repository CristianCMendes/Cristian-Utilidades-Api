namespace Utilidades.Api.Models.Identity.Interface;

public interface IUserLogin : IUser {
    public string? Token { get; set; }
    public DateTime? Expires { get; set; }
}