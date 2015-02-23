using FluentNHibernate.Mapping;
using Oms.Server.Domain.Users;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class FundManagerMap : SubclassMap<FundManager>
    {
        public FundManagerMap()
        {
            DiscriminatorValue(@"FundManager");
            References(x => x.Fund).Column(FundColumnName).Cascade.All();

        }

        public static readonly string FundColumnName = "fund_fundid";
    }
}