using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Oms.Server.Domain.Interfaces.Models;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Services.Filtering
{
    /// <summary>
    /// <see cref="IFilterInterpreter"/> on the TypeSafe API (https://docs.typesafe.ai/api): one Choice question per filterable
    /// field, each picking among the real values or "none". The model only selects; the ids come from our lists.
    /// </summary>
    public class TypeSafeFilterInterpreter : IFilterInterpreter
    {
        public const string None = "none";
        private readonly HttpClient _http;

        public TypeSafeFilterInterpreter(HttpClient http)
        {
            _http = http;
        }

        public TypeSafeFilterInterpreter(string apiKey)
            : this(new HttpClient { BaseAddress = new Uri("https://api.typesafe.ai/") })
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        }

        public async Task<OrderFilterDto> InterpretAsync(string text, IReadOnlyList<FundDto> funds, IReadOnlyList<SecurityDto> securities)
        {
            var response = await _http.PostAsJsonAsync("v1/systemone", BuildRequest(text, funds, securities));
            response.EnsureSuccessStatusCode();
            return ReadAnswers(await response.Content.ReadFromJsonAsync<JsonElement>(), funds, securities);
        }

        public static JsonObject BuildRequest(string text, IReadOnlyList<FundDto> funds, IReadOnlyList<SecurityDto> securities)
        {
            return new JsonObject
            {
                ["state"] = text,
                ["model"] = "jev-latest",
                ["questions"] = new JsonObject
                {
                    ["way"] = Choice("Does the filter text restrict the orders to buys or to sells?", new Dictionary<string, string>
                    {
                        ["buy"] = "Only buy orders are asked for",
                        ["sell"] = "Only sell orders are asked for",
                        [None] = "Neither side is mentioned"
                    }),
                    ["fund"] = Choice("Which fund does the filter text restrict the orders to?",
                        funds.GroupBy(f => f.Name).ToDictionary(g => g.Key, g => "Portfolio " + g.First().Portfolio)),
                    ["security"] = Choice("Which security does the filter text restrict the orders to? It may be named by its name, ISIN or ticker.",
                        securities.GroupBy(s => s.Name).ToDictionary(g => g.Key, g => "ISIN " + g.First().IsinCode + ", ticker " + g.First().BloombergTicker)),
                    ["status"] = Choice("Which order status does the filter text restrict the orders to?",
                        Enum.GetNames<OrderStatus>().Where(n => n != nameof(OrderStatus.Undefined)).ToDictionary(n => n, n => (string)null))
                }
            };
        }

        public static OrderFilterDto ReadAnswers(JsonElement response, IReadOnlyList<FundDto> funds, IReadOnlyList<SecurityDto> securities)
        {
            var answers = response.GetProperty("answers");
            string Picked(string id)
            {
                var choice = answers.GetProperty(id).GetProperty("choice").GetString();
                return choice == None ? null : choice;
            }
            var way = Picked("way");
            return new OrderFilterDto
            {
                Way = way == null ? (OrderDtoWay?)null : way == "buy" ? OrderDtoWay.Buy : OrderDtoWay.Sell,
                FundId = funds.FirstOrDefault(f => f.Name == Picked("fund"))?.Id,
                SecurityId = securities.FirstOrDefault(s => s.Name == Picked("security"))?.Id,
                Status = Picked("status")
            };
        }

        // The "none" option is added to every question so that text which does not mention a field restricts nothing.
        private static JsonObject Choice(string instructions, Dictionary<string, string> options)
        {
            var criteria = new JsonObject();
            foreach (var option in options)
                criteria[option.Key] = option.Value;
            if (!criteria.ContainsKey(None))
                criteria[None] = "The filter text does not mention it";
            return new JsonObject { ["type"] = "choice", ["instructions"] = instructions, ["criteria"] = criteria };
        }
    }
}
