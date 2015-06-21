using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Automapping;
using FluentNHibernate.Automapping.Alterations;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.DataAccess.NHibernate.Mapping
{
    public class OrderAutoMappingOverride : IAutoMappingOverride<Order>
    {
        public void Override(AutoMapping<Order> mapping)
        {
            foreach (var propertyInfo in typeof(IOrderEditableData).GetProperties()
                .Union(typeof(IOrderRoutingData).GetProperties())
                .Union(typeof(IOrderComputedData).GetProperties()))
            {
               ((IPropertyIgnorer)mapping).IgnoreProperty(propertyInfo.Name);
            }
        }
    }
}
