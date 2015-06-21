using FluentValidation;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Validation
{
    public class OrderValidator : AbstractValidator<IOrder>
    {
        public OrderCoreValidator OrderCoreValidator { get; private set; }

        public OrderEditableValidator OrderEditableValidator { get; private set; }

        public OrderValidator()
        {
            OrderCoreValidator = new OrderCoreValidator();
            OrderEditableValidator = new OrderEditableValidator();

            RuleFor<IOrderCoreData>(o => o).SetValidator(OrderCoreValidator);
            RuleFor<IOrderEditableData>(o => o).SetValidator(OrderEditableValidator);
        }
    }
}