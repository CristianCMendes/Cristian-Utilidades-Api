namespace Utilidades.Api.Models.SecretFriend.Interface;

public interface ISecretFriendWishlist {
    public int SecretFriendId { get; set; }
    public int UserId { get; set; }
    public string Wish { get; set; }
    public decimal? Price { get; set; }
    public DateTime Date { get; set; }
}