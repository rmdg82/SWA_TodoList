using Api.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Validators;

public class ClientPrincipalToAddValidator : AbstractValidator<ClientPrincipal>
{
    public ClientPrincipalToAddValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserDetails).NotEmpty();
        RuleFor(x => x.IdentityProvider).NotEmpty();
        RuleFor(x => x.UserRoles).NotEmpty();
    }
}