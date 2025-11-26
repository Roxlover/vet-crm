using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class RemindersController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public RemindersController(VetCrmDbContext db)
    {
        _db = db;
    }

 [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateReminderStatusRequest request)
    {
        var reminder = await _db.Reminders.FirstOrDefaultAsync(r => r.Id == id);
        if (reminder == null)
            return NotFound();

        var today = DateOnly.FromDateTime(DateTime.Today);

        if (request.Completed)
        {
            reminder.IsCompleted = true;
            reminder.CompletedAt = DateTime.UtcNow;
        }
        else
        {
            reminder.IsCompleted = false;

            if (request.MarkAsOverdue && reminder.DueDate >= today)
            {
                // bugünden sonrayı "geciken"e çekmek için:
                reminder.DueDate = today.AddDays(-1);
            }
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
[HttpPatch("{id:int}/credit")]
public async Task<IActionResult> UpdateCredit(
    int id,
    [FromBody] UpdateReminderCreditDto dto)
{
    Console.WriteLine($"[UpdateCredit] id={id}, amount={dto.CreditAmountTl}");

    var reminder = await _db.Reminders
        .Include(r => r.Visit)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (reminder == null || reminder.Visit == null)
        return NotFound();

    reminder.Visit.CreditAmountTl = dto.CreditAmountTl;
    await _db.SaveChangesAsync();

    Console.WriteLine($"[UpdateCredit] VISIT {reminder.VisitId} now has credit={reminder.Visit.CreditAmountTl}");

    return NoContent();
}


}

public class UpdateReminderCreditDto
    {
        public decimal CreditAmountTl { get; set; }
    }