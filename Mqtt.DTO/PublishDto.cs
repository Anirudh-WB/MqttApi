using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mqtt.DTO
{
    public class PublishDto
    {
        public string Tag { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
