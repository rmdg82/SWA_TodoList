using SharedLibrary.Dtos;

namespace Client.HttpRepository.Interfaces;

public interface IAuthHttpRepository
{
    Task<IdentityDto?> GetIdentity();
}