using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Oms.Server.Domain.Assets;
using Oms.Server.Domain.Interfaces;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class AssetRepository : RepositoryBase<Asset>, IAssetRepository
    {
        public Asset GetAssetById(int id)
        {
            return base.GetById(id);
        }

        public IList<Asset> GetAssetByKind(AssetType assetType)
        {
            return base.GetByColumn("Type", assetType);
        }

        public IList<Asset> GetAssetList()
        {
            return base.GetByColumn("IsActive", true);
        }
    }
}
