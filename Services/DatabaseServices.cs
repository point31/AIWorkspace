using AIWorkspace.Data;

namespace AIWorkspace.Services;

public static class DatabaseService
{
    public static void Initialize()
    {
        using var db = new AppDbContext();

        db.Database.EnsureCreated();
    }
}