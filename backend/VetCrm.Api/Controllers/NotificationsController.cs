using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Api.Services;
using VetCrm.Infrastructure.Data;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly VetCrmDbContext _db;
    private readonly ICurrentUserService _currentUser;

    public NotificationsController(VetCrmDbContext db, ICurrentUserService currentUser)
    {
        _db = db;
        _currentUser = currentUser;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetMyNotifications()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var list = await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Take(50)
            .ToListAsync();

        var dto = list.Select(n => new NotificationDto
        {
            Id = n.Id,
            Message = n.Message,
            CreatedAt = n.CreatedAt,
            IsRead = n.IsRead
        }).ToList();

        return Ok(dto);
    }

    [HttpPost("read")]
    public async Task<IActionResult> MarkAllRead()
    {
        var userId = _currentUser.UserId;
        if (userId == null)
            return Unauthorized();

        var list = await _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ToListAsync();

        foreach (var n in list)
            n.IsRead = true;

        await _db.SaveChangesAsync();
        return NoContent();
    }
}

public class NotificationDto
{
    public int Id { get; set; }
    public string Message { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public bool IsRead { get; set; }
}
