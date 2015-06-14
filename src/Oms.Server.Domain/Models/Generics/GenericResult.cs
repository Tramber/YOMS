namespace Oms.Server.Domain.Models.Generics
{
    public class GenericResult
    {
        public const int NoErrorCode = 0;
        public const int DefaultErrorCode = 1;

        private GenericResult(string errorMessage, int errorCode)
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public string ErrorMessage { get; private set; }

        public int ErrorCode { get; private set; }

        public static GenericResult Failure(string errorMessage, int errorCode = DefaultErrorCode)
        {
            return new GenericResult(errorMessage, errorCode == NoErrorCode ? DefaultErrorCode : errorCode);
        }

        public static GenericResult Success()
        {
            return new GenericResult(null, NoErrorCode);
        }
    }
}