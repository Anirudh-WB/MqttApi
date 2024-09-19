using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace MqttApi.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttController : ControllerBase
    {
        private readonly IMqttClient _mqttClient;
        private readonly IOptions<MqttClientOptions> _mqttOptions;

        public MqttController(IMqttClient mqttClient, IOptions<MqttClientOptions> mqttOptions)
        {
            _mqttClient = mqttClient;
            _mqttOptions = mqttOptions;
        }

        [HttpPost("update-options")]
        public async Task<IActionResult> UpdateOptions([FromBody] MqttOptionsUpdateDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.Server) || dto.Port <= 0)
            {
                return BadRequest("Invalid input.");
            }

            var clientCertificate = new X509Certificate2("D:\\Projects\\MqttApi\\MqttApi.API\\Certificates\\MyOpcUaServer.pfx", string.Empty);

            // Update MQTT Client Options
            var options = new MqttClientOptionsBuilder()
                .WithClientId(_mqttOptions.Value.ClientId)
                .WithTcpServer(dto.Server, dto.Port)
                .WithCleanSession(false)
                .WithWillContentType("application/json")
                .WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    Certificates = new List<X509Certificate> { clientCertificate },
                    //AllowUntrustedCertificates = true, // Set to false in production
                    //IgnoreCertificateChainErrors = true, // Set to false in production
                    //IgnoreCertificateRevocationErrors = true // Set to false in production
                })
                .Build();


            // Disconnect the current client if connected
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }

            // Update the client options in the service (not directly updating IOptions here as it's typically singleton)
            _mqttOptions.Value.ChannelOptions = options.ChannelOptions;

            // Reconnect with the new options
            await _mqttClient.ConnectAsync(options);

            return Ok("MQTT client options updated and reconnected.");
        }

        [HttpPost("publish")]
        public async Task<IActionResult> PublishMessage(string topic, [FromBody] object payload)
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttOptions.Value);
            }

            var jsonPayload = JsonSerializer.Serialize(payload);

            var mqttMessage = new MQTTnet.MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(jsonPayload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            await _mqttClient.PublishAsync(mqttMessage);

            return Ok(new { Message = "Published successfully", Topic = topic, Payload = jsonPayload });
        }

        /*[HttpPost("subscribe")]
        public async Task<IActionResult> SubscribeToTopic(string topic)
        {
            if (!_mqttClient.IsConnected)
            {
                await _mqttClient.ConnectAsync(_mqttOptions.Value);
            }

            await _mqttClient.SubscribeAsync(new MQTTnet.MqttTopicFilterBuilder()
                .WithTopic(topic)
                .Build());

            var msg = "";

            _mqttClient.ApplicationMessageReceivedAsync += e =>
            {
                var receivedMessage = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                // Handle the received message (log it, process it, etc.)

                Console.WriteLine($"Received message: {receivedMessage} on topic: {e.ApplicationMessage.Topic}");

                msg = receivedMessage;

                return Task.CompletedTask;
            };

            return Ok(new { Message = $"Subscribed to topic: {topic}", Payload = $"Received message: {msg}" });
        }*/
    }

    public class MqttOptionsUpdateDto
    {
        public string Server { get; set; }
        public int Port { get; set; }
    }

}
