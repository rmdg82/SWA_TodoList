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
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Xunit;

namespace Api.Tests.HttpTriggers;

public class UserApiTests
{
    private const string _notExistingUserId = "notExistingUserId";

    private readonly ClientPrincipal _notExistingUserClientPrincipal = new()
    {
        UserId = "3",
        IdentityProvider = "aad",
        UserDetails = "rmdg82",
        UserRoles = new[] { "authenticated", "admin" }
    };

    private readonly List<User> _usersOnDb;
    private readonly string _existingUserId;
    private readonly User _existingUser;

    private readonly Mock<IUserRepository> _mockedUserRepository;
    private readonly ILogger<UserApi> _logger;
    private readonly IMapper _mapper;

    private readonly UserApi _webApi;

    public UserApiTests()
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

        #endregion FakeUsers

        _mockedUserRepository = new Mock<IUserRepository>();
        _logger = new LoggerFactory().CreateLogger<UserApi>();
        var userProfile = new UserProfile();
        var todoProfile = new TodoProfile();
        var mapperConf = new MapperConfiguration(cfg => cfg.AddProfiles(new Profile[] { userProfile, todoProfile }));
        _mapper = new Mapper(mapperConf);
        _webApi = new UserApi(_mockedUserRepository.Object, _logger, _mapper);

        SetUpMockedRepository(_mockedUserRepository);
    }

    private void SetUpMockedRepository(Mock<IUserRepository> mockedUserRepository)
    {
        _mockedUserRepository.Setup(x => x.GetUser(_notExistingUserId))
            .ReturnsAsync((User?)null);
        _mockedUserRepository.Setup(x => x.GetUser(_existingUserId))
            .ReturnsAsync(_usersOnDb.FirstOrDefault(u => u.Id == _existingUserId));

        _mockedUserRepository.Setup(x => x.CreateUser(It.Is<ClientPrincipal>(x => x.UserId == _existingUserId)))
            .ReturnsAsync((User?)null);
        _mockedUserRepository.Setup(x => x.CreateUser(It.Is<ClientPrincipal>(x => x.UserId == _notExistingUserClientPrincipal.UserId)))
            .ReturnsAsync(new User { Id = _notExistingUserClientPrincipal.UserId, ClientPrincipal = _notExistingUserClientPrincipal });
    }

    [Fact]
    public async Task GetUser_UserNotFound_ReturnNotFoundResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get);

        var actionResult = await _webApi.GetUser(request, _notExistingUserId);

        actionResult.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetUser_UserFound_ReturnOkWithSerializedUser()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Get);

        var result = await _webApi.GetUser(request, _existingUserId);

        result.Should().BeOfType<OkObjectResult>();
        var user = ((OkObjectResult)result).Value as UserDto;
        user.Should().NotBeNull().And.BeEquivalentTo(_mapper.Map<UserDto>(_existingUser));
        user.Id.Should().Be(_existingUserId);
    }

    [Fact]
    public async Task CreateUser_ClientPrincipalDtoNull_ReturnBadRequestResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: null);

        var result = await _webApi.CreateUser(request);

        result.Should().BeOfType<BadRequestResult>();
        _mockedUserRepository.Verify(x => x.CreateUser(It.IsAny<ClientPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_ClientPrincipalParsedAsNull_ReturnBadRequestResult()
    {
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: "null");

        var result = await _webApi.CreateUser(request);

        result.Should().BeOfType<BadRequestResult>();
        _mockedUserRepository.Verify(x => x.CreateUser(It.IsAny<ClientPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_ValidationErrors_ReturnBadRequestResult()
    {
        var clientPrincipalDto = new ClientPrincipalDto
        {
            UserId = "",
            IdentityProvider = "",
            UserDetails = "",
            UserRoles = Array.Empty<string>()
        };
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(clientPrincipalDto));

        var result = await _webApi.CreateUser(request);

        result.Should().NotBeNull().And.BeOfType<BadRequestObjectResult>();
        _mockedUserRepository.Verify(x => x.CreateUser(It.IsAny<ClientPrincipal>()), Times.Never);
    }

    [Fact]
    public async Task CreateUser_NoValidationErrors_ReturnCreatedAtRoute()
    {
        ClientPrincipalDto clientPrincipalDto = _mapper.Map<ClientPrincipalDto>(_notExistingUserClientPrincipal);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(clientPrincipalDto));

        var result = await _webApi.CreateUser(request);

        result.Should().NotBeNull().And.BeOfType<CreatedAtRouteResult>().And.Subject.As<CreatedAtRouteResult>().RouteName.Should().Be("GetUser");
        _mockedUserRepository.Verify(x => x.CreateUser(It.IsAny<ClientPrincipal>()), Times.Once);
    }

    [Fact]
    public async Task CreateUser_UserAlreadyPresent_ReturnConflictObject()
    {
        ClientPrincipalDto clientPrincipalDto = _mapper.Map<ClientPrincipalDto>(_existingUser.ClientPrincipal);
        HttpRequest request = HttpRequestHelper.CreateHttpRequest(HttpMethods.Post, queryStrings: null, body: JsonSerializer.Serialize(clientPrincipalDto));

        var result = await _webApi.CreateUser(request);

        result.Should().NotBeNull().And.BeOfType<OkObjectResult>();
        _mockedUserRepository.Verify(x => x.CreateUser(It.IsAny<ClientPrincipal>()), Times.Once);
    }
}