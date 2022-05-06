using Client.Constants;
using Client.HttpRepository.Interfaces;
using SharedLibrary.Dtos;
using System.Net.Http.Json;

namespace Client.HttpRepository.Implementations;

public class AuthHttpRepository : IAuthHttpRepository
{
    private readonly HttpClient _httpClient;

    public AuthHttpRepository(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<IdentityDto?> GetIdentity()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<IdentityDto>(AuthEndpoint.Me);
        }
        catch (Exception)
        {
            return null;
        }
    }
}