using System.Linq.Expressions;
using EntityEase.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.SecretFriend;

namespace Utilidades.Api.Context;

public class UtilDbContext : EEDbContext<UtilDbContext> {
    public UtilDbContext(DbContextOptions<UtilDbContext> options) : base(options) { }
    public static bool HasUsers = false;

    public DbSet<User> Users { get; set; }
    public DbSet<SecretFriend> SecretFriends { get; set; }
    public DbSet<SecretFriendMember> SecretFriendMembers { get; set; }
    public DbSet<SecretFriendWishlist> SecretFriendWishlists { get; set; }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder modelBuilder) {
        base.OnModelCreating(modelBuilder);

        modelBuilder.UseSerialColumns();
        modelBuilder.HasDefaultSchema("utilidades");

    }

    /// <inheritdoc />
    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder) {
        base.ConfigureConventions(configurationBuilder);


        if (Database.IsNpgsql()) {
            configurationBuilder.Properties<DateTime>(x => {
                x.HaveColumnType("timestamp with time zone");
                AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
                
            });
        }

    }
}

