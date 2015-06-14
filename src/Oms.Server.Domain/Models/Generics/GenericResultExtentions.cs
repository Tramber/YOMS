namespace Oms.Server.Domain.Models.Generics
{
    public static class GenericResultExtentions
    {
        public static bool IsSuccess(this GenericResult genericResult)
        {
            return genericResult.ErrorCode == GenericResult.NoErrorCode;
        }

        public static bool IsFailure(this GenericResult genericResult)
        {
            return genericResult.ErrorCode != GenericResult.NoErrorCode;
        }
    }
}