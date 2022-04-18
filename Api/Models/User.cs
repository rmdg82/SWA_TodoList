using MongoDB.Bson.Serialization.Attributes;

namespace Api.Models;

public class User
{
    public string Id { get; set; }

    public ClientPrincipal ClientPrincipal { get; set; }
    public ICollection<Todo> Todos { get; set; } = new List<Todo>();
}