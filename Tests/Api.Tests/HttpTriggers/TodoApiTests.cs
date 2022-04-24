using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Microsoft.Extensions.Logging;
using AutoMapper;
using Api.MappingProfiles;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FluentAssertions;
using Api.Models;
using System.IO;
using SharedLibrary.Dtos;
using SharedLibrary;
using System.Text.Json;
using Api.HttpTriggers;
using Api.Repositories.Interfaces;
using Api.Exceptions;

namespace Api.Tests.HttpTriggers;

public class TodoApiTests
{
    private const string _notExistingUserId = "notExistingUserId";
    private const string _notExistingTodoId = "notExistingTodoId";

    private readonly string _todoTextNotTooLong = new('*', Validation.maxLengthOnAdd);
    private readonly string _todoTextTooLong = new('*', Validation.maxLengthOnAdd + 1);

    private readonly Mock<ITodoRepository> _mockedTodoRepository;
    private readonly ILogger<TodoApi> _logger;
    private readonly IMapper _mapper;

    private readonly TodoApi _webApi;

    private readonly List<User> _usersOnDb;

    private readonly HeaderInjector _headerInjector;

    public TodoApiTests()
    {
        _mockedTodoRepository = new Mock<ITodoRepository>();
        _logger = new LoggerFactory().CreateLogger<TodoApi>();
        var mapperProfile = new TodoProfile();
        var mapperConf = new MapperConfiguration(cfg => cfg.AddProfile(mapperProfile));
        _mapper = new Mapper(mapperConf);
        _webApi = new TodoApi(_mockedTodoRepository.Object, _logger, _mapper);
        _usersOnDb = new List<User>
        {
            new User
            {
                Id = "1",
                ClientPrincipal = new ClientPrincipal
                {
                    UserId = "1",
                    IdentityProvider = "github",
                    UserDetails = "rmdg82",
                    UserRoles = new [] { "authenticated","admin" }
                },
                Todos = new List<Todo>
                {
                    new Todo
                    {
                        Id = "11",
                        Text = "Test Todo 11",
                        CreatedAt = new DateTime(2022, 1, 1),
                        CompletedAt = new DateTime(2022, 1, 2),
                        IsCompleted = true,
                    },
                    new Todo
                    {
                        Id = "12",
                        Text = "Test Todo 12",
                        CreatedAt = new DateTime(2022, 1, 1),
                        CompletedAt = new DateTime(2022, 1, 2),
                        IsCompleted = false,
                    },
                }
            },
            new User
            {
                Id = "2",
                ClientPrincipal = new ClientPrincipal
                {
                    UserId = "2",
                    IdentityProvider = "aad",
                    UserDetails = "rmdg82",
                    UserRoles = new [] { "authenticated","admin" }
                },
                Todos = new List<Todo>
                {
                    new Todo
                    {
                        Id = "21",
                        Text = "Test Todo 21",
                        CreatedAt = new DateTime(2022, 1, 1),
                        CompletedAt = new DateTime(2022, 1, 2),
                        IsCompleted = true,
                    },
                    new Todo
                    {
                        Id = "22",
                        Text = "Test Todo 22",
                        CreatedAt = new DateTime(2022, 1, 1),
                        CompletedAt = new DateTime(2022, 1, 2),
                        IsCompleted = false,
                    },
                }
            }
        };
        _headerInjector = new HeaderInjector(_usersOnDb.First().ClientPrincipal);

        SetUpMockedRepository(_mockedTodoRepository);

        //_mockedTodoRepository.Setup(x => x.GetByQueryAsync(false)).ReturnsAsync(allTodos);
        //_mockedTodoRepository.Setup(x => x.GetByQueryAsync(true)).ReturnsAsync(allTodos.Where(x => !x.IsCompleted).ToList());
        //_mockedTodoRepository.Setup(x => x.GetByIdAsync(_existingTodoId)).ReturnsAsync(allTodos.FirstOrDefault(x => x.Id.Equals(_existingTodoId)));
        //_mockedTodoRepository.Setup(x => x.GetByIdAsync(_notExistingTodoId)).ReturnsAsync(value: null);
        //_mockedTodoRepository.Setup(x => x.UpdateAsync(_notExistingTodoId, It.IsAny<string>())).Throws(new CosmosException("", System.Net.HttpStatusCode.NotFound, 0, "", 0));
        //_mockedTodoRepository.Setup(x => x.CompleteAsync(_existingTodoId)).Returns(Task.CompletedTask);
        //_mockedTodoRepository.Setup(x => x.CompleteAsync(_notExistingTodoId)).Throws(new KeyNotFoundException());
        //_mockedTodoRepository.Setup(x => x.DeleteAsync(_existingTodoId)).Returns(Task.CompletedTask);
        //_mockedTodoRepository.Setup(x => x.DeleteAsync(_notExistingTodoId)).Throws(new CosmosException("", System.Net.HttpStatusCode.NotFound, 0, "", 0));
    }

