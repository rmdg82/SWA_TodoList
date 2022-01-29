using FluentValidation;
using SharedLibrary;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Validators
{
    public class TodoDtoToUpdateValidator : AbstractValidator<TodoDtoToUpdate>
    {
        public TodoDtoToUpdateValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .MaximumLength(ValidationConstants.maxLengthOnUpdate);
        }
    }
}