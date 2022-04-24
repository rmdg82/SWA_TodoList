using Api.Models;
using Api.Repositories.Interfaces;

namespace Api.Repositories.Implementations;

public class CosmosUserRepository : IUserRepository
{
    public Task<User> CreateUser(ClientPrincipal clientPrincipal)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUser(string id)
    {
        throw new NotImplementedException();
    }
}