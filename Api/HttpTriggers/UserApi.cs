using Api.Models;
using Api.Repositories.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    public async Task<User> GetUser([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "user/{id}")] HttpRequest req, string id)
    {
        User user = await _userRepository.GetUser(id);
        return _mapper.Map<User>(user);
    }
}