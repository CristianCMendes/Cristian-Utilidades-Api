namespace Utilidades.Api.Models.SecretFriend.Interface;

public interface ISecretFriendMember {
    public int SecretFriendId { get; set; }
    public int UserId { get; set; }
    public int? UserPickedId { get; set; }
    public bool IsAdmin { get; set; }
}