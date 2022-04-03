using SharedLibrary.Dtos;

namespace Client.HttpRepository;

public interface IAuthRepository
{
    string[] Providers { get; }

    Task<IdentityDto?> GetIdentity();
}