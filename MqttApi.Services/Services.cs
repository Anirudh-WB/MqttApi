using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MqttApi.Services
{
    public class Services : BackgroundService
    {
        private readonly IMqttClient _mqttClient;
        private readonly IOptions<MqttClientOptions> _mqttOptions;

        public Services(IMqttClient mqttClient, IOptions<MqttClientOptions> mqttOptions)
        {
            _mqttClient = mqttClient;
            _mqttOptions = mqttOptions;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttOptions.Value);
            }

            await _mqttClient.SubscribeAsync(new MQTTnet.MqttTopicFilterBuilder()
                .WithTopic("test/topic")
                .Build());

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var message = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                Console.WriteLine($"Message received: {message}");
                return Task.CompletedTask;
            };

            while (!stoppingToken.IsCancellationRequested)
            {
                // Keep the connection alive or handle any other periodic logic
                await Task.Delay(1000);
            }
        }
    }
}
