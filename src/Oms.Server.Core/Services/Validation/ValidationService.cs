using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FluentValidation;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;

namespace Oms.Server.Core.Services.Validation
{
    public class ValidationService : IValidationService
    {
        private readonly OrderValidator _orderValidator = new OrderValidator();
        private readonly SecurityValidator _securityValidator = new SecurityValidator();

        public GenericResult IsValid(IOrder order)
        {
            return IsValid(_orderValidator, order);
        }

        public GenericResult IsValid(IOrderCoreData orderCoreData)
        {
            return IsValid(_orderValidator.OrderCoreValidator, orderCoreData);
        }

        public GenericResult IsValid(IOrderEditableData orderEditableData)
        {
            return IsValid(_orderValidator.OrderEditableValidator, orderEditableData);
        }
        public GenericResult IsValid(ISecurity security)
        {
            return IsValid(_securityValidator, security);
        }

        private GenericResult IsValid<T>(AbstractValidator<T> validator, T item)
        {
            Contract.Requires(item != null, "item != null");

            var result = validator.Validate(item);
            return result.IsValid ? GenericResult.Success()
                : GenericResult.FailureFormat(result.Errors.Aggregate(new StringBuilder(), (s, v) => { s.AppendFormat("{0} : {1}", v.PropertyName, v.ErrorMessage); return s; }).ToString());
        }
    }
}