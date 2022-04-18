using Client.HttpRepository.Interfaces;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository.Implementations;

public class UserHttpRepository : IUserHttpRepository
{
    private readonly HttpClient _httpClient;

    public UserHttpRepository(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task CreateUser(ClientPrincipalDto clientPrincipalDto)
    {
        if (clientPrincipalDto is null)
        {
            throw new ArgumentNullException(nameof(clientPrincipalDto));
        }

        await _httpClient.PostAsJsonAsync("api/users", clientPrincipalDto);
    }

    public async Task<UserDto?> GetUser(string userId)
    {
        if (string.IsNullOrWhiteSpace(userId))
        {
            throw new ArgumentException($"'{nameof(userId)}' cannot be null or whitespace.", nameof(userId));
        }

        var response = await _httpClient.GetAsync($"api/users/{userId}");

        if (response.IsSuccessStatusCode)
        {
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }

        return null;
    }
}