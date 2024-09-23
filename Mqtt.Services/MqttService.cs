using Mqtt.DTO;
using Mqtt.Models;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Packets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Mqtt.Services
{
    public class MqttService
    {
        private readonly IManagedMqttClient _mqttClient;
        private readonly MqttSettings _mqttSettings;
        private ManagedMqttClientOptions _managedOptions;

        public MqttService(MqttSettings mqttSettings, ManagedMqttClientOptions managedOptions)
        {
            _mqttSettings = mqttSettings;
            _managedOptions = managedOptions;

            // Create the managed MQTT client
            _mqttClient = new MqttFactory().CreateManagedMqttClient();
        }

        public async Task ConnectAsync(MqttClientOptions options)
        {
            try
            {
                if (!_mqttClient.IsConnected)
                {
                    await _mqttClient.StartAsync(new ManagedMqttClientOptionsBuilder()
                        .WithClientOptions(options)
                        .Build());
                    Console.WriteLine("Connected to the MQTT broker.");
                }
                else
                {
                    Console.WriteLine("Already connected to the MQTT broker.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MQTT broker: {ex.Message}");
            }
        }


        public async Task DisconnectAsync()
        {
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.StopAsync();
                Console.WriteLine("Disconnected from the MQTT broker.");
            }
            else
            {
                Console.WriteLine("Already disconnected from the MQTT broker.");
            }
        }

        public async Task PublishAsync(PublishDto publishDto)
        {
            var payload = JsonSerializer.Serialize(publishDto);

            var message = new MqttApplicationMessageBuilder()
                .WithTopic(_mqttSettings.Topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.ExactlyOnce)
                .WithRetainFlag(false)
                .Build();

            await _mqttClient.EnqueueAsync(message);
        }

        public async Task SubscribeAsync()
        {
            if (!_mqttClient.IsConnected)
            {
                await ConnectAsync(_managedOptions.ClientOptions); // Ensure client is connected
            }

            var topicFilter = new MqttTopicFilterBuilder().WithTopic(_mqttSettings.Topic).Build();
            await _mqttClient.SubscribeAsync(new List<MqttTopicFilter> { topicFilter });
            Console.WriteLine($"Subscribed to topic: {_mqttSettings.Topic}");
        }

        public async Task UnsubscribeAsync(string topic)
        {
            await _mqttClient.UnsubscribeAsync(new List<string> { topic });
            Console.WriteLine($"Unsubscribed from topic: {topic}");
        }

        public IManagedMqttClient GetClient() => _mqttClient; // Expose the client for event subscriptions
    }
}
