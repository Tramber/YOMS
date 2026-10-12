using System.Collections.Generic;

namespace Oms.Server.Domain.Interfaces.Repository
{
    /// <summary>Persistence of one entity type. Methods open their own session and transaction.</summary>
    public interface IRepository<T>
    {
        void Add(T entity);
        void Add(IEnumerable<T> entities);
        void Remove(T entity);
        void Remove(IEnumerable<T> entities);
        void Update(T entity);
        void Update(IEnumerable<T> entities);
        T GetById(int id);
        /// <summary>Every entity of the type.</summary>
        IList<T> GetAll();

        IEnumerable<T> GetById(IEnumerable<int> idList);
    }
}
