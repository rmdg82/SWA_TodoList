using Api.Models;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Api.Tests.Utils;

public static class HttpRequestHelper
{
    public static string AuthHeader { get; private set; } = "x-ms-client-principal";

    /// <summary>
    /// Inject "x-ms-client-principal" header with a serialized ClientPrincipal object into the request
    /// </summary>
    /// <param name="clientPrincipal"></param>
    /// <param name="req"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal static void InjectClientPrincipalToAuthHeader(ClientPrincipal clientPrincipal, HttpRequest req)
    {
        if (clientPrincipal is null)
        {
            throw new ArgumentNullException(nameof(clientPrincipal));
        }

        if (req is null)
        {
            throw new ArgumentNullException(nameof(req));
        }

        string principal = JsonSerializer.Serialize(clientPrincipal);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(principal));

        req.Headers.Add(AuthHeader, base64);
    }

    /// <summary>
    /// Create an HttpRequest from params
    /// </summary>
    /// <param name="method"></param>
    /// <param name="queryStrings"></param>
    /// <param name="body"></param>
    /// <returns></returns>
    internal static HttpRequest CreateHttpRequest(string method, QueryString? queryStrings = null, string? body = null)
    {
        var context = new DefaultHttpContext();
        var request = context.Request;
        request.Method = method;
        request.QueryString = queryStrings ?? QueryString.Empty;
        if (body != null)
        {
            var bytes = Encoding.UTF8.GetBytes(body);
            request.Body = new MemoryStream(bytes);
        }

        return request;
    }

    internal static string GetBase64FromClientPrincipal(ClientPrincipal clientPrincipal)
    {
        string principal = JsonSerializer.Serialize(clientPrincipal);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(principal));
        return base64;
    }
}