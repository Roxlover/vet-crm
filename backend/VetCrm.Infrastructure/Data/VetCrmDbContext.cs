using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;

namespace VetCrm.Infrastructure.Data;

public class VetCrmDbContext : DbContext
{
    public VetCrmDbContext(DbContextOptions<VetCrmDbContext> options)
        : base(options)
    {
    }

    public DbSet<Owner> Owners { get; set; } = null!;
    public DbSet<Pet> Pets { get; set; } = null!;
    public DbSet<Visit> Visits { get; set; } = null!;
    public DbSet<VisitImage> VisitImages { get; set; } = null!;
    public DbSet<Reminder> Reminders { get; set; } = null!;
    public DbSet<User> Users { get; set; } = null!;
    public DbSet<LedgerEntry> LedgerEntries { get; set; } = null!;

    public DbSet<Notification> Notifications { get; set; } = null!;


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

            // 🔴 Doktor ilişkisi (Visit.DoctorId, Visit.Doctor)
            b.HasOne(v => v.Doctor)
                .WithMany(d => d.Visits)
                .HasForeignKey(v => v.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);
            b.HasOne(v => v.CreatedByUser)
                .WithMany()
                .HasForeignKey(v => v.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);   
        });

        modelBuilder.Entity<Reminder>(b =>
        {
            b.HasOne(r => r.Visit)
                // Visit tarafında Reminders koleksiyonu TANIMLI DEĞİL, o yüzden:
                .WithMany() // <--- v => v.Reminders DEĞİL
                .HasForeignKey(r => r.VisitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LedgerEntry>(b =>
            {
                b.Property(x => x.Date).IsRequired();
                b.Property(x => x.Amount).IsRequired();
                b.Property(x => x.IsIncome).IsRequired();

                // CreatedAt için veritabanı default’u da tanımlayalım (ek güvenlik)
                b.Property(x => x.CreatedAt)
                    .HasDefaultValueSql("NOW()");
            });
        
        modelBuilder.Entity<User>(b =>
        {
            b.Property(u => u.FullName).IsRequired().HasMaxLength(120);
            b.Property(u => u.Username).IsRequired().HasMaxLength(50);
            b.Property(u => u.Email).HasMaxLength(120);

            b.HasIndex(u => u.Username).IsUnique(); // aynı username bir daha olmasın
        });

        modelBuilder.Entity<Notification>(b =>
        {
            b.Property(n => n.Type).IsRequired().HasMaxLength(50);
            b.Property(n => n.Message).IsRequired().HasMaxLength(500);
            b.HasOne(n => n.User)
                .WithMany() 
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
        
        modelBuilder.Entity<VisitImage>(b =>
        {
            b.HasKey(x => x.Id);

            b.Property(x => x.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            b.HasOne(x => x.Visit)
                .WithMany(v => v.Images)
                .HasForeignKey(x => x.VisitId)
                .OnDelete(DeleteBehavior.Cascade);
        });

    }
}
