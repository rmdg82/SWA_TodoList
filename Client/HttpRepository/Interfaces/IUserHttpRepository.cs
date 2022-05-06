using SharedLibrary.Dtos;

namespace Client.HttpRepository.Interfaces;

public interface IUserHttpRepository
{
    Task<UserDto?> GetUser(string userId);

    Task<UserDto?> CreateUser(ClientPrincipalDto clientPrincipalDto);
}