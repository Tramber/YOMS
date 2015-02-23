using System.Collections.Generic;
using Oms.Server.Domain.Assets;

namespace Oms.Server.Domain.Interfaces
{
    public interface IAssetRepository : IRepository<Asset>
    {
        Asset GetAssetById(int id);
        IList<Asset> GetAssetByKind(AssetType assetType);
        IList<Asset> GetAssetList();
    }
}