using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using Aberus.Data.Entity.Common;

namespace Aberus.Data.Entity
{
    public static class DbContextExtensions
    {
        public static string CreateDatabaseScript(this DbContext context)
        {
            Check.NotNull(context, nameof(context));

            return ((IObjectContextAdapter)context).ObjectContext?.CreateDatabaseScript()
                ?? "Database script cannot be created";
        }
    }
}