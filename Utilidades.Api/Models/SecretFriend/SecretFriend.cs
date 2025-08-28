using EntityEase.Models.Interfaces.Tracking;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.SecretFriend.Dto;
using Utilidades.Api.Models.SecretFriend.Interface;

namespace Utilidades.Api.Models.SecretFriend;

public record SecretFriend : ISecretFriend, IEECreatableBy<User, int> {
    public int Id { get; set; }
    public string Name { get; set; } 
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public virtual User? CreatedByEntity { get; set; }
    public int CreatedById { get; set; }
    public DateTime Date { get; set; }
    public decimal? MinimumPrice { get; set; }
    public decimal? MaximumPrice { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Allow any user to draw friend, even if they are not a admin
    /// </summary>
    public bool AllowPick { get; set; } = true;

    public virtual ICollection<SecretFriendWishlist> Wishlists { get; set; } = [];
    public virtual ICollection<SecretFriendMember> Members { get; set; } = [];

    public SecretFriend() { }

    public SecretFriend(CreateSecretFriendDto dto) : this() {
        Name = dto.Name;
        Description = dto.Description;
        Date = dto.Date;
        MinimumPrice = dto.MinimumPrice;
        MaximumPrice = dto.MaximumPrice;
    }
}