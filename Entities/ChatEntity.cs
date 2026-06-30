namespace AIWorkspace.Entities;

public class ChatEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string Provider { get; set; } = "";

    public string Model { get; set; } = "";
}