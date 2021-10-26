using Microsoft.Azure.Cosmos;
using Sds.Dal.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using static Sds.Dal.Interfaces.IBaseRepo;

namespace Sds.Dal.Repos
{
    public class BaseRepo<T> : IBaseRepo<T> where T : IHaveId
    {
        protected Container _container;

        public BaseRepo(CosmosClient dbClient, string databaseName, string containerName)
        {
            this._container = dbClient.GetContainer(databaseName, containerName);
        }

        public async Task<bool> Create(T model, string partitionKey)
        {
            try
            {
                if(model.Id == null || model.Id == Guid.Empty)
                {
                    model.Id = Guid.NewGuid();
                }

                var response = await this._container.CreateItemAsync<T>(model, new PartitionKey(partitionKey));
                return (response.StatusCode == HttpStatusCode.Created);
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ce)
            {
                if (ce.StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Update(T model, string partitionKey)
        {
            try
            {
                var foundItem = this.Get(aa => aa.Id == model.Id).Result.FirstOrDefault();

                if (foundItem != null)
                {
                    await this.Delete(foundItem, partitionKey);

                    var response = await this.Create(model, partitionKey);
                }

                else
                {
                    throw new Exception("Item does not exist.");
                }
                return true;
            }
            catch (Microsoft.Azure.Cosmos.CosmosException ce)
            {
                if (ce.StatusCode == HttpStatusCode.Conflict)
                {
                    return false;
                }
                else
                {
                    throw;
                }
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> Delete(T model, string? partitionKey)
        {
            try
            {
                if (string.IsNullOrEmpty(partitionKey))
                {
                    var response = await this._container.DeleteItemAsync<T>(model.Id.ToString(), PartitionKey.Null);
                }
                else
                {
                    var response = await this._container.DeleteItemAsync<T>(model.Id.ToString(), new PartitionKey(partitionKey));
                }
                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<IEnumerable<T>> Get(Expression<Func<T, bool>> predicate)
        {
            var items = this._container.GetItemLinqQueryable<T>(allowSynchronousQueryExecution: true).Where(predicate);

            return items.ToList();
        }
    }
}
