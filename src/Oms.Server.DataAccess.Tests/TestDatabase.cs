using System;
using System.IO;
using NUnit.Framework;

// No namespace: runs once for the whole assembly, before the first repository is created.
[SetUpFixture]
public class TestDatabase
{
    [OneTimeSetUp]
    public void UseAFreshDatabase()
    {
        // ConfigurationManager would otherwise read testhost.dll.config.
        AppContext.SetData("APP_CONFIG_FILE", Path.Combine(AppContext.BaseDirectory, "App.config"));
        // The schema is only created for a new or empty SQLite file; a file from an older run could have an old schema.
        var database = Path.Combine(Directory.GetCurrentDirectory(), "oms.sqlite");
        if (File.Exists(database))
            File.Delete(database);
    }
}
