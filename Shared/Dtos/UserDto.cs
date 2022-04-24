using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary.Dtos;

public class UserDto
{
    public string Id { get; set; }
    public ClientPrincipalDto ClientPrincipal { get; set; }
    public ICollection<TodoDto> Todos { get; set; } = new List<TodoDto>();
}