using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentNHibernate.Automapping;
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using FluentNHibernate.Conventions.Helpers;
using NHibernate;
using NHibernate.Tool.hbm2ddl;

namespace Oms.Server.DataAccess.NHibernate
{
    internal class NHibernateHelper
    {
        private readonly FluentConfiguration _configuration;
        private readonly ISessionFactory _sessionFactory;
        public static readonly NHibernateHelper Instance = new NHibernateHelper();

        private NHibernateHelper()
        {
            _configuration = Fluently.Configure()
                .Database(SQLiteConfiguration.Standard.UsingFile(@"c:\mydb.db"))
                .Mappings(m => m.AutoMappings.Add(new AutoPersistenceModel()));
            //    .Conventions.Add(DefaultLazy.Never()));

            _sessionFactory = _configuration
                .ExposeConfiguration(v => new SchemaExport(v).Create(false, false))
                .BuildSessionFactory();
        }

        public ISession OpenSession()
        {
            return _sessionFactory.OpenSession();
        }

        public void ClearDatabase()
        {
            _configuration.ExposeConfiguration(v => new SchemaExport(v).Create(true, true));
        }
    }
}
