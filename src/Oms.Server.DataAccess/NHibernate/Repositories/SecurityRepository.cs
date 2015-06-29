using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NHibernate.Proxy;
using Oms.Server.Domain.Interfaces;
using Oms.Server.Domain.Interfaces.Repository;
using Oms.Server.Domain.Models.EventLogs;
using Oms.Server.Domain.Models.Securities;

namespace Oms.Server.DataAccess.NHibernate.Repositories
{
    internal class SecurityRepository : RepositoryBase<Security>, ISecurityRepository
    {
        public IList<Security> GetBySecurityType(SecurityType securityType)
        {
            return base.GetByColumn("Type", securityType);
        }

        public IList<Security> GetByCode(string securityCode, SecurityCodeType securityCodeType)
        {
            return GetWithCachedFilter(securityCode, securityCodeType);
        }

        public IList<Security> GetActiveList()
        {
            return base.GetByColumn("IsActive", true);
        }

        private static readonly Dictionary<SecurityCodeType, Func<Security, string>> SecurityCodeToColumn = new Dictionary<SecurityCodeType, Func<Security, string>>
        {
            {SecurityCodeType.Isin, s => s.IsinCode },
            {SecurityCodeType.Sophis, s => s.SophisCode },
            {SecurityCodeType.BloombergTicker, s => s.BloombergTicker }
        };

        // TODO merge the 2 Security Mapping

        private static readonly Dictionary<SecurityCodeType, string> SecurityCodeTypeToColumnName = new Dictionary<SecurityCodeType, string>
        {
            {SecurityCodeType.Isin, "IsinCode"},
            {SecurityCodeType.Sophis, "SophisCode"},
            {SecurityCodeType.BloombergTicker, "BloombergTicker"}
        };

        private Dictionary<SecurityCodeType, Dictionary<string, int[]>> _cacheByCode;

        private IList<Security> GetWithCachedFilter(string code, SecurityCodeType securityCodeType)
        {
            if (_cacheByCode == null)
            {
                return GetByColumn(SecurityCodeTypeToColumnName[securityCodeType], code);
            }
            int[] cachedIdentities;
            return _cacheByCode[securityCodeType].TryGetValue(code, out cachedIdentities) ? GetById(cachedIdentities).ToList() : new List<Security>();
        }

        public void CacheByCode()
        {
            var allSecutities = GetActiveList();
            _cacheByCode = SecurityCodeToColumn.ToDictionary(codeToColumn => codeToColumn.Key, codeToColumn => 
                allSecutities.Where(s => string.IsNullOrWhiteSpace(codeToColumn.Value(s)))
                .GroupBy(s => codeToColumn.Value(s))
                .ToDictionary(s => s.Key, s => s.Select(x => x.Id).ToArray()));
        }


    }


    
        public class RepositoryIndexer<T> where T : IIdentifiable
        {
            private readonly IRepository<T> _repository;
            private Dictionary<string, Dictionary<string, int[]>> _cacheByCode;

            private static readonly Dictionary<string, RepositoryIndexerItem<T>> _columnValueToProperty = new Dictionary<string, RepositoryIndexerItem<T>>();

            public RepositoryIndexer(IRepository<T> repository)
            {
                _repository = repository;
            }

            public void AddColumnIndex(string columnName, Func<T, string> propertyGetter, string propertyName)
            {
                _columnValueToProperty.Add(columnName, new RepositoryIndexerItem<T>(propertyGetter, propertyName));
            }

            public void DoCaching(IEnumerable<T> entitiesToCache)
            {
                //TODO Call directly repository
                _cacheByCode = _columnValueToProperty.ToDictionary(v => v.Key, v =>
                    entitiesToCache.Where(s => string.IsNullOrWhiteSpace(v.Value.PropertyGetter (s)))
                    .GroupBy(s => v.Value.PropertyGetter(s))
                    .ToDictionary(s => s.Key, s => s.Select(x => x.Id).ToArray()));
            }
        }

        public class RepositoryIndexerItem<T>
        {
            public Func<T, string> PropertyGetter { get; private set; }
            public string PropertyName { get; private set; }

            public RepositoryIndexerItem(Func<T, string> propertyGetter, string propertyName)
            {
                Contract.Requires(propertyGetter != null, "propertyGetter != null");

                PropertyGetter = propertyGetter;
                PropertyName = propertyName;
            }
        }
}
