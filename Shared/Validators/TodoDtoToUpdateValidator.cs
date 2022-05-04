using FluentValidation;
using SharedLibrary;
using SharedLibrary.Dtos;

namespace SharedLibrary.Validators;

public class TodoDtoToUpdateValidator : AbstractValidator<TodoDtoToUpdate>
{
    public TodoDtoToUpdateValidator()
    {
        RuleFor(x => x.Text)
            .NotNull()
            .MaximumLength(Validation.maxLengthOnUpdate);
    }
}