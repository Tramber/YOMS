using System;
using Oms.Server.Domain.Models.Instruments;
using Oms.Server.Domain.Models.Orders;
using Oms.Server.Domain.Models.Users;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core
{
    public static class DtoMapperExtensions
    {
        public static User ToDomain(this UserDto userDto)
        {
            return new User
            {
                FirstName = userDto.FirstName,
                Id = userDto.Id,
                LastName = userDto.LastName,
                Login = userDto.Login
            };
        }

        private static TEnum ToMappingEnum<TEnum>(this Enum origin) where TEnum : struct
        {
            TEnum destination;
            if(Enum.TryParse(origin.ToString(), true, out destination))
                throw new NotSupportedException();
            return destination;
        }

        public static OrderDto ToDto(this Order order)
        {
            return new OrderDto
            {
                Id = order.Id,
                Price = order.Price,
                Quantity = order.Quantity,
                //Instrument = order.Instrument.ToDto(),
                //CreationDate = order.CreationDate,
                //Creator = order.Creator.ToDto(),
                //ExpiryDate = order.ExpiryDate,
                //OrderType = order.OrderType.ToMappingEnum<OrderDtoType>(),
                //Validity = order.OrderValidity.ToMappingEnum<OrderDtoValidityType>(),
                //Owner = order.Owner.ToDto(),
                //Version = order.Version,
                //Way = order.Way.ToMappingEnum<OrderDtoWay>()
            };
        }

        public static AssetDto ToDto(this Instrument instrument)
        {
            return new AssetDto
            {
                BloombergTicker = instrument.BloombergTicker,
                CreationDate = instrument.CreationDate,
                Creator = instrument.Creator.ToDto(),
                Currency = instrument.Currency.ToMappingEnum<CurrencyDto>(),
                Id = instrument.Id,
                IsinCode = instrument.IsinCode,
                IsActive = instrument.IsActive,
                Name = instrument.Name,
                Type = instrument.Type.ToMappingEnum<AssetDtoType>()
            };
        }

        public static UserDto ToDto(this User user)
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
