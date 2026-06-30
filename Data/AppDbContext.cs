using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;
using System.IO;

namespace AIWorkspace.Data;

public class AppDbContext : DbContext
{
    public DbSet<ChatEntity> Chats => Set<ChatEntity>();

    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        var dbPath = Path.Combine(
        AppDomain.CurrentDomain.BaseDirectory,
        "AIWorkspace.db");

        optionsBuilder.UseSqlite($"Data Source={dbPath}");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatEntity>()
            .HasMany<MessageEntity>()
            .WithOne(x => x.Chat)
            .HasForeignKey(x => x.ChatEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}