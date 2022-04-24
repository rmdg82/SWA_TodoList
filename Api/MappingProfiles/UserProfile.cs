using Api.Models;
using AutoMapper;
using SharedLibrary.Dtos;

namespace Api.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<User, UserDto>();
        CreateMap<UserDto, User>();
    }
}