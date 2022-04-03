using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Api.Repositories;
using AutoMapper;
using System.Text.Json;
using SharedLibrary.Dtos;
using System.Text;

namespace Api;

public class TestApi
{
    private readonly ITodoRepository _todoRepository;
    private readonly ILogger<WebApi> _logger;
    private readonly IMapper _mapper;

    public TestApi(ITodoRepository todoRepository, ILogger<WebApi> logger, IMapper mapper)
    {
        _todoRepository = todoRepository ?? throw new ArgumentNullException(nameof(todoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [FunctionName("identity")]
    public async Task<IActionResult> GetIdentity(
        [HttpTrigger(AuthorizationLevel.Function, "get", Route = "hello")] HttpRequest req)
    {
        _logger.LogInformation("GetIdentity called!");
        var result = Parse(req);

        return new OkObjectResult(result);
    }

    public static IdentityDto Parse(HttpRequest req)
    {
        var principal = new IdentityDto();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<IdentityDto>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        return principal;

        //principal.ClientPrincipal.UserRoles = principal.ClientPrincipal?.UserRoles.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

        //if (!principal.UserRoles?.Any() ?? true)
        //{
        //    return new ClaimsPrincipal();
        //}

        //var identity = new ClaimsIdentity(principal.IdentityProvider);
        //identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
        //identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
        //identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

        //return new ClaimsPrincipal(identity);
    }
}