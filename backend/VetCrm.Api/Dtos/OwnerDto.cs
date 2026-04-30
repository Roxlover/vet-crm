namespace VetCrm.Api.Dtos;

public class OwnerDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string PhoneE164 { get; set; } = null!;
    public string? Email { get; set; }
    public string? Address { get; set; }
    public bool KvkkOptIn { get; set; }

    public int PetCount { get; set; }

    public List<OwnerPetFullDto> Pets { get; set; } = new();
    public List<OwnerNoteDto> Notes { get; set; } = new();

    public static (int years, int months)? CalcAge(DateOnly? birthDate)
    {
        if (birthDate is null) return null;

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var years = today.Year - birthDate.Value.Year;
        var months = today.Month - birthDate.Value.Month;

        if (today.Day < birthDate.Value.Day)
            months--;

        if (months < 0)
        {
            years--;
            months += 12;
        }

        if (years < 0) years = 0;
        if (months < 0) months = 0;

        return (years, months);
    }
}

public class OwnerPetFullDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string? Species { get; set; }
    public string? Breed { get; set; }
    public DateOnly? BirthDate { get; set; }

    public int? AgeYears { get; set; }
    public int? AgeMonths { get; set; }
}

public class OwnerNoteDto
{
    public int Id { get; set; }
    public string Note { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

public class AddOwnerNoteRequest
{
    public string Note { get; set; } = null!;
}
