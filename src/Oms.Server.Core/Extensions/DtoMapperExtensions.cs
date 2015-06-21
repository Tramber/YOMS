using System;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Extensions
{
    public static class DtoMapperExtensions
    {
        public static TEnum ToMappingEnum<TEnum>(this Enum origin) where TEnum : struct
        {
            TEnum destination;
            if(Enum.TryParse(origin.ToString(), true, out destination))
                throw new NotSupportedException();
            return destination;
        }

        public static OrderDto ToDto(this IOrder order)
        {
            return new OrderDto
            {
                Id = order.Id,
                PriceLimit = order.PriceLimit,
                Quantity = order.Quantity,
                OrderType = order.OrderType.ToMappingEnum<OrderDtoType>(),
                ClientOrderRef = order.ClientOrderRef,
                CreationDate = order.CreationDate,
            };
        }

        public static SecurityDto ToDto(this ISecurity security)
        {
            return new SecurityDto
            {
                BloombergTicker = security.BloombergTicker,
                CreationDate = security.CreationDate,
                Creator = security.Creator.ToDto(),
                Currency = security.Currency.ToMappingEnum<CurrencyDto>(),
                Id = security.Id,
                IsinCode = security.IsinCode,
                IsActive = security.IsActive,
                Name = security.Name,
                Type = security.Type.ToMappingEnum<SecurityDtoType>()
            };
        }

        public static UserDto ToDto(this IUser user)
        {
            return new UserDto
            {
                FirstName = user.FirstName,
                Id = user.Id,
                LastName = user.LastName,
                Login = user.Login
            };
        }

    }
}
