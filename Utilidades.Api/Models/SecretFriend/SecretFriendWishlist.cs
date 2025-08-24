using Utilidades.Api.Models.Identity;

namespace Utilidades.Api.Models.SecretFriend;

public record SecretFriendWishlist {
    public int SecretFriendId { get; set; }
    public int UserId { get; set; }
    public virtual SecretFriend? SecretFriend { get; set; }
    public virtual User? User { get; set; }
    public string Wish { get; set; }
    public decimal? Price { get; set; }
    public DateTime Date { get; set; }
}