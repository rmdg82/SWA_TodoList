using System;
using System.Text;
using System.Text.Json;
using SharedLibrary.Dtos;

namespace Tests.UserSerializer;

public class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Serialize and convert to Base64 a Model.User object");
        Console.WriteLine("The result should be used as a value for the 'Authorization' header");

        var clientPrincipal = new ClientPrincipalDto
        {
            UserId = "57168f990d2329166d4e136248dc141f",
            IdentityProvider = "aad",
            UserDetails = "FakeMSUSer",
            UserRoles = new List<string> { "anonymous", "authenticated" }
        };

        var json = JsonSerializer.Serialize(clientPrincipal);

        Console.WriteLine("JSON: " + json);

        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        Console.WriteLine($"Base64: {base64}");
    }
}