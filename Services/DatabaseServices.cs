using AIWorkspace.AI;
using AIWorkspace.Data;
using AIWorkspace.Entities;
using Microsoft.EntityFrameworkCore;

namespace AIWorkspace.Services;

public class DatabaseService
{
    private readonly AppDbContext _db;

    public DatabaseService(AppDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Applies all pending EF Core migrations and seeds default data.
    /// Safe to call on every startup — already-applied migrations and
    /// already-existing rows are left untouched.
    /// </summary>
    public void Initialize()
    {
        _db.Database.Migrate();

        SeedProviders();
    }

    private void SeedProviders()
    {
        var defaults = new[]
        {
            new ProviderSettingsEntity
            {
                Provider     = ProviderType.OpenAI,
                IsEnabled    = false,
                ApiKey       = "",
                DefaultModel = "gpt-4o-mini"
            },
            new ProviderSettingsEntity
            {
                Provider     = ProviderType.Claude,
                IsEnabled    = false,
                ApiKey       = "",
                DefaultModel = "claude-3-5-sonnet-20241022"
            },
            new ProviderSettingsEntity
            {
                Provider     = ProviderType.Gemini,
                IsEnabled    = false,
                ApiKey       = "",
                DefaultModel = "gemini-2.0-flash"
            }
        };

        foreach (var provider in defaults)
        {
            var exists = _db.ProviderSettings
                .Any(x => x.Provider == provider.Provider);

            if (!exists)
                _db.ProviderSettings.Add(provider);
        }

        _db.SaveChanges();
    }
}
