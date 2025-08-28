namespace Utilidades.Api.Models.Identity.Cache;

public class UserOtp {
    public int UserId { get; set; }
    public string Otp { get; init; } = Random.Shared.Next(100000, 999999).ToString();
    public TimeSpan Expires { get; init; } = TimeSpan.FromMinutes(10);
}