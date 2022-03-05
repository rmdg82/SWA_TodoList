﻿using FluentValidation;
using SharedLibrary;
using SharedLibrary.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Validators
{
    public class TodoDtoToAddValidator : AbstractValidator<TodoDtoToAdd>
    {
        public TodoDtoToAddValidator()
        {
            RuleFor(x => x.Text)
                .NotNull()
                .MaximumLength(ValidationConstants.maxLengthOnAdd);
        }
    }
}