using Api.Models;
using Api.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Repositories.Implementations;

public class CosmosUserRepository : IUserRepository
{
    public Task CreateUser(ClientPrincipal clientPrincipal)
    {
        throw new NotImplementedException();
    }

    public Task<User> GetUser(string id)
    {
        throw new NotImplementedException();
    }
}