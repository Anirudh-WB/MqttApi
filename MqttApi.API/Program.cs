using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet;
using MqttApi.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure MqttClientOptions and register it as IOptions<MqttClientOptions>
builder.Services.Configure<MqttClientOptions>(options =>
{
    options.ClientId = "MqttRestApiClient";
    options.ChannelOptions = new MqttClientTcpOptions
    {
        Server = "localhost",
        Port = 1883 // Default MQTT port
    };
});

builder.Services.AddHostedService<MqttApi.Services.Services>();

// Register IMqttClient
builder.Services.AddSingleton<IMqttClient>(sp =>
{
    var mqttFactory = new MqttFactory();
    var mqttClient = mqttFactory.CreateMqttClient();

    return mqttClient;
});


var app = builder.Build();

// Configure the HTTP request pipeline.

app.UseAuthorization();

app.UseHttpsRedirection();

app.UseRouting();

app.MapControllers();

app.Run();
