using Api.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.Text;
using System.Text.Json;

namespace Api.Tests;

/// <summary>
/// Inject 'x-ms-client-principal' header into the request
/// </summary>
public class HeaderInjector
{
    public string HeaderName { get; } = "x-ms-client-principal";
    public ClientPrincipal ClientPrincipal { get; init; }

    public HeaderInjector(ClientPrincipal clientPrincipal)
    {
        ClientPrincipal = clientPrincipal ?? throw new ArgumentNullException(nameof(clientPrincipal));
    }

    public void Inject(HttpRequest req)
    {
        string principal = JsonSerializer.Serialize(ClientPrincipal);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(principal));

        req.Headers.Add(HeaderName, base64);
    }
}