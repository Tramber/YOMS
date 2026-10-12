using System;
using NUnit.Framework;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Models.Funds;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Securities;
using Oms.Server.Domain.Models.Users;
using Oms.Server.Domain.Validation;
using Oms.Server.Domain.Models.EventLogs;

namespace Oms.Server.Domain.Tests.Validation
{
    [TestFixture]
    public class ValidatorTests
    {
        private static OrderEditableEventParameter ValidEditable()
        {
            return new OrderEditableEventParameter { Fund = new Fund(), Security = new Security(), Quantity = 100 };
        }

        private static OrderCoreData ValidCore()
        {
            return new OrderCoreData { CreationDate = DateTime.Now, Creator = new User(), Owner = new User(), IsAutoAccepted = true };
        }

        [Test]
        public void Editable_data_with_fund_security_and_quantity_is_valid()
        {
            Assert.That(new OrderEditableValidator().IsValid(ValidEditable()).IsSuccess(), Is.True);
        }

        [Test]
        public void Editable_data_without_a_fund_is_invalid()
        {
            var data = ValidEditable();
            data.Fund = null;

            var result = new OrderEditableValidator().IsValid(data);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(result.ErrorMessage, Does.Contain("Fund"));
        }

        [Test]
        public void Editable_data_without_a_security_is_invalid()
        {
            var data = ValidEditable();
            data.Security = null;

            var result = new OrderEditableValidator().IsValid(data);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(result.ErrorMessage, Does.Contain("Security"));
        }

        [TestCase(0)]
        [TestCase(-1)]
        public void Editable_data_needs_a_positive_quantity(double quantity)
        {
            var data = ValidEditable();
            data.Quantity = quantity;

            var result = new OrderEditableValidator().IsValid(data);

            Assert.That(result.IsFailure(), Is.True);
            Assert.That(result.ErrorMessage, Does.Contain("Quantity"));
        }

        [Test]
        public void Core_data_with_creator_owner_and_date_is_valid()
        {
            var result = new OrderCoreValidator().IsValid(ValidCore());

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
        }

        [Test]
        public void Core_data_that_is_not_auto_accepted_is_valid()
        {
            var core = ValidCore();
            core.IsAutoAccepted = false;

            var result = new OrderCoreValidator().IsValid(core);

            Assert.That(result.IsSuccess(), Is.True, result.ErrorMessage);
        }

        [Test]
        public void Core_data_without_a_creator_is_invalid()
        {
            var core = ValidCore();
            core.Creator = null;

            Assert.That(new OrderCoreValidator().IsValid(core).IsFailure(), Is.True);
        }

        [Test]
        public void Core_data_without_an_owner_is_invalid()
        {
            var core = ValidCore();
            core.Owner = null;

            Assert.That(new OrderCoreValidator().IsValid(core).IsFailure(), Is.True);
        }

        [Test]
        public void Core_data_without_a_creation_date_is_invalid()
        {
            var core = ValidCore();
            core.CreationDate = default(DateTime);

            Assert.That(new OrderCoreValidator().IsValid(core).IsFailure(), Is.True);
        }
    }
}
