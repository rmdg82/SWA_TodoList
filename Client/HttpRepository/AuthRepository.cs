using SharedLibrary.Dtos;
using System.Net.Http.Json;

namespace Client.HttpRepository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly HttpClient _httpClient;
        public string[] Providers { get; } = { "aad", "github", "twitter" };

        public AuthRepository(HttpClient httpClient)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

        public async Task<IdentityDto?> GetIdentity()
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<IdentityDto>("/.auth/me");
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}