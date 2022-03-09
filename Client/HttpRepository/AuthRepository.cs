using SharedLibrary.Dtos;
using System.Net.Http.Json;

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