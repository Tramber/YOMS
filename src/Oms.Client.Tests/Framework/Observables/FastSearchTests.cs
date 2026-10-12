using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Oms.Client.Framework.Observables;
using Oms.Client.Model;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Tests.Framework.Observables
{
    [TestFixture]
    public class FastSearchTests
    {
        private List<OrderAdapter> _orders = new()
        {
            new OrderAdapter { Security = new SecurityDto() { IsinCode = "MyIsinCode1"}},
            new OrderAdapter { Security = new SecurityDto() { IsinCode = "MyIsinCode2"}},
            new OrderAdapter { Security = new SecurityDto() { IsinCode = "MyIsinCode3"}},
            new OrderAdapter { Security = new SecurityDto() { IsinCode = "MyIsinCode1"}},
        };



        [TestCase("MyIsinCode1", ExpectedResult = 2)]
        [TestCase("MyIsinCode2", ExpectedResult = 1)]
        public int Should_filter_by_isin_when_isin_only_is_set(string isinCode)
        {
            var fastSearch = new FastSearch<OrderAdapter>();
            fastSearch.AddPredicate(o => o.Security.IsinCode == isinCode);
            return _orders.Count(s => fastSearch.Predicate(s));
        }

        [TestCase("IsinCode = \"MyIsinCode1\"", ExpectedResult = 2)]
        [TestCase("Security.IsinCode = \"MyIsinCode2\"", ExpectedResult = 1)]
        public int Should_filter_by_isin_when_isin_only_is_set_in_string(string filterSearch)
        {
            var fastSearch = new FastSearch<OrderAdapter>();
            fastSearch.AddPredicate(filterSearch);
            return _orders.Count(s => fastSearch.Predicate(s));
        }

        [Test]
        public void Should_not_match_when_a_nested_value_is_missing()
        {
            var fastSearch = new FastSearch<OrderAdapter>();
            fastSearch.AddPredicate("Security.IsinCode = \"MyIsinCode1\"");

            Assert.That(fastSearch.Predicate(new OrderAdapter()), Is.False);
        }

        [Test]
        public void Should_match_everything_without_predicates_and_after_clear()
        {
            var fastSearch = new FastSearch<OrderAdapter>();
            fastSearch.AddPredicate("IsinCode = \"MyIsinCode1\"");
            fastSearch.Clear();

            Assert.That(_orders.All(fastSearch.Predicate), Is.True);
        }

        [Test]
        public void Should_require_every_predicate_to_match()
        {
            var fastSearch = new FastSearch<OrderAdapter>();
            fastSearch.AddPredicate(o => o.Security.IsinCode == "MyIsinCode1");
            fastSearch.AddPredicate(o => o.Security.IsinCode == "MyIsinCode2");

            Assert.That(_orders.Any(fastSearch.Predicate), Is.False);
        }
    }
}
