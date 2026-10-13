using System.Collections.Generic;
using System.Threading.Tasks;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Services.Filtering
{
    /// <summary>Turns a free-text blotter filter ("alpha fund buys") into the values it restricts the orders to.</summary>
    public interface IFilterInterpreter
    {
        /// <summary>The funds and securities are the only values the text can refer to.</summary>
        Task<OrderFilterDto> InterpretAsync(string text, IReadOnlyList<FundDto> funds, IReadOnlyList<SecurityDto> securities);
    }
}
