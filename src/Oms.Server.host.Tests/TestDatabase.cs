using System;
using System.IO;
using NUnit.Framework;

// No namespace: runs once for the whole assembly, before the first host is created.
[SetUpFixture]
public class TestDatabase
{
    [OneTimeSetUp]
    public void UseAFreshDatabase()
    {
        // ConfigurationManager would otherwise read testhost.dll.config; a file from an older run could have an old schema.
        AppContext.SetData("APP_CONFIG_FILE", Path.Combine(AppContext.BaseDirectory, "App.config"));
        var database = Path.Combine(Directory.GetCurrentDirectory(), "oms.sqlite");
        if (File.Exists(database))
            File.Delete(database);
    }
}
