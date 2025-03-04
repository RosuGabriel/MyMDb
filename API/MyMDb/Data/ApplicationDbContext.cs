﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyMDb.Models;

namespace MyMDb.Data
{
    public class ApplicationDbContext : IdentityDbContext<AppUser>
    {
        private readonly IConfiguration _configuration;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, IConfiguration configuration) : base(options)
        {
            _configuration = configuration;
        }

        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<UserProfile> UserProfiles { get; set; }
        public DbSet<Media> Media { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<MediaAttribute> MediaAttributes { get; set; }
        public DbSet<UserRefreshToken> UserRefreshTokens { get; set; }
        public DbSet<ContinueWatching> ContinueWatchings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // admin and roles seeding

            var adminRole = new IdentityRole
            {
                Name = "admin",
                NormalizedName = "ADMIN"
            };

            var userRole = new IdentityRole
            {
                Name = "user",
                NormalizedName = "USER"
            };

            builder.Entity<IdentityRole>().HasData(adminRole, userRole);

            var adminUser = new AppUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "admin",
                Approved = true,
                NormalizedUserName = "ADMIN",
                Email = _configuration["Seeding:AdminEmail"]!,
                NormalizedEmail = _configuration["Seeding:AdminEmail"]!.ToUpper(),
                EmailConfirmed = true,
                SecurityStamp = Guid.NewGuid().ToString("D"),
                PasswordHash = new PasswordHasher<AppUser>().HashPassword(null!, _configuration["Seeding:AdminPassword"]!)
            };

            var adminProfile = new UserProfile { UserId = adminUser.Id, UserName = "Admin"};

            builder.Entity<AppUser>().HasData(adminUser);
            builder.Entity<UserProfile>().HasData(adminProfile);

            var adminUserRoles = new List<IdentityUserRole<string>>
        {
            new IdentityUserRole<string>
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            },
            new IdentityUserRole<string>
            {
                UserId = adminUser.Id,
                RoleId = userRole.Id
            }
        };

            builder.Entity<IdentityUserRole<string>>().HasData(adminUserRoles);

            // refreshToken
            builder.Entity<UserRefreshToken>()
                .HasKey(t => t.Id);

            // user - user profile
            builder.Entity<UserProfile>()
                .HasKey(p => p.UserId);

            builder.Entity<UserProfile>()
                .HasOne(p => p.User)
                .WithOne(u => u.UserProfile)
                .HasForeignKey<UserProfile>(p => p.UserId);

            // attributes - media
            builder.Entity<Media>()
                .HasMany(m => m.MediaAttributes)
                .WithOne(ma => ma.Media)
                .HasForeignKey(ma => ma.MediaId)
                .OnDelete(DeleteBehavior.Cascade);

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
                .HasOne(r => r.UserProfile)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            // media - continue watching
            builder.Entity<ContinueWatching>()
                .HasKey(m => m.Id);

            builder.Entity<ContinueWatching>()
                .HasOne(cw => cw.Media)
                .WithMany(m => m.ContinueWatchings)
                .HasForeignKey(cw => cw.MediaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
