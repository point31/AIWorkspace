using AIWorkspace.Data;
using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIWorkspace.Repositories;

public class ChatRepository
{
    public async Task<List<ChatEntity>> GetAllAsync()
    {
        using var db = new AppDbContext();

        return await db.Chats
            .OrderByDescending(x => x.UpdatedAt)
            .ToListAsync();
    }

    public async Task<ChatEntity> CreateAsync(string title)
    {
        using var db = new AppDbContext();

        var chat = new ChatEntity
        {
            Title = "New Chat",
            Provider = "OpenAI", // temporary default
            Model = "",
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };

        db.Chats.Add(chat);

        await db.SaveChangesAsync();

        return chat;
    }

    public async Task DeleteAsync(int id)
    {
        using var db = new AppDbContext();

        var chat = await db.Chats.FindAsync(id);

        if (chat == null)
            return;

        db.Chats.Remove(chat);

        await db.SaveChangesAsync();
    }


}