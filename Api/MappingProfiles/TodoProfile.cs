using Api.Dtos;
using Api.Models;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.MappingProfiles
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<Todo, TodoDto>().ReverseMap();
            CreateMap<Todo, TodoDtoToAdd>().ReverseMap();
            CreateMap<Todo, TodoDtoToUpdate>().ReverseMap();
        }
    }
}