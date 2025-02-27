namespace MyMDb.Data
{
    public class ActionResponseExceptions
    {
        public class BaseException : Exception
        {
            public int StatusCode { get; }

            public BaseException(string message, int statusCode) : base(message)
            {
                StatusCode = statusCode;
            }
        }

        public class InternalServerErrorException : BaseException
        {
            public InternalServerErrorException(string message) : base(message, 500) { }
        }

        public class ConflictException : BaseException
        {
            public ConflictException(string message) : base(message, 409) { }
        }

        public class ForbiddenAccessException : BaseException
        {
            public ForbiddenAccessException(string message) : base(message, 403) { }
        }

        public class UnauthorizedAccessException : BaseException
        {
            public UnauthorizedAccessException(string message) : base(message, 401) { }
        }

        public class BadRequestException : BaseException
        {
            public BadRequestException(string message) : base(message, 400) { }
        }

        public class NotFoundException : BaseException
        {
            public NotFoundException(string message) : base(message, 404) { }
        }
    }
}
