using FluentValidation;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Validation
{
    public class OrderEditableValidator : AbstractValidator<IOrderEditableData>
    {
        public OrderEditableValidator()
        {
            RuleFor(o => o.Fund).NotNull();
            RuleFor(o => o.Security).NotNull();
            RuleFor(o => o.Quantity).GreaterThan(0);
        }
    }
}