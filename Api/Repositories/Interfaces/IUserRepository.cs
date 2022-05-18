using Api.Models;

namespace Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User?> GetUser(string id);

    Task<User?> CreateUser(ClientPrincipal clientPrincipal);
}