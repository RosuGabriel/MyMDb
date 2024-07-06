using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMDb.Models;
using System.Reflection.Emit;

namespace MyMDb.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Review> Reviews { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // user - user profile
            builder.Entity<UserProfile>()
                .HasKey(p => p.Id);

            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // movie types (movie, series or episode)
            builder.Entity<Media>()
           .HasDiscriminator<string>("MediaType")
           .HasValue<Movie>("Movie")
           .HasValue<Series>("Series")
           .HasValue<Episode>("Episode");

            // series - episodes
            builder.Entity<Series>()
                .HasMany(s => s.Episodes)
                .WithOne(e => e.Series)
                .HasForeignKey(e => e.SeriesId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            
            // media - reviews
            builder.Entity<Media>()
                .HasKey(m => m.Id);

            builder.Entity<Media>()
                .HasMany(m => m.Reviews)
                .WithOne(r => r.Media)
                .HasForeignKey(r => r.MediaId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // user - reviews
            builder.Entity<Review>()
                .HasKey(r => r.Id);

            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
