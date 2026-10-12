using System;
using System.Collections.Generic;
using System.Linq;
using FluentNHibernate;
using FluentNHibernate.Diagnostics;

namespace Oms.Server.DataAccess.NHibernate
{
    /// <summary>
    /// Types for the automapper: those of an assembly plus extra ones. Scanning an assembly only finds open generic
    /// definitions, so the closed event log types (<c>OrderParameterEventLog&lt;IOrderEditableData&gt;</c>, ...) must be added here.
    /// </summary>
    public class ExplicitTypeSource : ITypeSource
    {
        private readonly IEnumerable<Type> _types;

        public ExplicitTypeSource(Type assemblyMarker, IEnumerable<Type> extraTypes)
        {
            _types = [.. assemblyMarker.Assembly.GetTypes(), .. extraTypes];
        }

        public IEnumerable<Type> GetTypes()
        {
            return _types;
        }

        public void LogSource(IDiagnosticLogger logger)
        {
        }

        public string GetIdentifier()
        {
            return "Oms domain types";
        }
    }
}
