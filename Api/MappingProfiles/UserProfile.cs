using Api.Models;
using AutoMapper;
using SharedLibrary.Dtos;

namespace Api.MappingProfiles;

public class UserProfile : Profile
{
    public UserProfile()
    {
        CreateMap<ClientPrincipal, ClientPrincipalDto>().ReverseMap();
        CreateMap<User, UserDto>().ReverseMap();
    }
}