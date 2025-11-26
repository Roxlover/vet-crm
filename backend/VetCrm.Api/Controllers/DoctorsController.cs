using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VetCrm.Infrastructure.Data;
using VetCrm.Domain.Entities;
using Microsoft.AspNetCore.Authorization;

namespace VetCrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class DoctorsController : ControllerBase
{
    private readonly VetCrmDbContext _db;

    public DoctorsController(VetCrmDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<DoctorDto>>> Get()
    {
        var doctors = await _db.Users
            .Where(u => u.Role == "Doctor")
            .OrderBy(u => u.FullName)
            .Select(u => new DoctorDto
            {
                Id = u.Id,
                FullName = u.FullName
            })
            .ToListAsync();

        return Ok(doctors);
    }
}

public class DoctorDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
}
