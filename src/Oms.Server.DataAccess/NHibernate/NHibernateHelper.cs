using System;
using System.Configuration;
using System.IO;
using System.Security.Cryptography;
using System.Text;
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
        private bool _isSqlite;
        private bool _isPostgres;
        private int _schemaVersion;
        private string _schemaScript;
        public static readonly NHibernateHelper Instance = new NHibernateHelper();


        private IPersistenceConfigurer GetDatabaseConfigurer()
        {
            var connectionNode = ConfigurationManager.ConnectionStrings["OmsDB"];
            if (connectionNode == null)
                throw new ConfigurationErrorsException("The connection string 'OmsDB' is missing from the application configuration (App.config).");
            switch (connectionNode.ProviderName)
            {
                case "SQLite" :
                    _isSqlite = true;
                    var databaseFile = new FileInfo(connectionNode.ConnectionString);
                    // A SQLite file that is missing or empty has no tables yet. Existing data is never dropped.
                    _createSchema = !databaseFile.Exists || databaseFile.Length == 0;
                    return SQLiteConfiguration.Standard
                        .ConnectionString("Data Source=" + connectionNode.ConnectionString)
                        .Driver<MicrosoftDataSqliteDriver>();
                case "PostgreSQL" :
                    _isPostgres = true;
                    return PostgreSQLConfiguration.PostgreSQL82.ConnectionString(connectionNode.ConnectionString);
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
                .ExposeConfiguration(v => { if (_isSqlite) v.SetProperty(global::NHibernate.Cfg.Environment.Hbm2ddlKeyWords, "none"); })
                .ExposeConfiguration(v =>
                {
                    _schemaVersion = ComputeSchemaVersion(v);
                    if (_createSchema) new SchemaExport(v).Create(false, true);
                    // PostgreSQL can describe its schema, so missing tables are added; existing ones are left alone.
                    if (_isPostgres) new SchemaUpdate(v).Execute(false, true);
                })
                .BuildSessionFactory();

            if (_isSqlite) CheckSchemaVersion();
        }

        /// <summary>A fingerprint of the DDL the mappings generate, small enough for SQLite's <c>user_version</c>.</summary>
        private int ComputeSchemaVersion(global::NHibernate.Cfg.Configuration configuration)
        {
            var script = new StringBuilder();
            new SchemaExport(configuration).Create(line => script.AppendLine(line), false);
            _schemaScript = script.ToString();
            return BitConverter.ToInt32(SHA256.HashData(Encoding.UTF8.GetBytes(_schemaScript)), 0);
        }

        /// <summary>
        /// Microsoft.Data.Sqlite cannot describe its schema, so NHibernate cannot validate or update it. The schema
        /// fingerprint is stored in the file when it is created and compared on every later start.
        /// </summary>
        private void CheckSchemaVersion()
        {
            using (var session = _sessionFactory.OpenSession())
            {
                if (_createSchema)
                {
                    session.CreateSQLQuery("PRAGMA user_version = " + _schemaVersion).ExecuteUpdate();
                    return;
                }
                var stored = Convert.ToInt32(session.CreateSQLQuery("PRAGMA user_version").UniqueResult());
                if (stored != _schemaVersion)
                    throw new InvalidOperationException("The SQLite database was created for another version of the mappings (schema " + stored + ", expected " + _schemaVersion + "). Delete the file to create it again.");
            }
        }

        internal ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }

        /// <summary>The DDL the mappings generate.</summary>
        internal string GetSchemaScript()
        {
            return _schemaScript;
        }
    }
}
