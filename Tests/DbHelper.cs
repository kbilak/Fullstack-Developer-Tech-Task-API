using Data;
using Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Tests;

/// <summary>
/// Creates a fresh SQLite in-memory AppDbContext for each test.
/// Uses real SQLite (same as production) to avoid EF Core InMemory quirks.
/// </summary>
public static class DbHelper
{
    /// <summary>
    /// Creates a new SQLite in-memory context. The returned connection must
    /// stay open for the lifetime of the context — closing it destroys the DB.
    /// </summary>
    public static (AppDbContext db, SqliteConnection connection) CreateContext()
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new AppDbContext(options);
        db.Database.EnsureCreated();

        return (db, connection);
    }

    /// <summary>
    /// Seeds a context with a few stores and entries for testing.
    /// </summary>
    public static (List<Store> Stores, List<Entry> Entries) Seed(AppDbContext db)
    {
        var stores = new List<Store>
        {
            new() { Name = "Store Warsaw", City = "Warsaw", Country = "Poland" },
            new() { Name = "Store Berlin", City = "Berlin", Country = "Germany" },
            new() { Name = "Store Paris", City = "Paris", Country = "France" },
        };

        db.Stores.AddRange(stores);
        db.SaveChanges();

        var entries = new List<Entry>
        {
            // Store Warsaw — 3 entries
            new() { IDStore = stores[0].ID, EntryDate = new DateTime(2026, 2, 1, 10, 0, 0) },
            new() { IDStore = stores[0].ID, EntryDate = new DateTime(2026, 2, 2, 11, 0, 0) },
            new() { IDStore = stores[0].ID, EntryDate = new DateTime(2026, 2, 3, 12, 0, 0) },
            // Store Berlin — 2 entries
            new() { IDStore = stores[1].ID, EntryDate = new DateTime(2026, 2, 1, 9, 0, 0) },
            new() { IDStore = stores[1].ID, EntryDate = new DateTime(2026, 2, 5, 14, 0, 0) },
            // Store Paris — 1 entry
            new() { IDStore = stores[2].ID, EntryDate = new DateTime(2026, 2, 3, 8, 0, 0) },
        };

        db.Entries.AddRange(entries);
        db.SaveChanges();

        return (stores, entries);
    }
}
