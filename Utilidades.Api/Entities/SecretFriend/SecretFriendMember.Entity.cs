using EntityEase.Models.Repo;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.SecretFriend;

namespace Utilidades.Api.Entities.SecretFriend;

public class SecretFriendMemberEntity : EEEntity<SecretFriendMember> {
    /// <inheritdoc />
    public override ModelBuilder Build(ModelBuilder builder) {
        return builder.Entity<SecretFriendMember>(entity => {
            entity.ToTable(EntityName);

            entity.HasKey(e => new { e.SecretFriendId, e.UserId });
            entity.Property(e => e.SecretFriendId).IsRequired();
            entity.Property(e => e.UserId).IsRequired();
            entity.HasOne(e => e.SecretFriend).WithMany(e => e.Members).HasForeignKey(e => e.SecretFriendId);
            entity.HasOne(e => e.User).WithMany(e => e.SecretFriendMembers).HasForeignKey(e => e.UserId);
        });
    }
}