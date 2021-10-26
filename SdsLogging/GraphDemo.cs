using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Sds.Dal.Models;
using SdsLogging.Models;

namespace SdsLogging
{
    public class GraphDemo
    {
        private GraphDemoSettings _settings;

        public GraphDemo(GraphDemoSettings settings)
        {
            this._settings = settings;
        }

        [Function("GraphDemo")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Function, "post")] HttpRequestData req,
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


            await IncrementItemCount("", pdfAccessLog.SharePointDocumentId);//Update sharepoint 

            response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/plain; charset=utf-8");

            return response;
        }

        public async Task IncrementItemCount(string sharepointUrl, int sharepointDocId)
        {
            // The client credentials flow requires that you request the
            // /.default scope, and preconfigure your permissions on the
            // app registration in Azure. An administrator must grant consent
            // to those permissions beforehand.
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            var options = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(
                _settings.TenantId, _settings.GraphDemoClientId, _settings.GraphDemoClientSecret, options);

            GraphServiceClient graphClient = new GraphServiceClient(clientSecretCredential, scopes);

            var queryOptions = new List<QueryOption>()
            {
                new QueryOption("expand", "fields")
            };

            //to get sharepoint site id, call this graph query https://graph.microsoft.com/v1.0/sites/[Sharepoint domain]:/sites/[SITE]?$select=id

            var item = await graphClient.Sites[_settings.GraphDemoSiteId].Lists[_settings.GraphDemoListId].Items[sharepointDocId.ToString()]
                .Request()
                .GetAsync();

            var data = item.Fields.AdditionalData;

            var count = double.Parse(data["Count"].ToString());

            var fieldValueSet = new FieldValueSet
            {
                AdditionalData = new Dictionary<string, object>()
                {
                    {"Count", count + 1}
                }
            };

            await graphClient.Sites[_settings.GraphDemoSiteId].Lists[_settings.GraphDemoListId].Items[sharepointDocId.ToString()].Fields
                  .Request()
                  .UpdateAsync(fieldValueSet);
        }
    }
}
