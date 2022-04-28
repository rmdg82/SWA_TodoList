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

    public UserApi(IUserRepository userRepository, ILogger<UserApi> logger, IMapper mapper)
    {
        _mapper = mapper;
        _logger = logger;
        _userRepository = userRepository;
    }

    [FunctionName(nameof(GetUser))]
    public async Task<IActionResult> GetUser([HttpTrigger(AuthorizationLevel.Function, "get", Route = "users/{id}")] HttpRequest req, string id)
    {
        _logger.LogInformation($"New request GetUser: {id}");

        User? user = await _userRepository.GetUser(id);

        if (user is null)
        {
            return new NotFoundResult();
        }

        return new OkObjectResult(_mapper.Map<UserDto>(user));
    }

    [FunctionName(nameof(CreateUser))]
    public async Task<IActionResult> CreateUser([HttpTrigger(AuthorizationLevel.Function, "post", Route = "users")] HttpRequest req)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
        _logger.LogInformation($"New request for {nameof(CreateUser)} with body [{requestBody}].");

        ClientPrincipalDto? clientPrincipal;
        try
        {
            clientPrincipal = JsonSerializer.Deserialize<ClientPrincipalDto>(requestBody);
        }
        catch (Exception)
        {
            _logger.LogError($"I could not parse the request body {requestBody}.");
            return new BadRequestResult();
        }

        if (clientPrincipal is null)
        {
            _logger.LogError($"Parses request body is null.");
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

        var user = await _userRepository.CreateUser(_mapper.Map<ClientPrincipal>(clientPrincipal));

        return (user is null) ?
            new OkObjectResult(new { Message = "User already exists." }) :
            new CreatedAtRouteResult("GetUser", new { id = user.Id }, user);
    }
}