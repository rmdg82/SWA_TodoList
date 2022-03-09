using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository
{
    public interface IAuthRepository
    {
        Task<ClientPrincipalDto?> GetClaimsIdentity();
    }
}