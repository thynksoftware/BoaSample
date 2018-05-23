using Microsoft.EntityFrameworkCore;
using Boa.Sample.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Boa.Sample.Data
{
    public class BoaIntegrationDbContext : IdentityDbContext<User>
    {
        public BoaIntegrationDbContext(DbContextOptions<BoaIntegrationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().ToTable("Users", "dbo");
            modelBuilder.Entity<IdentityRole>().ToTable("Roles", "dbo");
            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("UserRoles", "dbo");
            modelBuilder.Entity<IdentityUserClaim<string>>().ToTable("UserClaims", "dbo");
            modelBuilder.Entity<IdentityUserLogin<string>>().ToTable("UserLogins", "dbo");
        }
    }
}
