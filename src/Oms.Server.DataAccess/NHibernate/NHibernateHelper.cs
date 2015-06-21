using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Workflow;

namespace Oms.Server.DataAccess.NHibernate
{
    internal class NHibernateHelper
    {
        private readonly FluentConfiguration _configuration;
        private readonly ISessionFactory _sessionFactory;
        public static readonly NHibernateHelper Instance = new NHibernateHelper();


        private IPersistenceConfigurer GetDatabaseConfigurer()
        {
            var connectionNode = ConfigurationManager.ConnectionStrings["OmsDB"];
            switch (connectionNode.ProviderName)
            {
                case "SQLite" :
                    return SQLiteConfiguration.Standard.UsingFile(Path.Combine(Assembly.GetExecutingAssembly().Location));
                case "Oracle" :
                    return OracleManagedDataClientConfiguration.Oracle10.ConnectionString(connectionNode.ConnectionString);
                default :
                    throw new NotImplementedException(connectionNode.ProviderName);
            }
        }

        private NHibernateHelper()
        {
            _configuration = Fluently.Configure()
                .Database(GetDatabaseConfigurer)
                .Mappings(m => m.AutoMappings.Add(AutoMap.AssemblyOf<Order>(new AutomappingConfiguration())
                .IgnoreBase(typeof(EventLog<,,>))
                .IgnoreBase(typeof(OrderParameterEventLog<>))
                .Conventions.Add(
                    DefaultLazy.Never()
                )));

            _sessionFactory = _configuration
                .ExposeConfiguration(v => new SchemaExport(v).Create(false, false))
                .BuildSessionFactory();
        }

        internal ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }

        internal void ClearDatabase(bool execute = true)
        {
            _configuration.ExposeConfiguration(v => new SchemaExport(v).Create(true, execute));
        }
    }
}
