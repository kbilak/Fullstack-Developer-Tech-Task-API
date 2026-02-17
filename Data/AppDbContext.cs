using Data.Models;
using Microsoft.EntityFrameworkCore;

namespace Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Store> Stores { get; set; }
    public DbSet<Entry> Entries { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Store -> Entries relationship
        modelBuilder.Entity<Entry>()
            .HasOne(e => e.Store)
            .WithMany(s => s.Entries)
            .HasForeignKey(e => e.IDStore)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on EntryDate for statistics queries
        modelBuilder.Entity<Entry>()
            .HasIndex(e => e.EntryDate);

        // Composite index for filtering by store and date range
        modelBuilder.Entity<Entry>()
            .HasIndex(e => new { e.IDStore, e.EntryDate });

        // Index on Store.Name for search and sort queries
        modelBuilder.Entity<Store>()
            .HasIndex(s => s.Name);
    }

    public void SeedData()
    {
        if (Stores.Any())
            return;

        var random = new Random(42);

        var cities = new[]
        {
            ("Warsaw", "Poland"),
            ("Krakow", "Poland"),
            ("Gdansk", "Poland"),
            ("Wroclaw", "Poland"),
            ("Poznan", "Poland"),
            ("Berlin", "Germany"),
            ("Munich", "Germany"),
            ("Hamburg", "Germany"),
            ("Prague", "Czech Republic"),
            ("Vienna", "Austria"),
            ("Budapest", "Hungary"),
            ("Bratislava", "Slovakia"),
            ("Amsterdam", "Netherlands"),
            ("Brussels", "Belgium"),
            ("Paris", "France"),
            ("Lyon", "France"),
            ("Madrid", "Spain"),
            ("Barcelona", "Spain"),
            ("Rome", "Italy"),
            ("Milan", "Italy"),
            ("London", "United Kingdom"),
            ("Manchester", "United Kingdom"),
            ("Dublin", "Ireland"),
            ("Stockholm", "Sweden"),
            ("Oslo", "Norway"),
            ("Copenhagen", "Denmark"),
            ("Helsinki", "Finland")
        };

        var stores = new List<Store>();
        for (int i = 0; i < 27; i++)
        {
            var (city, country) = cities[i];
            stores.Add(new Store
            {
                Name = $"Store {city}",
                City = city,
                Country = country
            });
        }

        Stores.AddRange(stores);
        SaveChanges();

        var entries = new List<Entry>();
        var startDate = DateTime.UtcNow.AddDays(-90);

        foreach (var store in stores)
        {
            int entryCount = random.Next(50, 151);

            for (int i = 0; i < entryCount; i++)
            {
                var randomDays = random.Next(0, 90);
                var randomHours = random.Next(8, 21);
                var randomMinutes = random.Next(0, 60);

                entries.Add(new Entry
                {
                    IDStore = store.ID,
                    EntryDate = startDate.AddDays(randomDays).Date.AddHours(randomHours).AddMinutes(randomMinutes)
                });
            }
        }

        Entries.AddRange(entries);
        SaveChanges();
    }
}
