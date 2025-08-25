using EntityEase.Models.Interfaces.Identity;
using EntityEase.Models.Interfaces.Identity.Naming;
using EntityEase.Models.Interfaces.Status;
using Utilidades.Api.Models.Identity.Interface;
using Utilidades.Api.Models.SecretFriend;

namespace Utilidades.Api.Models.Identity.Dto;

// classe criada apenas para não popular a senha em lugar nenhum fora na entidade
public record UserResponse : IUser {
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    /// <inheritdoc />
    public bool IsActive { get; set; } = true;

    public bool IsEmailConfirmed { get; set; } = false;

    public virtual ICollection<SecretFriend.SecretFriend> OwnSecretFriends { get; set; } = [];
    public virtual ICollection<SecretFriendMember> SecretFriendMembers { get; set; } = [];
    public virtual ICollection<SecretFriendWishlist> SecretFriendWishlists { get; set; } = [];
    public virtual ICollection<UserRoles> Roles { get; set; } = [];
    
    public UserResponse() { }

    public UserResponse(User user) : this() {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
        IsActive = user.IsActive;
        IsEmailConfirmed = user.IsEmailConfirmed;
        SecretFriendMembers = user.SecretFriendMembers;
        SecretFriendWishlists = user.SecretFriendWishlists;
        OwnSecretFriends = user.OwnSecretFriends;
        Roles = user.Roles;
    }

    /// <inheritdoc />
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}