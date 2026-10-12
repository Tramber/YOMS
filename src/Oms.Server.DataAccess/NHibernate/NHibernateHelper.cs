using System;
using System.Configuration;
using System.IO;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Tool.hbm2ddl;
using Oms.Server.DataAccess.NHibernate.Mapping;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Orders;

namespace Oms.Server.DataAccess.NHibernate
{
    internal class NHibernateHelper
    {
        private readonly FluentConfiguration _configuration;
        private readonly ISessionFactory _sessionFactory;
        private bool _createSchema;
        public static readonly NHibernateHelper Instance = new NHibernateHelper();


        private IPersistenceConfigurer GetDatabaseConfigurer()
        {
            var connectionNode = ConfigurationManager.ConnectionStrings["OmsDB"];
            switch (connectionNode.ProviderName)
            {
                case "SQLite" :
                    var databaseFile = new FileInfo(connectionNode.ConnectionString);
                    // A SQLite file that is missing or empty has no tables yet. Existing data is never dropped.
                    _createSchema = !databaseFile.Exists || databaseFile.Length == 0;
                    return SQLiteConfiguration.Standard
                        .ConnectionString("Data Source=" + connectionNode.ConnectionString)
                        .Driver<MicrosoftDataSqliteDriver>();
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
                .Mappings(m => m.AutoMappings.Add(AutoMap.Source(new ExplicitTypeSource(typeof(Order), AutomappingConfiguration.ClosedGenericTypes), new AutomappingConfiguration())
                .IgnoreBase(typeof(EventLog<,,>))
                .UseOverridesFromAssemblyOf<OrderAutoMappingOverride>()
                .Conventions.Add(
                    DefaultLazy.Never(),
                    new InterfaceReferenceConvention(),
                    new EventLogDiscriminatorConvention()
                )));

            _sessionFactory = _configuration
                // Microsoft.Data.Sqlite cannot describe its schema (GetSchema), which NHibernate uses to read the reserved words.
                .ExposeConfiguration(v => v.SetProperty(global::NHibernate.Cfg.Environment.Hbm2ddlKeyWords, "none"))
                .ExposeConfiguration(v =>
                {
                    if (_createSchema) new SchemaExport(v).Create(false, true);
                })
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
