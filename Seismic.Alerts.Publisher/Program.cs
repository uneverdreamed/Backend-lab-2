using Microsoft.OpenApi;
using RabbitMQ.Client;
using Seismic.Alerts.Publisher.Common;
using Seismic.Alerts.Publisher.Options;
using Seismic.Alerts.Publisher.Services;
using Serilog;

Console.WriteLine("====================================================");
Console.WriteLine(" Seismic Alerts Publisher");
Console.WriteLine($" {MeInfo.Line}");
Console.WriteLine("====================================================");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddSerilog(config => config.ReadFrom.Configuration(builder.Configuration));
builder.Services.Configure<ConnectionFactory>(builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<RabbitMqTopologyOptions>(builder.Configuration.GetSection("RabbitMqTopology"));
builder.Services.Configure<AlertClassificationOptions>(builder.Configuration.GetSection("AlertClassification"));
builder.Services.AddSingleton<IAlertPublisher, AlertPublisher>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapControllers();

app.Run();