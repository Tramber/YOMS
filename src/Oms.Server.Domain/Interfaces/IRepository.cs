using System.Collections.Generic;

namespace Oms.Server.Domain.Interfaces
{
    public interface IRepository<in T>
    {
        void Add(T entity);
        void Add(IEnumerable<T> entities);
        void Remove(T entity);
        void Remove(IEnumerable<T> entities);
        void Update(T entity);
        void Update(IEnumerable<T> entities);
    }
}
