using NUnit.Framework;
using Oms.Server.DataAccess.NHibernate;

namespace Oms.Server.DataAccess.Tests.NHibernate
{
    [TestFixture]
    [Explicit]
    public class ManualOperations
    {
        [Test]
        public void DisplaySchema()
        {
            TestContext.Out.WriteLine(NHibernateHelper.Instance.GetSchemaScript());
        }
    }
}
