using System;
using System.Linq;
using System.Data.Entity.Core.Objects;
using Aberus.Data.Entity.Common;

namespace Aberus.Data.Entity
{
    public static class QueryableExtensions
    {
        public static string ToTraceString(this IQueryable source)
        {
            Check.NotNull(source, nameof(source));

            return source is ObjectQuery query
                ? query.ToTraceString()
                : "This 'IQueryable' does not support generation of trace strings.";
        }
    }
}