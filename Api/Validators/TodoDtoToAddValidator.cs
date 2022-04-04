using FluentValidation;
using SharedLibrary;
using SharedLibrary.Dtos;

namespace Api.Validators
{
    public class TodoDtoToAddValidator : AbstractValidator<TodoDtoToAdd>
    {
        public TodoDtoToAddValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .MaximumLength(Validation.maxLengthOnAdd);
        }
    }
}