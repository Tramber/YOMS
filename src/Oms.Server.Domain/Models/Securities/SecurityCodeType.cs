namespace Oms.Server.Domain.Models.Securities
{
    /// <summary>
    /// Represents the type of a security code.
    /// </summary>
    public enum SecurityCodeType
    {
        Undefined,
        Isin,
        BloombergTicker,
        Sophis,
    }
}
