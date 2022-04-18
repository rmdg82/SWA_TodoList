using Client.HttpRepository.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SharedLibrary.Dtos;
using System.Text.Json;

namespace Client.Shared;

public partial class MainLayout
{
    [Inject]
    public IAuthHttpRepository? AuthHttpRepository { get; set; }

    [Inject]
    public IUserHttpRepository? UserHttpRepository { get; set; }

    public IdentityDto? IdentityDto { get; set; }
    private bool _isAuthenticated;
    private string _icon = Icons.TwoTone.Person;

    protected override async Task OnInitializedAsync()
    {
        IdentityDto = await AuthHttpRepository!.GetIdentity();
        if (IdentityDto?.ClientPrincipal != null && IdentityDto.ClientPrincipal.UserRoles.Contains("authenticated"))
        {
            _isAuthenticated = true;

            switch (IdentityDto.ClientPrincipal.IdentityProvider)
            {
                case "github":
                    _icon = Icons.Custom.Brands.GitHub;
                    break;

                case "aad":
                    _icon = Icons.Custom.Brands.Microsoft;
                    break;

                case "twitter":
                    _icon = Icons.Custom.Brands.Twitter;
                    break;

                default:
                    break;
            }

            var user = await UserHttpRepository!.GetUser(IdentityDto!.ClientPrincipal.UserId);
            if (user is null)
            {
                await UserHttpRepository!.CreateUser(IdentityDto.ClientPrincipal);
            }
        }
    }
}