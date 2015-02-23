using System;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Users;

namespace Oms.Server.Domain.Assets
{
    public class Asset : IIdentifiable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string IsinCode { get; set; }
        public string BloombergTicker { get; set; }
        public Currency Currency { get; set; }
        public AssetType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreationDate { get; set; }
        public User Creator { get; set; }
        public AssetOriginType Origin { get; set; }
    }
}
