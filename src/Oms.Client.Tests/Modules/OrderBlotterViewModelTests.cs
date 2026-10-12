using System.Linq;
using NUnit.Framework;
using Oms.Client.Model;
using Oms.Client.Modules.OrderBlotter;
using Oms.Transport.Contracts.Dto;

namespace Oms.Client.Tests.Modules
{
    [TestFixture]
    public class OrderBlotterViewModelTests
    {
        private OrderBlotterViewModel _blotter;

        [SetUp]
        public void SetUp()
        {
            _blotter = new OrderBlotterViewModel();
        }

        private OrderAdapter[] Rows()
        {
            return _blotter.OrdersView.Cast<object>().OfType<OrderAdapter>().ToArray();
        }

        [Test]
        public void Should_start_with_sample_orders_in_five_groups()
        {
            var rows = Rows();

            Assert.That(rows, Has.Length.EqualTo(50));
            Assert.That(rows.Select(r => r.GroupId).Distinct().Count(), Is.EqualTo(5));
        }

        [Test]
        public void Should_add_the_orders_pushed_by_the_server()
        {
            _blotter.AddOrders(new[] { new OrderDto { Id = 900, Quantity = 10, PriceLimit = 1.5 } });

            Assert.That(Rows().Single(r => r.Id == 900).Quantity, Is.EqualTo(10));
            Assert.That(Rows().Single(r => r.Id == 900).Price, Is.EqualTo(1.5));
        }

        [Test]
        public void Should_group_ungrouped_orders_under_a_new_group_id()
        {
            _blotter.AddOrders(new[] { new OrderDto { Id = 901 }, new OrderDto { Id = 902 } });
            var selected = Rows().Where(r => r.Id >= 901).ToList();

            _blotter.GroupOrdersCommand.Execute(selected);

            Assert.That(selected.Select(o => o.GroupId).Distinct().Count(), Is.EqualTo(1));
            Assert.That(selected[0].GroupId, Is.GreaterThanOrEqualTo(4000));
        }

        [Test]
        public void Should_not_group_orders_when_one_is_already_grouped()
        {
            var selected = Rows().Where(r => r.GroupId == 0).Take(1).Concat(Rows().Where(r => r.GroupId != 0).Take(1)).ToList();
            var before = selected.Select(o => o.GroupId).ToList();

            _blotter.GroupOrdersCommand.Execute(selected);

            Assert.That(selected.Select(o => o.GroupId).ToList(), Is.EqualTo(before));
        }

        [Test]
        public void Should_ungroup_grouped_orders()
        {
            var selected = Rows().Where(r => r.GroupId != 0).Take(3).ToList();

            _blotter.UnGroupOrdersCommand.Execute(selected);

            Assert.That(selected.All(o => o.GroupId == 0), Is.True);
        }

        [Test]
        public void Should_not_ungroup_when_one_order_is_not_grouped()
        {
            var selected = Rows().Where(r => r.GroupId == 0).Take(1).Concat(Rows().Where(r => r.GroupId != 0).Take(1)).ToList();
            var grouped = selected.Last();
            var groupId = grouped.GroupId;

            _blotter.UnGroupOrdersCommand.Execute(selected);

            Assert.That(grouped.GroupId, Is.EqualTo(groupId));
        }

        [Test]
        public void Should_filter_the_rows_with_a_property_expression()
        {
            _blotter.FilterText = "Version = \"none\"";

            Assert.That(Rows(), Is.Empty);
        }

        [Test]
        public void Should_treat_a_missing_nested_value_as_a_non_match()
        {
            _blotter.FilterText = "Security.IsinCode = \"FR0000\"";

            Assert.That(Rows(), Is.Empty);
        }

        [TestCase("not an expression")]
        [TestCase("Id = \"5\"")]
        [TestCase("")]
        public void Should_show_every_row_when_the_filter_is_empty_or_invalid(string text)
        {
            _blotter.FilterText = "Version = \"none\"";

            _blotter.FilterText = text;

            Assert.That(Rows(), Has.Length.EqualTo(50));
        }
    }
}
