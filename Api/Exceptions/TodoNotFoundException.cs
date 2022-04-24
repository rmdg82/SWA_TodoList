using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Api.Exceptions;

public class TodoNotFoundException : ApplicationException
{
    public TodoNotFoundException()
    {
    }

    public TodoNotFoundException(string? message) : base(message)
    {
    }

    public TodoNotFoundException(string? message, Exception? innerException) : base(message, innerException)
    {
    }
}