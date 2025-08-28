namespace Utilidades.Api.Models.SecretFriend.Dto;

public class AddSecretFriendMemberDto {
    public int? UserId { get; set; }
    public string? Email { get; set; }
    public bool? Admin { get; set; }

    public static AddSecretFriendMemberDto Example => new() {
        UserId = 1,
        Email = "example@example.com",
        Admin = true
    };
}