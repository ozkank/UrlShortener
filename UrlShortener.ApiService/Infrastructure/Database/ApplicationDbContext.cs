using System.Collections.Generic;
using System.Reflection.Emit;
using UrlShortener.ApiService.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection;

namespace UrlShortener.ApiService.Infrastructure.Database
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions options)
            : base(options)
        {
        }

        public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ShortenedUrl>(builder =>
            {
                builder
                    .Property(shortenedUrl => shortenedUrl.Code)
                    .HasMaxLength(ShortLinkSettings.Length);

                builder
                    .HasIndex(shortenedUrl => shortenedUrl.Code)
                    .IsUnique();
            });
        }
    }

}
