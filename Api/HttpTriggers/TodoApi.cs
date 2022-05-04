using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Api.Models;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using System.Net;
using SharedLibrary.Dtos;
using Api.Repositories.Interfaces;
using Api.Utilities;
using Api.Exceptions;
using SharedLibrary.Validators;

namespace Api.HttpTriggers;

public class TodoApi
{
    private readonly ITodoRepository _todoRepository;
    private readonly ILogger<TodoApi> _logger;
    private readonly IMapper _mapper;

    public TodoApi(ITodoRepository todoRepository, ILogger<TodoApi> logger, IMapper mapper)
    {
        _todoRepository = todoRepository;
        _logger = logger;
        _mapper = mapper;
    }

    [FunctionName(nameof(ResetDb))]
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

    [FunctionName(nameof(GetTodos))]
    public async Task<IActionResult> GetTodos([HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos")] HttpRequest req)
    {
        var queryParams = req.QueryString;
        var getOnlyUncompleted = req.Query["onlyUncompleted"];
        _logger.LogInformation($"New request for {nameof(GetTodos)} with querystring [{queryParams}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        var todos = new List<Todo>();

        bool onlyUncompleted = Convert.ToBoolean(getOnlyUncompleted);
        try
        {
            if (onlyUncompleted)
            {
                todos = (await _todoRepository.GetByQueryAsync(clientPrincipal.UserId, getOnlyUncompleted: true)).ToList();
            }
            else
            {
                todos = (await _todoRepository.GetByQueryAsync(clientPrincipal.UserId)).ToList();
            }
        }
        catch (UserNotFoundException)
        {
            _logger.LogError($"User {clientPrincipal.UserId} not found. Return empty list.");
            return new OkObjectResult(new List<TodoDto>());
        }

        var todosDto = _mapper.Map<IEnumerable<TodoDto>>(todos);
        return new OkObjectResult(todosDto);
    }

    [FunctionName(nameof(GetTodoById))]
    public async Task<IActionResult> GetTodoById([HttpTrigger(AuthorizationLevel.Function, "get", Route = "todos/{todoId}")] HttpRequest req, string todoId)
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

    [FunctionName(nameof(AddTodo))]
    public async Task<IActionResult> AddTodo([HttpTrigger(AuthorizationLevel.Function, "post", Route = "todos")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"New request for {nameof(AddTodo)} with body [{requestBody}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        TodoDtoToAdd? todoToAddDto;
        try
        {
            todoToAddDto = JsonSerializer.Deserialize<TodoDtoToAdd>(requestBody);
        }
        catch (Exception)
        {
            _logger.LogError($"I could not parse the request body {requestBody}.");
            return new BadRequestResult();
        }

        if (todoToAddDto is null)
        {
            _logger.LogError($"Parses request body is null.");
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
        catch (UserNotFoundException)
        {
            _logger.LogError($"User {clientPrincipal.UserId} not found.");
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestResult();
        }

        _logger.LogInformation($"Added Todo with id: {todoToAdd.Id} text: {todoToAdd.Text}");
        return new CreatedAtRouteResult(new { todoId = todoToAdd.Id }, todoToAdd);
    }

    [FunctionName(nameof(UpdateTodo))]
    public async Task<IActionResult> UpdateTodo([HttpTrigger(AuthorizationLevel.Function, "put", Route = "todos/{todoId}")] HttpRequest req, string todoId)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"New request for {nameof(UpdateTodo)} with id [{todoId}] and body [{requestBody}].");

        var clientPrincipal = HttpRequestParser.ParseToClientPrincipal(req);
        if (clientPrincipal is null)
        {
            return new UnauthorizedResult();
        }

        TodoDtoToUpdate? todoToUpdateDto;
        try
        {
            todoToUpdateDto = JsonSerializer.Deserialize<TodoDtoToUpdate>(requestBody);
        }
        catch (Exception)
        {
            _logger.LogError($"I could not parse the request body {requestBody}.");
            return new BadRequestResult();
        }

        if (todoToUpdateDto is null)
        {
            _logger.LogError($"Parses request body is null.");
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
        catch (UserNotFoundException)
        {
            _logger.LogError($"User {clientPrincipal.UserId} not found.");
            return new NotFoundResult();
        }
        catch (TodoNotFoundException)
        {
            _logger.LogError($"Todo with id {todoId} not found.");
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestResult();
        }

        return new NoContentResult();
    }

    [FunctionName(nameof(CompleteTodo))]
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
        catch (UserNotFoundException)
        {
            _logger.LogError($"User {clientPrincipal.UserId} not found.");
            return new NotFoundResult();
        }
        catch (TodoNotFoundException)
        {
            _logger.LogError($"Todo with id {todoId} not found.");
            return new NotFoundResult();
        }
        catch (Exception ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new BadRequestResult();
        }

        return new OkResult();
    }

    [FunctionName(nameof(DeleteTodo))]
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
        catch (UserNotFoundException ex)
        {
            _logger.LogError($"Catched exception: [{ex.Message}]");
            return new NotFoundResult();
        }
        catch (TodoNotFoundException ex)
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