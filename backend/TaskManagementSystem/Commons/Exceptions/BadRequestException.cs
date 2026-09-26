namespace TaskManagementSystem.Commons.Exceptions
{
    public class BadRequestException : AppException
    {
        public BadRequestException(string message)
            : base(message)
        {
        }
    }
}
