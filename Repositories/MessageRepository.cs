using AIWorkspace.Data;
using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIWorkspace.Repositories;

public class MessageRepository
{
    public async Task<List<MessageEntity>> GetByChatAsync(int chatId)
    {
        using var db = new AppDbContext();

        return await db.Messages
            .Where(x => x.ChatEntityId == chatId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<MessageEntity> AddAsync(
        int chatId,
        string role,
        string content)
    {
        using var db = new AppDbContext();

        var message = new MessageEntity
        {
            ChatEntityId = chatId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.Now
        };

        db.Messages.Add(message);

        var chat = await db.Chats.FindAsync(chatId);

        if (chat != null)
        {
            chat.UpdatedAt = DateTime.Now;
        }

        await db.SaveChangesAsync();

        return message;
    }
}