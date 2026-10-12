using NHibernate.Driver;

namespace Oms.Server.DataAccess.NHibernate
{
    /// <summary>NHibernate driver for Microsoft.Data.Sqlite (System.Data.SQLite is not available on .NET 10).</summary>
    public class MicrosoftDataSqliteDriver : ReflectionBasedDriver
    {
        public MicrosoftDataSqliteDriver()
            : base("Microsoft.Data.Sqlite", "Microsoft.Data.Sqlite.SqliteConnection", "Microsoft.Data.Sqlite.SqliteCommand")
        {
        }

        public override bool UseNamedPrefixInSql => true;
        public override bool UseNamedPrefixInParameter => true;
        public override string NamedPrefix => "@";
        public override bool SupportsMultipleOpenReaders => false;
    }
}
