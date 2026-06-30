namespace AIWorkspace.Entities;

using AIWorkspace.AI;

public class ProviderSettingsEntity
{
    public int Id { get; set; }

    public ProviderType Provider { get; set; }

    public bool IsEnabled { get; set; }

    public string ApiKey { get; set; } = "";

    public string DefaultModel { get; set; } = "";
}