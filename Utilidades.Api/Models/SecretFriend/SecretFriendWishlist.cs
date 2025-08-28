using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.SecretFriend.Interface;

namespace Utilidades.Api.Models.SecretFriend;

public record SecretFriendWishlist : ISecretFriendWishlist {
    public int SecretFriendId { get; set; }
    public int UserId { get; set; }
    public virtual SecretFriend? SecretFriend { get; set; }
    public virtual User? User { get; set; }
    public string Wish { get; set; }
    public decimal? Price { get; set; }
    public DateTime Date { get; set; }
}