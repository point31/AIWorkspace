using AIWorkspace.AI;
using AIWorkspace.Data;
using AIWorkspace.Entities;
using AIWorkspace.Security;
using Microsoft.EntityFrameworkCore;

namespace AIWorkspace.Repositories;

public class ProviderSettingsRepository
{
    private readonly AppDbContext _db;

    public ProviderSettingsRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<ProviderSettingsEntity>> GetAllAsync()
    {
        var rows = await _db.ProviderSettings
            .OrderBy(x => x.Provider)
            .ToListAsync();

        // Decrypt API keys before returning so callers always see plain text.
        foreach (var row in rows)
            row.ApiKey = SecureStorageService.Decrypt(row.ApiKey);

        return rows;
    }

    public async Task<ProviderSettingsEntity?> GetAsync(ProviderType provider)
    {
        var row = await _db.ProviderSettings
            .FirstOrDefaultAsync(x => x.Provider == provider);

        if (row != null)
            row.ApiKey = SecureStorageService.Decrypt(row.ApiKey);

        return row;
    }

    public async Task SaveAsync(ProviderSettingsEntity entity)
    {
        var existing = await _db.ProviderSettings
            .FirstOrDefaultAsync(x => x.Provider == entity.Provider);

        // Always encrypt the API key before persisting.
        var encryptedKey = SecureStorageService.Encrypt(entity.ApiKey);

        if (existing == null)
        {
            _db.ProviderSettings.Add(new ProviderSettingsEntity
            {
                Provider     = entity.Provider,
                IsEnabled    = entity.IsEnabled,
                ApiKey       = encryptedKey,
                DefaultModel = entity.DefaultModel
            });
        }
        else
        {
            existing.ApiKey       = encryptedKey;
            existing.DefaultModel = entity.DefaultModel;
            existing.IsEnabled    = entity.IsEnabled;
        }

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(ProviderType provider)
    {
        var existing = await _db.ProviderSettings
            .FirstOrDefaultAsync(x => x.Provider == provider);

        if (existing != null)
        {
            existing.ApiKey    = "";
            existing.IsEnabled = false;
            await _db.SaveChangesAsync();
        }
    }
}