using Api.Models;
using Api.Repositories.Interfaces;
using Api.Validators;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using SharedLibrary.Dtos;
using System.Text.Json;

namespace Api.HttpTriggers;

public class UserApi
{
    private readonly IMapper _mapper;
    private readonly ILogger<UserApi> _logger;
    private readonly IUserRepository _userRepository;

    public UserApi(IMapper mapper, ILogger<UserApi> logger, IUserRepository userRepository)
    {
        _mapper = mapper;
        _logger = logger;
        _userRepository = userRepository;
    }

    [FunctionName("GetUser")]
    public async Task<ActionResult<UserDto>> GetUser([HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation($"New request GetUser: {id}");
        User? user = await _userRepository.GetUser(id);

        if (user is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(_mapper.Map<UserDto>(user));
    }

    [FunctionName("CreateUser")]
    public async Task<ActionResult<User>> CreateUser([HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"New request for {nameof(CreateUser)} with body [{requestBody}].");

        var clientPrincipal = JsonSerializer.Deserialize<ClientPrincipal>(requestBody);

        if (clientPrincipal is null)
        {
            _logger.LogError($"Invalid request body for {nameof(CreateUser)}.");
            return new BadRequestResult();
        }

        var validator = new ClientPrincipalToAddValidator();
        var validationResult = validator.Validate(clientPrincipal);
        if (!validationResult.IsValid)
        {
            return new BadRequestObjectResult(
                validationResult.Errors.Select(x => new
                {
                    Field = x.PropertyName,
                    Error = x.ErrorMessage
                }));
        }

        var user = await _userRepository.CreateUser(clientPrincipal);

        return new CreatedAtRouteResult("GetUser", new { id = user.Id }, user);
    }
}