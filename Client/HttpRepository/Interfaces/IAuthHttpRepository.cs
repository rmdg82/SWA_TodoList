using SharedLibrary.Dtos;

namespace Client.HttpRepository.Interfaces;

public interface IAuthHttpRepository
{
    string[] Providers { get; }

    Task<IdentityDto?> GetIdentity();
}