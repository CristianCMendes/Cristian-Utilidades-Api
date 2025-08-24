using EntityEase.Context;
using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.SecretFriend;

namespace Utilidades.Api.Context;

public class UtilDbContext(DbContextOptions<UtilDbContext> options) : EEDbContext<UtilDbContext>(options) {
    public static bool HasUsers = false;

    public DbSet<User> Users { get; set; }
    public DbSet<SecretFriend> SecretFriends { get; set; }
    public DbSet<SecretFriendMember> SecretFriendMembers { get; set; }
    public DbSet<SecretFriendWishlist> SecretFriendWishlists { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        modelBuilder.UseSerialColumns();
        modelBuilder.HasDefaultSchema("utilidades");

        base.OnModelCreating(modelBuilder);
    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        base.ConfigureConventions(configurationBuilder);
        
        if (Database.IsNpgsql()) {
            configurationBuilder.Properties<DateTime>().HaveColumnType("timestamp without time zone");
        }

    }
}