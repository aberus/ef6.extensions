using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Data.Objects;

namespace EnitityFramework.Conventions
{
    public class QueryableExtensions
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

    public class DbContextExtensions
    {
        public static string CreateDatabaseScript(this DbContext context)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(context));

            return source is IObjectContextAdapter context
                ? context.ObjectContext?.CreateDatabaseScript() ?? ""
                : "This 'IQueryable' does not support generation of trace strings.";
        }
    }
}