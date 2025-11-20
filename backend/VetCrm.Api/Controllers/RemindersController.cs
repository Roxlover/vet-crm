using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Dtos;
using VetCrm.Domain.Entities;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

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
}
