using Client.HttpRepository.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository.Implementations;

public class TestRepository : ITestRepository
{
    private readonly HttpClient _httpClient;
    private const string _endpoint = "api/test";

    public TestRepository(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string?> GetTest()
    {
        var result = await _httpClient.GetAsync(_endpoint);
        if (result is not null && result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }

        return $"Can't reach {_endpoint}";
    }
}