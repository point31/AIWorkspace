namespace AIWorkspace.Entities;

using AIWorkspace.AI;

public class ChatEntity
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public ProviderType Provider { get; set; }

    public string Model { get; set; } = "";
}