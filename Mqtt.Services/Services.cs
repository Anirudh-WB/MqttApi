using Microsoft.Extensions.Hosting;
using Mqtt.Models;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Mqtt.Services
{
    public class Services : BackgroundService
    {
        private readonly MqttService _mqttService;

        public Services(MqttService mqttService)
        {
            _mqttService = mqttService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // Subscribe to a topic
            await _mqttService.SubscribeAsync();

            // Handle incoming messages
            _mqttService.GetClient().ApplicationMessageReceivedAsync += e =>
            {
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                //var receivedMessage = JsonSerializer.Deserialize<Dictionary<string, object>>(payload);
                //Console.WriteLine($"Message received: {JsonSerializer.Serialize(receivedMessage)}");
                Console.WriteLine($"Message received: {payload}");
                return Task.CompletedTask;
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                // Keep the connection alive or handle any other periodic logic
                await Task.Delay(1000);
            }

            await _mqttService.DisconnectAsync();
        }
    }
}
