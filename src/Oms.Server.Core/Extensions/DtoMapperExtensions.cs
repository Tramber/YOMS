using System;
using System.Linq;
using Oms.Server.Domain.Framework;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Transport.Contracts.Dto;
using Oms.Transport.Contracts.Messages;

namespace Oms.Server.Core.Extensions
{
    /// <summary>Maps domain objects to the DTOs sent to the clients.</summary>
    public static class DtoMapperExtensions
    {
        /// <summary>Converts an enum value to the enum of the same member name. Throws <see cref="NotSupportedException"/> when the target has no such member.</summary>
        public static TEnum ToMappingEnum<TEnum>(this Enum origin) where TEnum : struct
        {
            TEnum destination;
            if(!Enum.TryParse(origin.ToString(), true, out destination))
                throw new NotSupportedException();
            return destination;
        }

        public static OrderDto ToDto(this IOrder order)
        {
            var dto = new OrderDto
            {
                Id = order.Id,
                PriceLimit = order.PriceLimit,
                PriceStop = order.PriceStop,
                Quantity = order.Quantity,
                Way = order.Side.ToMappingEnum<OrderDtoWay>(),
                OrderType = order.OrderType.ToMappingEnum<OrderDtoType>(),
                Validity = order.OrderValidity.ToMappingEnum<OrderDtoValidityType>(),
                ExpiryDate = order.ExpiryDate,
                SettlementDate = order.SettlementDate,
                ClientOrderRef = order.ClientOrderRef,
                CreationDate = order.CreationDate,
                Owner = order.Owner == null ? null : order.Owner.ToDto(),
                Creator = order.Creator == null ? null : order.Creator.ToDto(),
                Security = order.Security == null ? null : order.Security.ToDto(),
                Fund = order.Fund == null ? null : order.Fund.ToDto(),
            };
            // Orders know their state; a plain IOrder may not.
            var computed = order as IOrderComputedData;
            if (computed != null)
            {
                dto.State = computed.OrderState.ToString();
                dto.Status = computed.OrderStatus.ToString();
                dto.RemainingQuantity = computed.RemainingQuantity;
            }
            var withTrades = order as Oms.Server.Domain.Models.Orders.Order;
            if (withTrades != null)
                dto.Trades = withTrades.GetTrades().Select(t => t.ToDto()).ToList();
            return dto;
        }

        public static TradeDto ToDto(this ITrade trade)
        {
            return new TradeDto
            {
                Id = trade.Id,
                OrderId = trade.Order == null ? 0 : trade.Order.Id,
                Way = trade.Side.ToMappingEnum<OrderDtoWay>(),
                ExecutionQuantity = trade.ExecutionQuantity,
                ExecutionAmountNet = trade.ExecutionAmountNet,
                State = trade.TradeState.ToString(),
            };
        }

        public static FundDto ToDto(this IFund fund)
        {
            return new FundDto
            {
                Id = fund.Id,
                Name = fund.Name,
                Portfolio = fund.Portfolio,
                Depositary = fund.Depositary,
            };
        }

        public static GenericResultDto ToDto(this GenericResult result)
        {
            return new GenericResultDto(result.ErrorMessage, result.ErrorCode);
        }

        public static SecurityDto ToDto(this ISecurity security)
        {
            return new SecurityDto
            {
                BloombergTicker = security.BloombergTicker,
                CreationDate = security.CreationDate,
                Creator = security.Creator == null ? null : security.Creator.ToDto(),
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
