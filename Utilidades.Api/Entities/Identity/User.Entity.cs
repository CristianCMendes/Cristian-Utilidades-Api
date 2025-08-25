using EntityEase.Models.Repo;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.Identity;

namespace Utilidades.Api.Entities.Identity;

public class UserEntity : EEEntity<User> {
    /// <inheritdoc />
    public override ModelBuilder Build(ModelBuilder builder) {
        return builder.Entity<User>(entity => {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Email).IsRequired();
            entity.Property(e => e.Password).IsRequired();
            entity.Property(e => e.IsActive);
            entity.Property(e => e.IsEmailConfirmed);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");

            entity.HasIndex(e => e.Email).IsUnique();
        });
    }
}