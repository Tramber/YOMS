using System;
using Oms.Server.Domain.Assets;
using Oms.Server.Domain.Orders;
using Oms.Server.Domain.Users;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core
{
    public static class DtoMapperExtensions
    {
        public static Order ToDomain(this OrderDto orderDto)
        {
            return new Order
            {
                Id = orderDto.Id,
                Price = orderDto.Price,
                Quantity = orderDto.Quantity,
                //Asset = orderDto.Asset.ToDomain(),
                //CreationDate = orderDto.CreationDate,
                //Creator = orderDto.Creator.ToDomain(),
                //ExpiryDate = orderDto.ExpiryDate,
                //OrderType = orderDto.OrderType.ToMappingEnum<OrderType>(),
                //OrderValidity = orderDto.Validity.ToMappingEnum<OrderValidityType>(),
                //Owner = orderDto.Owner.ToDomain(),
                //Version = orderDto.Version,
                //Way = orderDto.Way.ToMappingEnum<OrderWay>()
            };
        }

        public static Asset ToDomain(this AssetDto assetDto)
        {
            return new Asset
            {
                BloombergTicker = assetDto.BloombergTicker,
                CreationDate = assetDto.CreationDate,
                Creator = assetDto.Creator.ToDomain(),
                Currency = assetDto.Currency.ToMappingEnum<Currency>(),
                Id = assetDto.Id,
                IsinCode = assetDto.IsinCode,
                IsActive = assetDto.IsActive,
                Name = assetDto.Name,
                Origin = assetDto.Origin.ToMappingEnum<AssetOriginType>(),
                Type = assetDto.Type.ToMappingEnum<AssetType>()
            };
        }

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
                //Asset = order.Asset.ToDto(),
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

        public static AssetDto ToDto(this Asset asset)
        {
            return new AssetDto
            {
                BloombergTicker = asset.BloombergTicker,
                CreationDate = asset.CreationDate,
                Creator = asset.Creator.ToDto(),
                Currency = asset.Currency.ToMappingEnum<CurrencyDto>(),
                Id = asset.Id,
                IsinCode = asset.IsinCode,
                IsActive = asset.IsActive,
                Name = asset.Name,
                Origin = asset.Origin.ToMappingEnum<AssetOriginDtoType>(),
                Type = asset.Type.ToMappingEnum<AssetDtoType>()
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
