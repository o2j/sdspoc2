using Microsoft.Azure.Cosmos;
using Sds.Dal.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Dal.Repos
{
    public class LoggingRepo : BaseRepo<PdfAccessLog>
    {
        private const string DataBaseName = "FRB";
        private const string ContainerName = "Logging";
        public LoggingRepo(CosmosClient dbClient) : base(dbClient, DataBaseName, ContainerName)
        {

        }
    }
}
