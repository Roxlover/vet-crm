namespace VetCrm.Api.Services;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Role { get; }
    string? FullName { get; }
}
