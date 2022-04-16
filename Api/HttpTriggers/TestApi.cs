using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using System.Text.Json;
using SharedLibrary.Dtos;
using System.Text;
using Api.Repositories.Interfaces;
using System.Security.Claims;
using Api.Models;

namespace Api.HttpTriggers;

public class TestApi
{
    private readonly ITodoRepository _todoRepository;
    private readonly ILogger<TodoApi> _logger;
    private readonly IMapper _mapper;

    public TestApi(ITodoRepository todoRepository, ILogger<TodoApi> logger, IMapper mapper)
    {
        _todoRepository = todoRepository ?? throw new ArgumentNullException(nameof(todoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [FunctionName("TestApi")]
    public async Task<IActionResult> PrintHelloWorld([HttpTrigger(AuthorizationLevel.Function, "get", Route = "hello")] HttpRequest req)
    {
        var claimsPrincipal = Parse(req);
        if (claimsPrincipal == null)
        {
            return new UnauthorizedResult();
        }
        var userId = claimsPrincipal.FindFirst(ClaimTypes.NameIdentifier).Value;
        var userDetails = claimsPrincipal.FindFirst(ClaimTypes.Name).Value;

        return new OkObjectResult($"UserId: {userId}, UserDetails: {userDetails}");
    }

    private static ClaimsPrincipal Parse(HttpRequest req)
    {
        var principal = new ClientPrincipal();

        if (req.Headers.TryGetValue("x-ms-client-principal", out var header))
        {
            var data = header[0];
            var decoded = Convert.FromBase64String(data);
            var json = Encoding.UTF8.GetString(decoded);
            principal = JsonSerializer.Deserialize<ClientPrincipal>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        principal.UserRoles = principal.UserRoles?.Except(new string[] { "anonymous" }, StringComparer.CurrentCultureIgnoreCase);

        if (!principal.UserRoles?.Any() ?? true)
        {
            return new ClaimsPrincipal();
        }

        var identity = new ClaimsIdentity(principal.IdentityProvider);
        identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, principal.UserId));
        identity.AddClaim(new Claim(ClaimTypes.Name, principal.UserDetails));
        identity.AddClaims(principal.UserRoles.Select(r => new Claim(ClaimTypes.Role, r)));

        return new ClaimsPrincipal(identity);
    }
}