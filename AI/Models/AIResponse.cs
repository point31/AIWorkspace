namespace AIWorkspace.AI.Models;

public class AIResponse
{
    public bool Success { get; set; }

    public string Content { get; set; } = "";

    public string Error { get; set; } = "";
}