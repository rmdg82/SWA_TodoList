using Api.Models;
using AutoMapper;
using System;
using SharedLibrary.Dtos;

namespace Api.MappingProfiles;

public class TodoProfile : Profile
{
    public TodoProfile()
    {
        CreateMap<Todo, TodoDto>();

        CreateMap<TodoDtoToAdd, Todo>()
            .ForMember(dest => dest.IsCompleted, opt => opt.MapFrom(src => false))
            .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => DateTime.UtcNow));

        CreateMap<TodoDtoToUpdate, Todo>();
    }
}