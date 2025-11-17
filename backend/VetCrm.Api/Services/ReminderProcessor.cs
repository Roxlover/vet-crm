using Microsoft.EntityFrameworkCore;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;


namespace VetCrm.Api.Services;

public class ReminderProcessor
{
    private readonly VetCrmDbContext _db;

    public ReminderProcessor(VetCrmDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// Bugüne ait ve Pending durumdaki reminder'ları işler.
    /// Şimdilik sadece console'a log yazar ve status'ü Sent yapar.
    /// </summary>
    public async Task ProcessDueRemindersAsync()
    {
        var today = DateOnly.FromDateTime(DateTime.Now.Date);

        var reminders = await _db.Reminders
            .Include(r => r.Visit)
                .ThenInclude(v => v.Pet)
                    .ThenInclude(p => p.Owner)
            .Where(r => r.DueDate == today && r.Status == ReminderStatus.Pending)
            .ToListAsync();

        if (!reminders.Any())
        {
            Console.WriteLine($"[REMINDER] {today}: işlenecek kayıt yok.");
            return;
        }

        Console.WriteLine($"[REMINDER] {today}: {reminders.Count} kayıt işlenecek.");

        foreach (var r in reminders)
        {
            var owner = r.Visit.Pet.Owner;
            var pet = r.Visit.Pet;

            // Şimdilik sadece log yazıyoruz
            Console.WriteLine(
                $"[REMINDER] {owner.FullName} ({owner.PhoneE164}) - " +
                $"{pet.Name} için {r.DueDate} tarihinde hatırlatma. " +
                $"İşlem: {r.Visit.Procedures}");

            r.Status = ReminderStatus.Sent;
            r.SentAt = DateTime.UtcNow;  // 🔴 ÖNEMLİ: Local değil, UTC

        }

        await _db.SaveChangesAsync();
    }
}
