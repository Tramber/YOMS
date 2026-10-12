using System.IO;
using System.Reflection;
using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Oms.Server.Core.Contracts;
using Oms.Server.DataAccess.NHibernate.Repositories;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Core.Services;
using Oms.Transport.Contracts;

XmlConfigurator.Configure(LogManager.GetRepository(Assembly.GetEntryAssembly()), new FileInfo("log4net.config"));

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:9001");
builder.Services.AddSignalR();
builder.Services.AddSingleton<IRepositoryFacade, RepositoryFacade>();
builder.Services.AddSingleton<IOrderManagementService, OrderManagementService>();
builder.Services.AddSingleton<OrderServiceContract>();
builder.Services.AddSingleton<IOrderServiceContract>(sp => sp.GetRequiredService<OrderServiceContract>());

var app = builder.Build();
app.MapHub<OrderHub>(OrderHubMethods.Path);

// Push order changes raised by the service to every subscribed client.
var hub = app.Services.GetRequiredService<IHubContext<OrderHub>>();
app.Services.GetRequiredService<OrderServiceContract>().OrderListUpdated += (type, orders) =>
    hub.Clients.Group(OrderHub.SubscribersGroup).SendAsync(OrderHubMethods.OrderListUpdated, type, orders);

app.Run();
