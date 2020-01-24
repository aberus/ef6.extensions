using System;
using System.Linq;
using System.Data.Entity.Core.Objects;

namespace Aberus.Data.Entity
{
    public static class QueryableExtensions
    {
        public static string ToTraceString(this IQueryable source)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));

            return source is ObjectQuery query
                ? query.ToTraceString()
                : "This 'IQueryable' does not support generation of trace strings.";
        }
    }
}