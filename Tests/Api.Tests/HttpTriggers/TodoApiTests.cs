﻿using Api.Exceptions;
using Api.HttpTriggers;
using Api.MappingProfiles;
using Api.Models;
using Api.Repositories.Interfaces;
using Api.Tests.Utils;
using AutoMapper;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using SharedLibrary;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.HttpTriggers;

public class TodoApiTests
{
    private const string _notExistingUserId = "notExistingUserId";
    private const string _notExistingTodoId = "notExistingTodoId";

    private readonly List<User> _usersOnDb;
    private readonly string _existingUserId;
    private readonly User _existingUser;
    private readonly Todo _existingTodo;
    private readonly string _existingTodoId;

    private readonly string _todoTextNotTooLong = new('*', Validation.maxLengthOnAdd);
    private readonly string _todoTextTooLong = new('*', Validation.maxLengthOnAdd + 1);

    private readonly Mock<ITodoRepository> _mockedTodoRepository;
    private readonly ILogger<TodoApi> _logger;
    private readonly IMapper _mapper;

    private readonly TodoApi _webApi;

    public TodoApiTests()
    {
        #region FakeUsers

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
        _existingUser = _usersOnDb.First();
        _existingUserId = _existingUser.ClientPrincipal.UserId;
        _existingTodo = _existingUser.Todos.First();
        _existingTodoId = _existingTodo.Id;

        #endregion FakeUsers

        #region Mocks

        _mockedTodoRepository = new Mock<ITodoRepository>();
        _logger = new LoggerFactory().CreateLogger<TodoApi>();
        var mapperProfile = new TodoProfile();
        var mapperConf = new MapperConfiguration(cfg => cfg.AddProfile(mapperProfile));
        _mapper = new Mapper(mapperConf);
        _webApi = new TodoApi(_mockedTodoRepository.Object, _logger, _mapper);

        #endregion Mocks

        #region Setup

        SetUpMockedRepository(_mockedTodoRepository);

        #endregion Setup
    }

    private void SetUpMockedRepository(Mock<ITodoRepository> mockedTodoRepository)
    {
        _mockedTodoRepository.Setup(x => x.ResetDb(_existingUserId)).Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.ResetDb(_notExistingUserId)).Throws<UserNotFoundException>();
        _mockedTodoRepository.Setup(x => x.ResetDb(string.Empty)).Throws<ArgumentException>();

