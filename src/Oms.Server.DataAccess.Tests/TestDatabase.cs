using System;
using System.IO;
using NUnit.Framework;
using Testcontainers.PostgreSql;

// No namespace: runs once for the whole assembly, before the first repository is created.
// SQLite by default; OMS_TEST_DB=postgres runs the same tests against a PostgreSQL container (needs Docker).
[SetUpFixture]
public class TestDatabase
{
    private PostgreSqlContainer _postgres;

    [OneTimeSetUp]
    public async System.Threading.Tasks.Task UseAFreshDatabase()
    {
        // ConfigurationManager would otherwise read testhost.dll.config.
        var config = Path.Combine(AppContext.BaseDirectory, "App.config");
        if (Environment.GetEnvironmentVariable("OMS_TEST_DB") == "postgres")
        {
            _postgres = new PostgreSqlBuilder().WithImage("postgres:17-alpine").Build();
            await _postgres.StartAsync();
            config = Path.Combine(AppContext.BaseDirectory, "App.postgres.config");
            File.WriteAllText(config, "<configuration><connectionStrings><add name=\"OmsDB\" providerName=\"PostgreSQL\" connectionString=\""
                + _postgres.GetConnectionString() + "\" /></connectionStrings></configuration>");
        }
        AppContext.SetData("APP_CONFIG_FILE", config);
        // The schema is only created for a new or empty SQLite file; a file from an older run could have an old schema.
        var database = Path.Combine(Directory.GetCurrentDirectory(), "oms.sqlite");
        if (File.Exists(database))
            File.Delete(database);
    }

    [OneTimeTearDown]
    public async System.Threading.Tasks.Task StopTheContainer()
    {
        if (_postgres != null) await _postgres.DisposeAsync();
    }
}
