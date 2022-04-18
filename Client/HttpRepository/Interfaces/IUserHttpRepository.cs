using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository.Interfaces;

public interface IUserHttpRepository
{
    Task<UserDto?> GetUser(string userId);

    Task CreateUser(ClientPrincipalDto clientPrincipalDto);
}