    private void SetUpMockedRepository(Mock<ITodoRepository> mockedTodoRepository)
    {
        _mockedTodoRepository.Setup(x => x.ResetDb(_usersOnDb.First().Id)).Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.ResetDb(string.Empty)).Throws<ArgumentException>();

        _mockedTodoRepository.Setup(x => x.GetByQueryAsync(_usersOnDb.First().Id, false))
            .ReturnsAsync(_usersOnDb.First().Todos);
        _mockedTodoRepository.Setup(x => x.GetByQueryAsync(_usersOnDb.First().Id, true))
            .ReturnsAsync(_usersOnDb.First().Todos.Where(x => !x.IsCompleted).ToList());

        _mockedTodoRepository.Setup(x => x.GetByIdAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id))
            .ReturnsAsync(_usersOnDb.First().Todos.FirstOrDefault(x => x.Id.Equals(_usersOnDb.First().Todos.First().Id)));
        _mockedTodoRepository.Setup(x => x.GetByIdAsync(_usersOnDb.First().Id, _notExistingUserId))
            .ReturnsAsync(value: null);

        _mockedTodoRepository.Setup(x => x.AddAsync(_usersOnDb.First().Id, It.IsAny<Todo>()))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.AddAsync(_notExistingUserId, It.IsAny<Todo>()))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));

        _mockedTodoRepository.Setup(x => x.UpdateAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id, _todoTextNotTooLong))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.UpdateAsync(_notExistingUserId, _usersOnDb.First().Todos.First().Id, It.IsAny<string>()))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));
        _mockedTodoRepository.Setup(x => x.UpdateAsync(_usersOnDb.First().Id, _notExistingTodoId, It.IsAny<string>()))
            .ThrowsAsync(new TodoNotFoundException($"Todo {_notExistingTodoId} not found."));

        _mockedTodoRepository.Setup(x => x.CompleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.CompleteAsync(_notExistingUserId, _usersOnDb.First().Todos.First().Id))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));
        _mockedTodoRepository.Setup(x => x.CompleteAsync(_usersOnDb.First().Id, _notExistingTodoId))
            .ThrowsAsync(new TodoNotFoundException($"Todo {_notExistingTodoId} not found."));

        _mockedTodoRepository.Setup(x => x.DeleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.DeleteAsync(_notExistingUserId, _usersOnDb.First().Todos.First().Id))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));
        _mockedTodoRepository.Setup(x => x.DeleteAsync(_usersOnDb.First().Id, _notExistingTodoId))
            .ThrowsAsync(new TodoNotFoundException($"Todo {_notExistingTodoId} not found."));
    }

    [Fact]
    public async Task ResetDb_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post);
        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task ResetDb_ExistingId_CallService()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post);

        _headerInjector.Inject(request);

        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeAssignableTo<OkResult>();
        _mockedTodoRepository.Verify(x => x.ResetDb(_usersOnDb.First().Id), Times.Once());
    }

    [Fact]
    public async Task ResetDb_EmptyId_ThrowArgumentException()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post);

        _headerInjector.Inject(request);

        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeAssignableTo<OkResult>();
        _mockedTodoRepository.Verify(x => x.ResetDb(string.Empty), Times.Never());
    }

    [Fact]
    public async Task GetTodos_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post);
        var actionResult = await _webApi.GetTodos(request);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTodos_WhenCalledWithNoQueryString_ReturnAllTodos()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

        _headerInjector.Inject(request);

        OkObjectResult? result = (await _webApi.GetTodos(request)) as OkObjectResult;

        result.Should().NotBeNull();
        var todos = result!.Value as IEnumerable<TodoDto>;
        todos.Should().NotBeNull();
        todos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodos_WhenCalledWithQueryString_ReturnOnlyUncompleted()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: new QueryString("?onlyUncompleted=true"), body: null);

        _headerInjector.Inject(request);

        OkObjectResult? result = (await _webApi.GetTodos(request)) as OkObjectResult;

        result.Should().NotBeNull();
        var todos = result!.Value as IEnumerable<TodoDto>;
        todos.Should().NotBeNull();
        todos.Should().HaveCount(1);
        todos!.Single().IsCompleted.Should().BeFalse();
    }

    [Fact]
    public async Task GetTodos_WhenCalledWithIncorrectQueryString_ReturnAllTodos()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: new QueryString("?asd123"), body: null);

        _headerInjector.Inject(request);

        OkObjectResult? result = (await _webApi.GetTodos(request)) as OkObjectResult;

        result.Should().NotBeNull();
        var todos = result!.Value as IEnumerable<TodoDto>;
        todos.Should().NotBeNull();
        todos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodoById_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post);
        var actionResult = await _webApi.GetTodoById(request, _usersOnDb.First().Todos.First().Id);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTodoById_WhenCalledWithExistingId_ReturnsTodo()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

        _headerInjector.Inject(request);

        OkObjectResult? result = (await _webApi.GetTodoById(request, _usersOnDb.First().Todos.First().Id)) as OkObjectResult;

        result.Should().NotBeNull();
        var todo = result!.Value as TodoDto;
        todo.Should().NotBeNull();
        todo!.Id.Should().Be(_usersOnDb.First().Todos.First().Id);
        todo!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetTodoById_WhenCalledWithNotExistingId_ReturnsNotFound()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

        _headerInjector.Inject(request);

        var result = (await _webApi.GetTodoById(request, _notExistingUserId));

        result.Should().NotBeNull();
        result.Should().BeAssignableTo<NotFoundResult>();
    }

    [Fact]
    public async Task AddTodos_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        var actionResult = await _webApi.AddTodo(request);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task AddTodo_TodoTextTooLong_ReturnBadRequest()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        _headerInjector.Inject(request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddTodo_TodoTextNotTooLong_ReturnCreatedAtRoute()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        _headerInjector.Inject(request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeAssignableTo<CreatedAtRouteResult>();
        _mockedTodoRepository.Verify(x => x.AddAsync(_usersOnDb.First().Id, It.IsAny<Todo>()), Times.Once());
    }

    [Fact]
    public async Task AddTodo_TodoNull_ReturnBadRequest()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

        _headerInjector.Inject(request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
    }

    [Fact]
    public async Task AddTodo_NotExistingUserIdThrowException_ReturnBadRequest()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        // Inject a not existing user
        var headerInjector = new HeaderInjector(new ClientPrincipal() { UserId = _notExistingUserId });
        headerInjector.Inject(request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.AddAsync(_notExistingUserId, It.IsAny<Todo>()), Times.Once());
    }

    [Fact]
    public async Task AddTodo_GenericException_ReturnBadRequestResult()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));
        _headerInjector.Inject(request);
        _mockedTodoRepository.Setup(x => x.AddAsync(_usersOnDb.First().Id, It.IsAny<Todo>())).Throws(new Exception());

        IActionResult result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.AddAsync(_usersOnDb.First().Id, It.IsAny<Todo>()), Times.Once());
    }

    [Fact]
    public async Task UpdateTodos_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        var actionResult = await _webApi.UpdateTodo(request, _usersOnDb.First().Todos.First().Id);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task UpdateTodo_TodoTextTooLong_ReturnBadRequest()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        _headerInjector.Inject(request);

        var result = await _webApi.UpdateTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateTodo_TodoTextNotTooLong_ReturnNoContent()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        _headerInjector.Inject(request);

        var result = await _webApi.UpdateTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<NoContentResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id, _todoTextNotTooLong), Times.Once());
    }

    [Fact]
    public async Task UpdateTodo_TodoNull_ReturnBadRequest()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: null);

        _headerInjector.Inject(request);

        var result = await _webApi.UpdateTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
    }

    [Fact]
    public async Task UpdateTodo_UserNotFoundThrowException_ReturnNotFound()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        // Inject a not existing user
        var headerInjector = new HeaderInjector(new ClientPrincipal() { UserId = _notExistingUserId });
        headerInjector.Inject(request);

        var result = await _webApi.UpdateTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_notExistingUserId, _usersOnDb.First().Todos.First().Id, _todoTextNotTooLong), Times.Once());
    }

    [Fact]
    public async Task UpdateTodo_TodoIdNotFoundThrowException_ReturnNotFound()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        _headerInjector.Inject(request);

        var result = await _webApi.UpdateTodo(request, _notExistingTodoId);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_usersOnDb.First().Id, _notExistingTodoId, _todoTextNotTooLong), Times.Once());
    }

    [Fact]
    public async Task UpdateTodo_GenericException_ReturnBadRequest()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));
        _mockedTodoRepository.Setup(x => x.UpdateAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id, It.IsAny<string>())).Throws(new Exception());
        _headerInjector.Inject(request);

        var result = await _webApi.UpdateTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id, It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task CompleteToDo_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

        var actionResult = await _webApi.CompleteTodo(request, _usersOnDb.First().Todos.First().Id);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task CompleteTodo_WhenCalledOnExistingTodoId_ReturnOkResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        _headerInjector.Inject(request);

        var result = await _webApi.CompleteTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<OkResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id), Times.Once());
    }

    [Fact]
    public async Task CompleteTodo_WhenCalledOnNotExistingUser_ReturnNotFound()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        // Inject a not existing user
        var headerInjector = new HeaderInjector(new ClientPrincipal() { UserId = _notExistingUserId });
        headerInjector.Inject(request);

        var result = await _webApi.CompleteTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_notExistingUserId, _usersOnDb.First().Todos.First().Id), Times.Once());
    }

    [Fact]
    public async Task CompleteTodo_WhenCalledOnNotExistingTodo_ReturnNotFound()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        _headerInjector.Inject(request);

        var result = await _webApi.CompleteTodo(request, _notExistingTodoId);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_usersOnDb.First().Id, _notExistingTodoId), Times.Once());
    }

    [Fact]
    public async Task CompleteTodo_GenericException_ReturnBadRequestObjectResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        _mockedTodoRepository.Setup(x => x.CompleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id)).Throws(new Exception());
        _headerInjector.Inject(request);

        var result = await _webApi.CompleteTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);

        var actionResult = await _webApi.DeleteTodo(request, _usersOnDb.First().Todos.First().Id);

        actionResult.Should().BeAssignableTo<UnauthorizedResult>();
    }

    [Fact]
    public async Task DeleteTodo_WhenCalledOnNotExistingUser_ReturnNotFound()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);
        // Inject a not existing user
        var headerInjector = new HeaderInjector(new ClientPrincipal() { UserId = _notExistingUserId });
        headerInjector.Inject(request);

        var result = await _webApi.DeleteTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_notExistingUserId, _usersOnDb.First().Todos.First().Id), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_WhenCalledOnNotExistingTodo_ReturnNotFound()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);
        _headerInjector.Inject(request);

        var result = await _webApi.DeleteTodo(request, _notExistingTodoId);

        result.Should().NotBeNull().And.BeAssignableTo<NotFoundResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_usersOnDb.First().Id, _notExistingTodoId), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_WhenCalledOnExistingTodo_ReturnNoContentResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        _headerInjector.Inject(request);

        IActionResult result = await _webApi.DeleteTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<NoContentResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_GenericException_ReturnBadRequestResult()
    {
        HttpRequest request = CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        _headerInjector.Inject(request);
        _mockedTodoRepository.Setup(x => x.DeleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id)).Throws(new Exception());

        IActionResult result = await _webApi.DeleteTodo(request, _usersOnDb.First().Todos.First().Id);

        result.Should().NotBeNull().And.BeAssignableTo<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_usersOnDb.First().Id, _usersOnDb.First().Todos.First().Id), Times.Once());
    }

    private static HttpRequest CreateHttpRequest(string method, QueryString? queryStrings = null, string? body = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = method;
        request.QueryString = queryStrings ?? QueryString.Empty;
        if (body != null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            request.Body = new MemoryStream(bytes);
        }

        return request;
    }
}