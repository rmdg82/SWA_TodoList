using Api.Repositories.Interfaces;
using Api.Utilities;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

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
    public async Task<IActionResult> TestStuff([HttpTrigger(AuthorizationLevel.Function, "get", Route = "test")] HttpRequest req)
    {
        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal == null)
        {
            return new UnauthorizedResult();
        }

        return new OkObjectResult(clientPrincipal);
    }
}