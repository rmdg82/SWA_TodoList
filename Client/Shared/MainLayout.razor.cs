using Client.HttpRepository;
using Microsoft.AspNetCore.Components;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Shared
{
    public partial class MainLayout
    {
        [Inject]
        public IAuthRepository? AuthRepository { get; set; }

        public IdentityDto? IdentityDto { get; set; }
        private bool _isAuthenticated;

        protected override async Task OnInitializedAsync()
        {
            IdentityDto = await AuthRepository!.GetIdentity();
            if (IdentityDto != null)
            {
                _isAuthenticated = true;
            }
        }
    }
}