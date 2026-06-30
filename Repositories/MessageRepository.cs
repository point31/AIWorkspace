using AIWorkspace.Data;
using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIWorkspace.Repositories;

public class MessageRepository
{
    private readonly AppDbContext _db;

    public MessageRepository(AppDbContext db)
    {
        _db = db;
    }
    public async Task<List<MessageEntity>> GetByChatAsync(int chatId)
    {

        return await _db.Messages
            .Where(x => x.ChatEntityId == chatId)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<MessageEntity> AddAsync(
        int chatId,
        string role,
        string content)
    {

        var message = new MessageEntity
        {
            ChatEntityId = chatId,
            Role = role,
            Content = content,
            CreatedAt = DateTime.Now
        };

        _db.Messages.Add(message);

        var chat = await _db.Chats.FindAsync(chatId);

        if (chat != null)
        {
            chat.UpdatedAt = DateTime.Now;
        }

        await _db.SaveChangesAsync();

        return message;
    }
}