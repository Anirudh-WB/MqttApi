using Microsoft.Extensions.Options;
using Mqtt.Models;
using Mqtt.Services;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System.Security.Cryptography.X509Certificates;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Bind the MQTT settings from appsettings.json
builder.Services.Configure<MqttSettings>(builder.Configuration.GetSection("MQTTSettings"));

builder.Services.AddHostedService<Services>();

builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<IOptions<MqttSettings>>().Value);

// Register MqttService with MqttSettings as a singleton
builder.Services.AddSingleton<MqttService>(sp =>
{
    var mqttSettings = sp.GetRequiredService<IOptions<MqttSettings>>().Value;

    // Build MQTT client options with or without TLS
    var optionsBuilder = new MqttClientOptionsBuilder()
        .WithClientId(mqttSettings.ClientId)
        .WithTcpServer(mqttSettings.BrokerAddress, mqttSettings.BrokerPort)
        .WithCleanSession(false);

    if (mqttSettings.UseTls)
    {
        var certificates = new List<X509Certificate2>();
        if (!string.IsNullOrEmpty(mqttSettings.CertificatePath))
        {
            var certificate = new X509Certificate2(
                mqttSettings.CertificatePath,
                mqttSettings.CertificatePassword);
            certificates.Add(certificate);
        }

        optionsBuilder.WithTls(tlsOptions =>
        {
            tlsOptions.UseTls = mqttSettings.UseTls;
            tlsOptions.Certificates = certificates;
            tlsOptions.AllowUntrustedCertificates = true; // Set to false in production
            tlsOptions.CertificateValidationHandler = _ => true; // Bypass validation for testing
        });
    }

    var managedOptions = new ManagedMqttClientOptionsBuilder()
        .WithClientOptions(optionsBuilder.Build())
        .WithAutoReconnectDelay(TimeSpan.FromSeconds(5))
        .Build();

    return new MqttService(mqttSettings, managedOptions);
});

builder.Services.AddControllers();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseAuthorization();

app.MapControllers();

app.Run();
