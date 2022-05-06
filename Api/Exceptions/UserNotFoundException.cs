namespace Api.Exceptions;

public class UserNotFoundException : ApplicationException
{
    public UserNotFoundException()
    {
    }

    public UserNotFoundException(string message) : base(message)
    {
    }

    public UserNotFoundException(string message, Exception? innerException) : base(message, innerException)
    {
    }
}