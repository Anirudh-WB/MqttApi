using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Mqtt.Services;
using MQTTnet.Client;
using System.Security.Cryptography.X509Certificates;

namespace Mqtt.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MqttController : ControllerBase
    {
        private readonly MqttService _mqttService;

        public MqttController(MqttService mqttService)
        {
            _mqttService = mqttService;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] string message)
        {
            await _mqttService.PublishAsync(message);
            return Ok("Message published");
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe([FromBody] string topic)
        {
            await _mqttService.SubscribeAsync(topic);
            return Ok($"Subscribed to {topic}");
        }

        [HttpPost("unsubscribe")]
        public async Task<IActionResult> Unsubscribe([FromBody] string topic)
        {
            await _mqttService.UnsubscribeAsync(topic);
            return Ok($"Unsubscribed from {topic}");
        }

        [HttpPost("update-settings")]
        public async Task<IActionResult> UpdateMqttSettings([FromBody] MqttSettingsUpdateDto dto)
        {
            if (dto == null || string.IsNullOrEmpty(dto.BrokerAddress) || dto.BrokerPort <= 0)
            {
                return BadRequest("Invalid input.");
            }

            // Load client certificate if necessary
            X509Certificate2 clientCertificate = null;
            if (!string.IsNullOrEmpty(dto.CertificatePath))
            {
                clientCertificate = new X509Certificate2(dto.CertificatePath, dto.CertificatePassword);
            }

            // Update MQTT Client Options
            var optionsBuilder = new MqttClientOptionsBuilder()
                .WithClientId(_mqttOptions.Value.ClientId)
                .WithTcpServer(dto.BrokerAddress, dto.BrokerPort)
                .WithCleanSession(dto.CleanSession);

            if (dto.UseTls)
            {
                optionsBuilder.WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    Certificates = clientCertificate != null ? new List<X509Certificate> { clientCertificate } : null,
                    AllowUntrustedCertificates = dto.AllowUntrustedCertificates,
                    IgnoreCertificateChainErrors = dto.IgnoreCertificateChainErrors,
                    IgnoreCertificateRevocationErrors = dto.IgnoreCertificateRevocationErrors
                });
            }

            var newOptions = optionsBuilder.Build();

            // Disconnect the current client if connected
            if (_mqttClient.IsConnected)
            {
                await _mqttClient.DisconnectAsync();
            }

            // Reconnect with the new options
            await _mqttClient.ConnectAsync(newOptions);

            // Update the options in the service if needed (not directly updating IOptions here)
            _mqttOptions.Value.BrokerAddress = dto.BrokerAddress;
            _mqttOptions.Value.BrokerPort = dto.BrokerPort;
            _mqttOptions.Value.UseTls = dto.UseTls;
            // Update any other properties from the DTO as necessary

            return Ok("MQTT client settings updated and reconnected.");
        }

    }
}
