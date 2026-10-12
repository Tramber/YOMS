namespace Oms.Server.Domain.Framework
{
    public static class GenericResultExtentions
    {
        /// <summary>True when the command succeeded.</summary>
        public static bool IsSuccess(this GenericResult genericResult)
        {
            return genericResult.ErrorCode == GenericResult.NoErrorCode;
        }

        /// <summary>True when the command failed; see <see cref="GenericResult.ErrorMessage"/>.</summary>
        public static bool IsFailure(this GenericResult genericResult)
        {
            return genericResult.ErrorCode != GenericResult.NoErrorCode;
        }
    }
}