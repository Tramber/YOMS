using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Oms.Client.Framework.Observables;
using Oms.Client.Model;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Tests.Framework.Observables
{
    [TestFixture]
    public class FastSearchTests
    {
        private List<OrderAdapter> orders = new List<OrderAdapter>
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
            return orders.Count(s => fastSearch.Predicate(s));
        }

        [TestCase("IsinCode = \"MyIsinCode1\"", ExpectedResult = 2)]
        [TestCase("Security.IsinCode = \"MyIsinCode2\"", ExpectedResult = 1)]
        public int Should_filter_by_isin_when_isin_only_is_set_in_string(string filterSearch)
        {
            var fastSearch = new FastSearch<OrderAdapter>();
            fastSearch.AddPredicate(filterSearch);
            return orders.Count(s => fastSearch.Predicate(s));
        }
    }
}
