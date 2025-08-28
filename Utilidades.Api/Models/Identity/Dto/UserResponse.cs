using EntityEase.Models.Interfaces.Identity;
using EntityEase.Models.Interfaces.Identity.Naming;
using EntityEase.Models.Interfaces.Status;
using NSwag.Annotations;
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

    /// <inheritdoc />
    public int? InvitedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [OpenApiIgnore] public virtual User? InvitedBy { get; set; }

    [OpenApiIgnore]public virtual ICollection<SecretFriend.SecretFriend> OwnSecretFriends { get; set; } = [];
    [OpenApiIgnore]public virtual ICollection<SecretFriendMember> SecretFriendMembers { get; set; } = [];
    [OpenApiIgnore]public virtual ICollection<SecretFriendWishlist> SecretFriendWishlists { get; set; } = [];
    [OpenApiIgnore]public virtual ICollection<UserRoles> Roles { get; set; } = [];

    public UserResponse() { }

    public UserResponse(User user) : this() {
        Id = user.Id;
        Name = user.Name;
        Email = user.Email;
        IsActive = user.IsActive;
        IsEmailConfirmed = user.IsEmailConfirmed;
        InvitedById = user.InvitedById;
        InvitedBy = user.InvitedBy;
        SecretFriendMembers = user.SecretFriendMembers;
        SecretFriendWishlists = user.SecretFriendWishlists;
        OwnSecretFriends = user.OwnSecretFriends;
        Roles = user.Roles;
    }

    public UserResponse(IUser user) : this((User)user) { }
    public UserResponse(IUserLogin user) : this((User)user) { }

    /// <inheritdoc />
}