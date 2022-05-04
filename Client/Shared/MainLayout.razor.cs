using Client.HttpRepository.Interfaces;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using SharedLibrary.Dtos;

namespace Client.Shared;

public partial class MainLayout
{
    [Inject]
    public IAuthHttpRepository? AuthHttpRepository { get; set; }

    [Inject]
    public IUserHttpRepository? UserHttpRepository { get; set; }

    [Inject]
    public ISnackbar? SnackbarService { get; set; }

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

            var result = await UserHttpRepository!.CreateUser(IdentityDto.ClientPrincipal);
            if (result is not null)
            {
                SnackbarService!.Add($"<h1>Welcome!</h1><h2>New user <strong>'{IdentityDto.ClientPrincipal.UserDetails}'</strong> has been created</h2>", Severity.Success, config => { config.Icon = _icon; });
            }
        }
    }
}