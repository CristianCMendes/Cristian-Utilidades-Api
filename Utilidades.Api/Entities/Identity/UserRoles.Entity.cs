using EntityEase.Models.Repo;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.Identity;

namespace Utilidades.Api.Entities.Identity;

public class UserRolesEntity : EEEntity<UserRoles> {
    /// <inheritdoc />
    public override ModelBuilder Build(ModelBuilder builder) {
        return builder.Entity<UserRoles>(entity => {
            entity.ToTable(EntityName);
            entity.HasKey(e => new { e.UserId, e.Role });
            entity.Property(e => e.Role).HasConversion<string>();
            entity.HasOne(e => e.User).WithMany(e => e.Roles).HasForeignKey(e => e.UserId);
            entity.HasIndex(e => new { e.UserId, e.Role }).IsUnique();
        });
    }
}