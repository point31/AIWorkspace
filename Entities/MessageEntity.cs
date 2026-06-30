namespace AIWorkspace.Entities;

public class MessageEntity
{
    public int Id { get; set; }

    public int ChatEntityId { get; set; }

    public ChatEntity? Chat { get; set; }

    public string Role { get; set; } = "";

    public string Content { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}