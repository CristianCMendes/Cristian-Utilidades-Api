using EntityEase.Models.Repo;
using Microsoft.EntityFrameworkCore;

namespace Utilidades.Api.Entities.SecretFriend;

public class SecretFriendEntity : EEEntity<Models.SecretFriend.SecretFriend> {
    
    /// <inheritdoc />
    public override ModelBuilder Build(ModelBuilder builder) {
        return builder.Entity<Models.SecretFriend.SecretFriend>(entity => {
            
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).ValueGeneratedOnAdd();
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Description).IsRequired(false);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.CreatedByEntity).WithMany(e=>e.OwnSecretFriends).HasForeignKey(e => e.CreatedById);
        });
    }
}