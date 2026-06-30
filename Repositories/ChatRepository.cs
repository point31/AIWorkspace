using AIWorkspace.Data;
using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;
using AIWorkspace.AI;

namespace AIWorkspace.Repositories;

public class ChatRepository
{
    private readonly AppDbContext _db;

    public ChatRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ChatEntity>> GetAllAsync()
    {
        return await _db.Chats
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<List<ChatEntity>> GetByProviderAsync(ProviderType provider)
    {
        return await _db.Chats
            .Where(x => x.Provider == provider)
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatEntity> CreateAsync(string title, ProviderType provider = ProviderType.OpenAI)
    {
        var chat = new ChatEntity
        {
            Title    = title,
            Provider = provider,
            Model    = "",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        _db.Chats.Add(chat);
        await _db.SaveChangesAsync();
        return chat;
    }

    public async Task UpdateTitleAsync(int id, string title)
    {
        var chat = await _db.Chats.FindAsync(id);
        if (chat == null) return;
        chat.Title = title;
        await _db.SaveChangesAsync();
    }

    public async Task UpdateProviderAsync(int id, ProviderType provider)
    {
        var chat = await _db.Chats.FindAsync(id);
        if (chat == null) return;
        chat.Provider = provider;
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var chat = await _db.Chats.FindAsync(id);
        if (chat == null) return;
        _db.Chats.Remove(chat);
        await _db.SaveChangesAsync();
    }

    public async Task<ChatEntity?> GetAsync(int id)
    {
        return await _db.Chats.FindAsync(id);
    }
}