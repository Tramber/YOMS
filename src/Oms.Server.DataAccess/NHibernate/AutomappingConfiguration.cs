using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Automapping;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Trades;
using Oms.Server.Domain.Models.Users;

namespace Oms.Server.DataAccess.NHibernate
{
    public class AutomappingConfiguration : DefaultAutomappingConfiguration
    {
        private readonly HashSet<Type> _mappedTypes = new HashSet<Type>();

        public AutomappingConfiguration()
        {
            _mappedTypes.Add(typeof (Order));
            _mappedTypes.Add(typeof (OrderBasket));
            _mappedTypes.Add(typeof (Trade));
            _mappedTypes.Add(typeof (User));
            _mappedTypes.Add(typeof (Fund));
        }

        public override string GetDiscriminatorColumn(Type type)
        {
            return base.GetDiscriminatorColumn(type);
        }

        public override bool ShouldMap(Type type)
        {
            return _mappedTypes.Contains(type);
        }
    }
}
