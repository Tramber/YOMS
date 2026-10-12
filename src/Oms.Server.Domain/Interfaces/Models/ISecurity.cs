using System;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.Domain.Interfaces.Models
{
    /// <summary>
    /// Represents a security in the system.
    /// </summary>
    public interface ISecurity : IIdentifiable
    {
        /// <summary>
        /// Gets or sets the name of the security.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Gets or sets the ISIN code of the security.
        /// </summary>
        string IsinCode { get; set; }

        /// <summary>
        /// Gets or sets the Bloomberg ticker of the security.
        /// </summary>
        string BloombergTicker { get; set; }

        /// <summary>
        /// Gets or sets the currency of the security.
        /// </summary>
        Currency Currency { get; set; }

        /// <summary>
        /// Gets or sets the type of the security.
        /// </summary>
        SecurityType Type { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the security is active.
        /// </summary>
        bool IsActive { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the security.
        /// </summary>
        DateTime CreationDate { get; set; }

        /// <summary>
        /// Gets or sets the creator of the security.
        /// </summary>
        IUser Creator { get; set; }

        /// <summary>
        /// Gets or sets the Sophis code of the security.
        /// </summary>
        string SophisCode { get; set; }
    }
}
