namespace Oms.Transport.Contracts.Messages
{
    public class GenericResultDto
    {
        public GenericResultDto()
        {
        }

        public GenericResultDto(string errorMessage, int errorCode)
        {
            ErrorMessage = errorMessage;
            ErrorCode = errorCode;
        }

        public string ErrorMessage { get; private set; }

        public int ErrorCode { get; private set; }

    }
}