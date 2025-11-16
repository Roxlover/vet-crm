using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;

namespace VetCrm.Infrastructure.Data;

public class VetCrmDbContext : DbContext
{
    public VetCrmDbContext(DbContextOptions<VetCrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Owner> Owners => Set<Owner>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<Visit> Visits => Set<Visit>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Owner>(b =>
        {
            b.Property(o => o.FullName).IsRequired().HasMaxLength(120);
            b.Property(o => o.PhoneE164).IsRequired().HasMaxLength(20);
        });

        modelBuilder.Entity<Pet>(b =>
        {
            b.Property(p => p.Name).IsRequired().HasMaxLength(80);
            b.HasOne(p => p.Owner)
                .WithMany(o => o.Pets)
                .HasForeignKey(p => p.OwnerId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Visit>(b =>
        {
            b.HasOne(v => v.Pet)
                .WithMany(p => p.Visits)
                .HasForeignKey(v => v.PetId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Reminder>(b =>
        {
            b.HasOne(r => r.Visit)
                .WithMany(v => v.Reminders)
                .HasForeignKey(r => r.VisitId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
