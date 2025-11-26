using VetCrm.Domain.Entities;

namespace VetCrm.Api.Services;

public interface ITokenService
{
    string CreateToken(User user);
}
