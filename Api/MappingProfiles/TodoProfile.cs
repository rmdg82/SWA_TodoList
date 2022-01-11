using Shared.Dtos;
using Api.Models;
using AutoMapper;

namespace Api.MappingProfiles;

public class TodoProfile : Profile
{
    public TodoProfile()
    {
        CreateMap<Todo, TodoDto>().ReverseMap();
        CreateMap<Todo, TodoDtoToAdd>().ReverseMap();
        CreateMap<Todo, TodoDtoToUpdate>().ReverseMap();
    }
}