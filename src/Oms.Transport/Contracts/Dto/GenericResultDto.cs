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

        public string ErrorMessage { get; set; }

        public int ErrorCode { get; set; }

    }
}