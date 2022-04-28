using FluentValidation;
using SharedLibrary.Dtos;

namespace Api.Validators;

public class ClientPrincipalToAddValidator : AbstractValidator<ClientPrincipalDto>
{
    public ClientPrincipalToAddValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.UserDetails).NotEmpty();
        RuleFor(x => x.IdentityProvider).NotEmpty();
        RuleFor(x => x.UserRoles).NotEmpty();
    }
}