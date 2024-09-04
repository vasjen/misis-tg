using Microsoft.EntityFrameworkCore;
using misis_tg.Models;

namespace misis_tg.Data;

public class AppDbContext : DbContext
{
    public AppDbContext( DbContextOptions<AppDbContext> options) : base(options)
    {
        
    }
    public DbSet<Education> Educations { get; set; }
    public DbSet<Student> Students { get; set; }
    public DbSet<Subscriber> Subscribers { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Education>().HasIndex(p => p.Code).IsUnique();
        modelBuilder.Entity<Student>().HasIndex(p => p.RegistrationNumber).IsUnique();
        modelBuilder.Entity<Subscriber>().HasIndex(p => p.RegistrationNumber);
    }
    
}