using FluentValidation;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Validation
{
    public class OrderCoreValidator : AbstractValidator<IOrderCoreData>
    {
        public OrderCoreValidator()
        {
            RuleFor(o => o.CreationDate).NotEmpty();
            RuleFor(o => o.Creator).NotEmpty();
            RuleFor(o => o.Owner).NotEmpty();
            RuleFor(o => o.IsAutoAccepted).NotEmpty();
        }
    }


    public class SecurityValidator : AbstractValidator<ISecurity>
    {
        public SecurityValidator()
        {
            RuleFor(s => s.CreationDate).NotEmpty();
            RuleFor(s => s.Creator).NotEmpty();
            RuleFor(s => s.BloombergTicker).NotEmpty();
        }
    }
}