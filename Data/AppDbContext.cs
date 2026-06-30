using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIWorkspace.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ChatEntity> Chats => Set<ChatEntity>();

    public DbSet<MessageEntity> Messages => Set<MessageEntity>();

    public DbSet<ProviderSettingsEntity> ProviderSettings => Set<ProviderSettingsEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ChatEntity>()
            .HasMany<MessageEntity>()
            .WithOne(x => x.Chat)
            .HasForeignKey(x => x.ChatEntityId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}