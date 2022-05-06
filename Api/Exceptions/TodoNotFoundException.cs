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