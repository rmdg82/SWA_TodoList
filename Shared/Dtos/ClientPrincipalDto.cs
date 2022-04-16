﻿namespace SharedLibrary.Dtos;

public class ClientPrincipalDto
{
    public string IdentityProvider { get; set; }
    public string UserId { get; set; }
    public string UserDetails { get; set; }
    public IEnumerable<string> UserRoles { get; set; }
}