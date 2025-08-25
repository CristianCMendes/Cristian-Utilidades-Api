using EntityEase.Models.Repo;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.SecretFriend;

namespace Utilidades.Api.Entities.SecretFriend;

public class SecretFriendWishlistEntity : EEEntity<SecretFriendWishlist> {
    /// <inheritdoc />
    public override ModelBuilder Build(ModelBuilder builder) {
        return builder.Entity<SecretFriendWishlist>(entity => {
            entity.HasKey(e => new { e.SecretFriendId, e.UserId });
            entity.Property(e => e.Wish).IsRequired();
            entity.Property(e => e.Price).IsRequired(false);
            entity.Property(e => e.Date).HasDefaultValueSql("CURRENT_TIMESTAMP");
            entity.HasOne(e => e.SecretFriend).WithMany(e => e.Wishlists).HasForeignKey(e => e.SecretFriendId);
            entity.HasOne(e => e.User).WithMany(e => e.SecretFriendWishlists).HasForeignKey(e => e.UserId);
        });
    }
}