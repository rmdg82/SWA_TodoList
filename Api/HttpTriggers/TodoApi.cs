using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Api.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Net;
using Api.Validators;
using SharedLibrary.Dtos;
using Api.Repositories.Interfaces;
using System.Text;
using System.Security.Claims;
using Api.Utilities;

namespace Api.HttpTriggers;

public class TodoApi
{
    private readonly ITodoRepository _todoRepository;
    private readonly ILogger<TodoApi> _logger;
    private readonly IMapper _mapper;

    public TodoApi(ITodoRepository todoRepository, ILogger<TodoApi> logger, IMapper mapper)
    {
        _todoRepository = todoRepository ?? throw new ArgumentNullException(nameof(todoRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    [FunctionName("ResetDb")]
    public async Task<IActionResult> ResetDb([HttpTrigger(AuthorizationLevel.Function, "post", Route = "resetDb")] HttpRequest req)
    {
        _logger.LogInformation("Reset Db with fake data.");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        await _todoRepository.ResetDb(clientPrincipal.UserId);
        return new OkResult();
    }

    [FunctionName("GetAllTodos")]
    public async Task<ActionResult<IEnumerable<TodoDto>>> GetTodos([HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos")] HttpRequest req)
    {
        var queryParams = req.QueryString;
        var getOnlyUncompleted = req.Query["onlyUncompleted"];
        _logger.LogInformation($"New request for {nameof(GetTodos)} with querystring [{queryParams}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal?.UserId is null)
        {
            //return new UnauthorizedResult();
            clientPrincipal = new ClientPrincipal
            {
                UserId = "16578f990d2329166d4e136248dc141f"
            };
        }

        var todos = new List<Todo>();
        try
        {
            bool onlyUncompleted = Convert.ToBoolean(getOnlyUncompleted);

            if (onlyUncompleted)
            {
                todos = (await _todoRepository.GetByQueryAsync(clientPrincipal.UserId, getOnlyUncompleted: true)).ToList();
            }
            else
            {
                todos = (await _todoRepository.GetByQueryAsync(clientPrincipal.UserId)).ToList();
            }
        }
        catch (FormatException ex)
        {
            todos = (await _todoRepository.GetByQueryAsync(clientPrincipal.UserId)).ToList();
            _logger.LogInformation(ex.Message);
        }

        var todosDto = _mapper.Map<IEnumerable<TodoDto>>(todos);
        return new OkObjectResult(todosDto);
    }

    [FunctionName("GetTodoById")]
    public async Task<ActionResult<TodoDto>> GetTodoById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos/{todoId}")] HttpRequest req, string todoId)
    {
        _logger.LogInformation($"New request for {nameof(GetTodoById)} with id [{todoId}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        var todo = await _todoRepository.GetByIdAsync(clientPrincipal.UserId, todoId);

        if (todo is null)
        {
            _logger.LogError($"Not found todo for {nameof(GetTodoById)} with id [{todoId}].");
            return new NotFoundResult();
        }

        return new OkObjectResult(_mapper.Map<TodoDto>(todo));
    }

    [FunctionName("AddTodo")]
    public async Task<IActionResult> AddTodo([HttpTrigger(AuthorizationLevel.Function, "post", Route = "todos")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"New request for {nameof(AddTodo)} with body [{requestBody}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        var todoToAddDto = JsonSerializer.Deserialize<TodoDtoToAdd>(requestBody);

        if (todoToAddDto is null)
        {
            _logger.LogError($"Invalid request body for {nameof(AddTodo)}.");
            return new BadRequestResult();
        }

        var validator = new TodoDtoToAddValidator();
        var validationResult = validator.Validate(todoToAddDto);
        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(
                validationResult.Errors.Select(x => new
                {
                    Field = x.PropertyName,
                    Error = x.ErrorMessage
                }));
        }

        var todoToAdd = _mapper.Map<Todo>(todoToAddDto);
        todoToAdd.Id = Guid.NewGuid().ToString();

        try
        {
            await _todoRepository.AddAsync(clientPrincipal.UserId, todoToAdd);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestResult();
        }

        _logger.LogInformation($"Added Todo with id: {todoToAdd.Id} text: {todoToAdd.Text}");
        return new CreatedAtRouteResult(new { todoId = todoToAdd.Id }, todoToAdd);
    }

    [FunctionName("UpdateTodo")]
    public async Task<IActionResult> UpdateTodo([HttpTrigger(AuthorizationLevel.Function, "put", Route = "todos/{todoId}")] HttpRequest req, string todoId)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"New request for {nameof(UpdateTodo)} with id [{todoId}] and body [{requestBody}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        var todoToUpdateDto = JsonSerializer.Deserialize<TodoDtoToUpdate>(requestBody);

        if (todoToUpdateDto is null)
        {
            _logger.LogError($"Invalid request body for {nameof(UpdateTodo)}.");
            return new BadRequestResult();
        }

        var validator = new TodoDtoToUpdateValidator();
        var validationResult = validator.Validate(todoToUpdateDto);
        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(
                validationResult.Errors.Select(x => new
                {
                    Field = x.PropertyName,
                    Error = x.ErrorMessage
                }));
        }

        try
        {
            await _todoRepository.UpdateAsync(clientPrincipal.UserId, todoId, todoToUpdateDto.Text);
        }
        catch (CosmosException ex) when (ex.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new NotFoundResult();
        }
        catch (CosmosException ex) when (ex.StatusCode.Equals(HttpStatusCode.BadRequest))
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestResult();
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new NotFoundObjectResult(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestObjectResult(ex.Message);
        }

        return new NoContentResult();
    }

    [FunctionName("CompleteTodo")]
    public async Task<IActionResult> CompleteTodo([HttpTrigger(AuthorizationLevel.Function, "post", Route = "todos/{todoId}/complete")] HttpRequest req, string todoId)
    {
        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        try
        {
            _logger.LogInformation($"Request complete todo with id [{todoId}]");
            await _todoRepository.CompleteAsync(clientPrincipal.UserId, todoId);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestObjectResult(ex.Message);
        }

        return new OkResult();
    }

    [FunctionName("DeleteTodo")]
    public async Task<IActionResult> DeleteTodo([HttpTrigger(AuthorizationLevel.Function, "delete", Route = "todos/{todoId}")] HttpRequest req, string todoId)
    {
        _logger.LogInformation($"New request for {nameof(DeleteTodo)} with id [{todoId}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        try
        {
            await _todoRepository.DeleteAsync(clientPrincipal.UserId, todoId);
        }
        catch (CosmosException ex) when (ex.StatusCode.Equals(HttpStatusCode.NotFound))
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestResult();
        }

        return new NoContentResult();
    }
}