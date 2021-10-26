using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sds.Dal.Interfaces
{
    public interface IHaveId
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        Guid Id { get; set; }
    }
}
