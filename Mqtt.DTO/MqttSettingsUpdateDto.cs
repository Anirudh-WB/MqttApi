using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mqtt.DTO
{
    public class MqttSettingsUpdateDto
    {
        public string? ClientId { get; set; }
        public string? BrokerAddress { get; set; }
        public int? BrokerPort { get; set; }
        public string? Topic { get; set; }
        public bool? UseTls { get; set; }  // Flag to indicate if TLS should be used
        public string? CertificatePath { get; set; }
        public string? CertificatePassword { get; set; }
    }
}
