using Azure.Identity;
using Azure.Security.KeyVault.Certificates;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.SharePoint.Client;
using Sds.Dal.Models;
using Sds.Dal.Repos;
using SdsLogging.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using AuthenticationManager = PnP.Framework.AuthenticationManager;

namespace SdsLogging
{
    public class PdfAccessLogFunction
    {
        private LoggingRepo _loggingRepo;

        private PdfAccessLogFunctionSettings _settings;

        public PdfAccessLogFunction(LoggingRepo loggingRepo, PdfAccessLogFunctionSettings settings)
        {
            this._loggingRepo = loggingRepo;
            this._settings = settings;
        }

        [Function("WritePdfAccessLog")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
            FunctionContext executionContext)
        {
            HttpResponseData response;
            //Convert HTTP to Model
            var options = new JsonSerializerOptions
            {
                Converters =
                {
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
                }
            };

            var pdfAccessLog = await JsonSerializer.DeserializeAsync<PdfAccessLog>(req.Body, options);

            if (pdfAccessLog.SharePointDocumentId == null)
            {
                response = req.CreateResponse(HttpStatusCode.BadRequest);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
                return response;
            }

            //Save Model to Cosmos
            if (pdfAccessLog.Id == null || pdfAccessLog.Id == Guid.Empty)
            {
                pdfAccessLog.Id = Guid.NewGuid();
            }
            var result = await _loggingRepo.Create(pdfAccessLog, pdfAccessLog.User);
            if (result == false)
            {
                response = req.CreateResponse(HttpStatusCode.InternalServerError);
                response.WriteString("CosmosDB Failed");
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");
            }
            else
            {

                await IncrementItemCount(_settings.SpoUrl, pdfAccessLog.SharePointDocumentId);//Update sharepoint 

                response = req.CreateResponse(HttpStatusCode.OK);
                response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            }

            return response;
        }

        public async Task IncrementItemCount(string sharepointUrl, int sharepointDocId)
        {

            var client = new SecretClient(new Uri(_settings.KeyVaultUrl), new DefaultAzureCredential());//Make sure WEBSITE_LOAD_USER_PROFILE = 1 in azure configuration
            var secret = await client.GetSecretAsync(_settings.CertName);
            var cert = new X509Certificate2(Convert.FromBase64String(secret.Value.Value));//For cert generation see https://docs.microsoft.com/en-us/sharepoint/dev/solution-guidance/security-apponly-azuread

            var authManager = new AuthenticationManager(certificate: cert, clientId: _settings.ClientId, tenantId: _settings.TenantId);//The the access tolken and PnP framework to request authenticate to spo.
            
            using (ClientContext ctx = authManager.GetContext(sharepointUrl))
            {
                var docLib = ctx.Web.Lists.GetByTitle("Documents");
                CamlQuery oQuery = CamlQuery.CreateAllItemsQuery();
                var listItems = docLib.GetItems(oQuery);
                ctx.Load(listItems, eachItem => eachItem.Where(item => item.Id == sharepointDocId).Include(
                 item => item["Count"]));
                ctx.ExecuteQuery();

                var foundItem = listItems.FirstOrDefault();

                if (foundItem["Count"] == null)
                {
                    foundItem["Count"] = 0.0;
                }

                var currentCount = (double)foundItem["Count"];
                foundItem["Count"] = ++currentCount;

                foundItem.Update();

                ctx.ExecuteQuery();
            }
        }

        private string GetResourceUri(string siteUrl)
        {
            var uri = new Uri(siteUrl);
            return $"{uri.Scheme}://{uri.DnsSafeHost}";
        }

        /// <summary>
        /// create the correct sope of from the shorthand provided, this appends the sharepoint site url to the scope
        /// </summary>
        /// <param name="siteUrl"></param>
        /// <param name="scopes"></param>
        /// <returns></returns>
        private string[] GetSharePointResourceScope(string siteUrl, string[] scopes = null)
        {
            string resourceUri = GetResourceUri(siteUrl);
            return scopes == null
                ? new[] { $"{resourceUri}/.default" }
                : scopes.Select(scope => $"{resourceUri}/{scope}").ToArray();
        }

    }
}
