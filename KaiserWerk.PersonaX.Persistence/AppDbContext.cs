using KaiserWerk.PersonaX.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace KaiserWerk.PersonaX.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Person> Persons { get; set; }
    public DbSet<Photo> Photos { get; set; }

    public AppDbContext(DbContextOptions options) : base(options) 
    {
        this.Database.EnsureCreated();
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.Entity<Person>(opts =>
        {
            opts.HasKey(p => p.Id);
            opts.Property(p => p.Id).ValueGeneratedOnAdd();
            opts.HasMany(p => p.Photos).WithOne().OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<Photo>(opts => 
        {
            opts.HasKey(p => p.Id);
            opts.Property(p => p.Id).ValueGeneratedOnAdd();
        });
        
    }
}
