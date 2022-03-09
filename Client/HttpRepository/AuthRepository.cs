using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly HttpClient _httpClient;

        public AuthRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IdentityDto?> GetIdentity()
        {
            return await _httpClient.GetFromJsonAsync<IdentityDto>("/.auth/me");
        }
    }
}