using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SdsLogging.Models
{
    public class PdfAccessLogFunctionSettings
    {
        public string SpoUrl { get; set; }
        public string KeyVaultUrl { get; set; }//Url of kev vault
        public string CertName { get; set; }//name of  cert in key vault
        public string ClientId { get; set; }//Client if of app registration
        public string TenantId { get; set; }//TenantId of app registration
    }
}
