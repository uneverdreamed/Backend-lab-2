using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using Seismic.Alerts.Consumer.BackgroundServices;
using Seismic.Alerts.Consumer.Common;
using Seismic.Alerts.Consumer.DataAccess;
using Seismic.Alerts.Consumer.Options;
using Serilog;

Console.WriteLine("====================================================");
Console.WriteLine(" Seismic Alerts Consumer");
Console.WriteLine($" {MeInfo.Line}");
Console.WriteLine("====================================================");

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));

builder.Services.Configure<ConnectionFactory>(builder.Configuration.GetSection("RabbitMq"));
builder.Services.Configure<RabbitMqTopologyOptions>(builder.Configuration.GetSection("RabbitMqTopology"));

builder.Services.AddDbContext<NotificationsDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("NotificationsDb")));

builder.Services.AddHostedService<AuditConsumer>();
builder.Services.AddHostedService<SubscribersConsumer>();
builder.Services.AddHostedService<CriticalConsumer>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<NotificationsDbContext>();
    db.Database.EnsureCreated();
}

app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();