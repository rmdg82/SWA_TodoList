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
        return new OkObjectResult("Hello World");
    }
}