using System;
using System.Linq;
using FluentNHibernate.Conventions;
using FluentNHibernate.Conventions.Instances;

namespace Oms.Server.DataAccess.NHibernate
{
    /// <summary>
    /// Stores a readable discriminator ("OrderParameterEventLog_IOrderEditableData") for the generic event logs,
    /// instead of the assembly qualified type name that NHibernate would use.
    /// </summary>
    public class EventLogDiscriminatorConvention : ISubclassConvention
    {
        public void Apply(ISubclassInstance instance)
        {
            var type = instance.EntityType;
            if (type.IsGenericType)
                instance.DiscriminatorValue(Name(type));
        }

        private static string Name(Type type)
        {
            return type.Name.Split('`')[0] + "_" + string.Join("_", type.GetGenericArguments().Select(a => a.Name));
        }
    }
}
