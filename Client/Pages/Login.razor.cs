using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Client.Pages
{
    public partial class Login
    {
        public static string[] Providers { get; } = { "github", "aad" };

        public static string GetProviderLink(string provider) => $"/.auth/login/{provider}";

        //public string LoginWithGitHub()
        //{
        //}
    }
}