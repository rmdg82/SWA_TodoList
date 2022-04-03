using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.HttpRepository;

public class TestRepository : ITestRepository
{
    private readonly HttpClient _httpClient;

    public TestRepository(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string?> GetHelloWorld()
    {
        var result = await _httpClient.GetAsync("/api/hello");
        if (result is not null && result.IsSuccessStatusCode)
        {
            return await result.Content.ReadAsStringAsync();
        }

        return "Can't get hello world!";
    }
}