        _mockedTodoRepository.Setup(x => x.GetByQueryAsync(_existingUserId, false))
            .ReturnsAsync(_existingUser.Todos);
        _mockedTodoRepository.Setup(x => x.GetByQueryAsync(_existingUserId, true))
            .ReturnsAsync(_existingUser.Todos.Where(x => !x.IsCompleted).ToList());
        _mockedTodoRepository.Setup(x => x.GetByQueryAsync(_notExistingUserId, false))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));

        _mockedTodoRepository.Setup(x => x.GetByIdAsync(_existingUserId, _existingTodoId))
            .ReturnsAsync(_existingUser.Todos.FirstOrDefault(x => x.Id.Equals(_existingTodoId)));
        _mockedTodoRepository.Setup(x => x.GetByIdAsync(_existingUserId, _notExistingUserId))
            .ReturnsAsync(value: null);

        _mockedTodoRepository.Setup(x => x.AddAsync(_existingUserId, It.IsAny<Todo>()))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.AddAsync(_notExistingUserId, It.IsAny<Todo>()))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));

        _mockedTodoRepository.Setup(x => x.UpdateAsync(_existingUserId, _existingTodoId, _todoTextNotTooLong))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.UpdateAsync(_notExistingUserId, _existingTodoId, It.IsAny<string>()))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));
        _mockedTodoRepository.Setup(x => x.UpdateAsync(_existingUserId, _notExistingTodoId, It.IsAny<string>()))
            .ThrowsAsync(new TodoNotFoundException($"Todo {_notExistingTodoId} not found."));

        _mockedTodoRepository.Setup(x => x.CompleteAsync(_existingUserId, _existingTodoId))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.CompleteAsync(_notExistingUserId, _existingTodoId))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));
        _mockedTodoRepository.Setup(x => x.CompleteAsync(_existingUserId, _notExistingTodoId))
            .ThrowsAsync(new TodoNotFoundException($"Todo {_notExistingTodoId} not found."));

        _mockedTodoRepository.Setup(x => x.DeleteAsync(_existingUserId, _existingTodoId))
            .Returns(Task.CompletedTask);
        _mockedTodoRepository.Setup(x => x.DeleteAsync(_notExistingUserId, _existingTodoId))
            .ThrowsAsync(new UserNotFoundException($"User {_notExistingUserId} not found."));
        _mockedTodoRepository.Setup(x => x.DeleteAsync(_existingUserId, _notExistingTodoId))
            .ThrowsAsync(new TodoNotFoundException($"Todo {_notExistingTodoId} not found."));
    }

    [Fact]
    public async Task ResetDb_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post);
        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task ResetDb_NotExistingUser_ReturnNotFound()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(new ClientPrincipal { UserId = _notExistingUserId }, request);

        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.ResetDb(_notExistingUserId), Times.Once());
    }

    [Fact]
    public async Task ResetDb_ExistingId_CallService()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeOfType<OkResult>();
        _mockedTodoRepository.Verify(x => x.ResetDb(_existingUserId), Times.Once());
    }

    [Fact]
    public async Task ResetDb_EmptyId_ThrowArgumentException()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        IActionResult actionResult = await _webApi.ResetDb(request);

        actionResult.Should().BeOfType<OkResult>();
        _mockedTodoRepository.Verify(x => x.ResetDb(string.Empty), Times.Never());
    }

    [Fact]
    public async Task GetTodos_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get);
        var actionResult = await _webApi.GetTodos(request);

        actionResult.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTodos_NotExistingUser_ReturnEmptyListOfTodos()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(new ClientPrincipal { UserId = _notExistingUserId }, request);

        var actionResult = await _webApi.GetTodos(request);

        actionResult.Should().BeOfType<OkObjectResult>();
        var todoList = actionResult.As<OkObjectResult>().Value.As<List<TodoDto>>();
        todoList.Should().BeEmpty();
    }

    [Fact]
    public async Task GetTodos_WhenCalledWithNoQueryString_ReturnAllTodos()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        OkObjectResult? result = (await _webApi.GetTodos(request)) as OkObjectResult;

        result.Should().NotBeNull();
        var todos = result!.Value as IEnumerable<TodoDto>;
        todos.Should().NotBeNull();
        todos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodos_WhenCalledWithQueryString_ReturnOnlyUncompleted()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: new QueryString("?onlyUncompleted=true"), body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

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
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: new QueryString("?asd123"), body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        OkObjectResult? result = (await _webApi.GetTodos(request)) as OkObjectResult;

        result.Should().NotBeNull();
        var todos = result!.Value as IEnumerable<TodoDto>;
        todos.Should().NotBeNull();
        todos.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetTodoById_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post);
        var actionResult = await _webApi.GetTodoById(request, _existingTodoId);

        actionResult.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task GetTodoById_WhenCalledWithExistingId_ReturnsTodo()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        OkObjectResult? result = (await _webApi.GetTodoById(request, _existingTodoId)) as OkObjectResult;

        result.Should().NotBeNull();
        var todo = result!.Value as TodoDto;
        todo.Should().NotBeNull();
        todo!.Id.Should().Be(_existingTodoId);
        todo!.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetTodoById_WhenCalledWithNotExistingId_ReturnsNotFound()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = (await _webApi.GetTodoById(request, _notExistingUserId));

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task AddTodos_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task AddTodo_TodoTextTooLong_ReturnBadRequest()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task AddTodo_TodoTextNotTooLong_ReturnCreatedAtRoute()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<CreatedAtRouteResult>();
        _mockedTodoRepository.Verify(x => x.AddAsync(_existingUserId, It.IsAny<Todo>()), Times.Once());
    }

    [Fact]
    public async Task AddTodo_TodoNull_ReturnBadRequest()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task AddTodo_TodoParsedAsNull_ReturnBadRequest()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: "null");

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task AddTodo_NotExistingUserIdThrowException_ReturnBadRequest()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));

        // Inject a not existing user
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(new ClientPrincipal() { UserId = _notExistingUserId }, request);

        var result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.AddAsync(_notExistingUserId, It.IsAny<Todo>()), Times.Once());
    }

    [Fact]
    public async Task AddTodo_GenericException_ReturnBadRequestResult()
    {
        TodoDtoToAdd todoDtoToAdd = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToAdd));
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);
        _mockedTodoRepository.Setup(x => x.AddAsync(_existingUserId, It.IsAny<Todo>())).Throws(new Exception());

        IActionResult result = await _webApi.AddTodo(request);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.AddAsync(_existingUserId, It.IsAny<Todo>()), Times.Once());
    }

    [Fact]
    public async Task UpdateTodos_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task UpdateTodo_TodoTextTooLong_ReturnBadRequest()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task UpdateTodo_TodoTextNotTooLong_ReturnNoContent()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<NoContentResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingUserId, _existingTodoId, _todoTextNotTooLong), Times.Once());
    }

    [Fact]
    public async Task UpdateTodo_TodoParsedAsNull_ReturnBadRequest()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: "null");

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task UpdateTodo_TodoNull_ReturnBadRequest()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: null);

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
    }

    [Fact]
    public async Task UpdateTodo_UserNotFound_ReturnNotFound()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        // Inject a not existing user
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(new ClientPrincipal() { UserId = _notExistingUserId }, request);

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_notExistingUserId, _existingTodoId, _todoTextNotTooLong), Times.Once());
    }

    [Fact]
    public async Task UpdateTodo_TodoIdNotFoundThrowException_ReturnNotFound()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));

        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.UpdateTodo(request, _notExistingTodoId);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingUserId, _notExistingTodoId, _todoTextNotTooLong), Times.Once());
    }

    [Fact]
    public async Task UpdateTodo_GenericException_ReturnBadRequest()
    {
        TodoDtoToUpdate todoDtoToUpdate = new(_todoTextNotTooLong);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Put, queryStrings: null, body: JsonSerializer.Serialize(todoDtoToUpdate));
        _mockedTodoRepository.Setup(x => x.UpdateAsync(_existingUserId, _existingTodoId, It.IsAny<string>())).Throws(new Exception());
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.UpdateTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.UpdateAsync(_existingUserId, _existingTodoId, It.IsAny<string>()), Times.Once());
    }

    [Fact]
    public async Task CompleteToDo_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

        var actionResult = await _webApi.CompleteTodo(request, _existingTodoId);

        actionResult.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task CompleteTodo_WhenCalledOnExistingTodoId_ReturnOkResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.CompleteTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<OkResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_existingUserId, _existingTodoId), Times.Once());
    }

    [Fact]
    public async Task CompleteTodo_WhenCalledOnNotExistingUser_ReturnNotFound()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        // Inject a not existing user
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(new ClientPrincipal() { UserId = _notExistingUserId }, request);

        var result = await _webApi.CompleteTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_notExistingUserId, _existingTodoId), Times.Once());
    }

    [Fact]
    public async Task CompleteTodo_WhenCalledOnNotExistingTodo_ReturnNotFound()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.CompleteTodo(request, _notExistingTodoId);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_existingUserId, _notExistingTodoId), Times.Once());
    }

    [Fact]
    public async Task CompleteTodo_GenericException_ReturnBadRequestObjectResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        _mockedTodoRepository.Setup(x => x.CompleteAsync(_existingUserId, _existingTodoId)).Throws(new Exception());
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.CompleteTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.CompleteAsync(_existingUserId, _existingTodoId), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_HeaderNotExisting_ReturnUnauthorizedResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);

        var result = await _webApi.DeleteTodo(request, _existingTodoId);

        result.Should().BeOfType<UnauthorizedResult>();
    }

    [Fact]
    public async Task DeleteTodo_WhenCalledOnNotExistingUser_ReturnNotFound()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);
        // Inject a not existing user
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(new ClientPrincipal() { UserId = _notExistingUserId }, request);

        var result = await _webApi.DeleteTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_notExistingUserId, _existingTodoId), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_WhenCalledOnNotExistingTodo_ReturnNotFound()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Delete, queryStrings: null, body: null);
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.DeleteTodo(request, _notExistingTodoId);

        result.Should().NotBeNull().And.BeOfType<NotFoundObjectResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_existingUserId, _notExistingTodoId), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_WhenCalledOnExistingTodo_ReturnNoContentResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);

        var result = await _webApi.DeleteTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<NoContentResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_existingUserId, _existingTodoId), Times.Once());
    }

    [Fact]
    public async Task DeleteTodo_GenericException_ReturnBadRequestResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);
        HttpRequestHelper.InjectClientPrincipalToAuthHeader(_existingUser.ClientPrincipal, request);
        _mockedTodoRepository.Setup(x => x.DeleteAsync(_existingUserId, _existingTodoId)).Throws(new Exception());

        var result = await _webApi.DeleteTodo(request, _existingTodoId);

        result.Should().NotBeNull().And.BeOfType<BadRequestResult>();
        _mockedTodoRepository.Verify(x => x.DeleteAsync(_existingUserId, _existingTodoId), Times.Once());
    }
}