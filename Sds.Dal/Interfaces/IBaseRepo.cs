using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Sds.Dal.Interfaces
{
    public interface IBaseRepo
    {
        public interface IBaseRepo<T> where T : IHaveId
        {
            Task<bool> Create(T model, string partitionKey);
            Task<bool> Delete(T model, string partitionKey);
            Task<IEnumerable<T>> Get(Expression<Func<T, bool>> predicate);
            Task<bool> Update(T model, string partitionKey);
        }
    }
}
