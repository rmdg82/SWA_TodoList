using SharedLibrary.Dtos;

namespace Client.HttpRepository
{
    public interface IAuthRepository
    {
        Task<IdentityDto?> GetIdentity();
    }
}