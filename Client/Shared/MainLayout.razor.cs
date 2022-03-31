using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using SharedLibrary.Dtos;

namespace Client.Shared
{
    public partial class MainLayout
    {
        [Inject]
        public IAuthRepository? AuthRepository { get; set; }

        public IdentityDto? IdentityDto { get; set; }
        private bool _isAuthenticated;
        private string _icon = @"@Icons.TwoTone.Person";

        protected override async Task OnInitializedAsync()
        {
            IdentityDto = await AuthRepository!.GetIdentity();
            if (IdentityDto?.ClientPrincipal != null && IdentityDto.ClientPrincipal.UserRoles.Contains("authenticated"))
            {
                _isAuthenticated = true;
                switch (IdentityDto.ClientPrincipal.IdentityProvider)
                {
                    case "github":
                        _icon = @"@Icons.Custom.Brands.GitHub";
                        break;

                    case "aad":
                        _icon = @"@Icons.Custom.Brands.Microsoft";
                        break;

                    case "twitter":
                        _icon = @"@Icons.Custom.Brands.Twitter";
                        break;

                    default:
                        break;
                }
            }

            StateHasChanged();
        }
    }
}