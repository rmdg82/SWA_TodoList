using Api.Models;

namespace Api.Repositories.Interfaces;

public interface IUserRepository
{
    Task<User> GetUser(string id);

    Task CreateUser(ClientPrincipal clientPrincipal);
}