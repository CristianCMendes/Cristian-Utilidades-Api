using EntityEase.Models.Interfaces.Identity;
using Utilidades.Api.Models.Identity;

namespace Utilidades.Api.Models.SecretFriend;

public record SecretFriendMember {
    public int SecretFriendId { get; set; }
    public virtual SecretFriend? SecretFriend { get; set; }
    public int UserId { get; set; }
    public virtual User? User { get; set; }
    public bool IsAdmin { get; set; }
    public virtual ICollection<SecretFriendWishlist> Wishlists { get; set; } = [];
}