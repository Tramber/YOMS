using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Oms.Server.Core.Services.Filtering;
using Oms.Transport.Contracts.Dto;

namespace Oms.Server.Core.Tests.Services
{
    [TestFixture]
    public class TypeSafeFilterInterpreterTests
    {
        private sealed class CannedHandler(string json) : HttpMessageHandler
        {
            public string RequestBody;

            protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
                return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json) };
            }
        }

        private readonly List<FundDto> _funds = [new FundDto { Id = 1, Name = "Alpha Fund", Portfolio = "ALPHA" }, new FundDto { Id = 2, Name = "Beta Fund", Portfolio = "BETA" }];
        private readonly List<SecurityDto> _securities = [new SecurityDto { Id = 7, Name = "ACME Corp", IsinCode = "FR0000000001", BloombergTicker = "ACME FP" }];

        [Test]
        public async Task The_chosen_options_become_ids_and_none_restricts_nothing()
        {
            var handler = new CannedHandler("""
                {"answers":{"way":{"type":"choice","choice":"buy"},"fund":{"type":"choice","choice":"Alpha Fund"},
                 "security":{"type":"choice","choice":"none"},"status":{"type":"choice","choice":"none"}}}
                """);
            var interpreter = new TypeSafeFilterInterpreter(new HttpClient(handler) { BaseAddress = new System.Uri("http://x/") });

            var filter = await interpreter.InterpretAsync("alpha fund buys", _funds, _securities);

            Assert.That(filter.Way, Is.EqualTo(OrderDtoWay.Buy));
            Assert.That(filter.FundId, Is.EqualTo(1));
            Assert.That(filter.SecurityId, Is.Null);
            Assert.That(filter.Status, Is.Null);
            Assert.That(handler.RequestBody, Does.Contain("alpha fund buys").And.Contain("Beta Fund").And.Contain("ACME FP"));
        }
    }
}
