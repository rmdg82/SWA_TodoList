using Api.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Validators;

public class UserToAddValidator : AbstractValidator<User>
{
    public UserToAddValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        RuleFor(x => x.ClientPrincipal.UserId).NotEmpty();
    }
}