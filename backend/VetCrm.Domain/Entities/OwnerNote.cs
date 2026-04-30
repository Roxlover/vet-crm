using System;

namespace VetCrm.Domain.Entities;

public class OwnerNote
{
    public int Id { get; set; }
    public int OwnerId { get; set; }
    public Owner Owner { get; set; } = null!;

    public string Note { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
