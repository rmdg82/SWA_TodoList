using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Models;

public class User
{
    public string Id { get; set; }
    public ClientPrincipal? ClientPrincipal { get; set; }
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
}