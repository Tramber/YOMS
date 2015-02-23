using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Criterion;
using Oms.Server.Domain;
using Oms.Server.Domain.Interfaces;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class RepositoryBase<T> : IRepository<T> where T : IIdentifiable
    {
        public void Add(T entity)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Save(entity);
                    transaction.Commit();
                }
            }
        }
        public void Add(IEnumerable<T> entities)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var entity in entities)
                    {
                        session.Save(entity);
                    }
                    transaction.Commit();
                }
            }
        }

        public void Remove(T entity)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Delete(entity);
                    transaction.Commit();
                }
            } 
        }

        public void Remove(IEnumerable<T> entities)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var entity in entities)
                    {
                        session.Delete(entity);
                    }
                    transaction.Commit();
                }
            } 
        }

        public void Update(T entity)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    session.Update(entity);
                    transaction.Commit();
                }
            }
        }

        public void Update(IEnumerable<T> entities)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                using (var transaction = session.BeginTransaction())
                {
                    foreach (var entity in entities)
                    {
                        session.Update(entity);
                    }
                    transaction.Commit();
                }
            }
        }

        public IList<T> GetByColumn(string propertyName, object columnValue)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                return session.CreateCriteria(typeof(T))
                    .Add(Restrictions.Eq(propertyName, columnValue))
                    .List<T>();
            }
        }

        public T GetById(int id)
        {
            using (var session = NHibernateHelper.Instance.OpenSession())
            {
                return session.Get<T>(id);
            }
        }
    }

}
