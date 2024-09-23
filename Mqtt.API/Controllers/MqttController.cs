using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mqtt.DTO;
using Mqtt.Models;
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
        private readonly MqttSettings _mqttSettings;

        public MqttController(MqttService mqttService, IOptions<MqttSettings> mqttSettings)
        {
            _mqttService = mqttService;
            _mqttSettings = mqttSettings.Value;
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish([FromBody] string message)
        {
            await _mqttService.PublishAsync(message);
            return Ok("Message published");
        }

        [HttpPost("subscribe")]
        public async Task<IActionResult> Subscribe()
        {
            await _mqttService.SubscribeAsync();
            return Ok($"Subscribed Successfully");
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
                .WithClientId(_mqttSettings.ClientId)
                .WithTcpServer(dto.BrokerAddress, dto.BrokerPort);

            if (dto.UseTls == true)
            {
                optionsBuilder.WithTls(new MqttClientOptionsBuilderTlsParameters
                {
                    UseTls = true,
                    Certificates = clientCertificate != null ? new List<X509Certificate> { clientCertificate } : null,
                });
            }

            var newOptions = optionsBuilder.Build();

            // Disconnect the current client if connected
            if (_mqttService.GetClient().IsConnected)
            {
                await _mqttService.DisconnectAsync();
            }

            // Reconnect with the new options
            await _mqttService.ConnectAsync(newOptions);

            // Update the options in the service if needed
            _mqttSettings.BrokerAddress = dto.BrokerAddress;
            _mqttSettings.BrokerPort = dto.BrokerPort.Value;
            //_mqttSettings.UseTls = dto.UseTls.Value;

            return Ok("MQTT client settings updated and reconnected.");
        }
    }
}
