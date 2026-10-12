using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using FluentValidation;
using Oms.Server.Domain.Framework;

namespace Oms.Server.Domain.Validation
{
    public static class ValidationExtensions
    {
        /// <summary>Validates the item and returns a <see cref="GenericResult"/> whose error message lists the failed properties.</summary>
        public static GenericResult IsValid<T>(this AbstractValidator<T> validator, T item)
        {
            Contract.Requires(item != null, "item != null");

            var result = validator.Validate(item);
            return result.IsValid ? GenericResult.Success()
                : GenericResult.FailureFormat(result.Errors.Aggregate(new StringBuilder(), (s, v) => { s.AppendFormat("{0} : {1}", v.PropertyName, v.ErrorMessage); return s; }).ToString());
        }


    }
}