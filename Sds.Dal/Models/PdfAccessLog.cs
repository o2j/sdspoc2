using Newtonsoft.Json;
using Sds.Dal.Interfaces;
using Sds.Dal.Models.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sds.Dal.Models
{
    public class PdfAccessLog : IHaveId
    {
        [JsonPropertyName("id")]
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonPropertyName("user")]
        [JsonProperty("user")]
        public string User { get; set; }

        [JsonPropertyName("sharePointDocumentId")]
        [JsonProperty("sharePointDocumentId")]
        public int SharePointDocumentId { get; set; }

        [JsonPropertyName("pdf")]
        [JsonProperty("pdf")]
        public string Pdf { get; set; }

        [JsonPropertyName("accessType")]
        [JsonProperty("accessType")]
        public AccessType AccessType { get; set; }

        [JsonPropertyName("pdfEventTime")]
        [JsonProperty("pdfEventTime")]
        public DateTime PdfEventTime { get; set; }
    }
}
