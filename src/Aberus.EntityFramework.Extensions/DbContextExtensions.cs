using System;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace Aberus.Data.Entity
{
    public static class DbContextExtensions
    {
        public static string CreateDatabaseScript(this DbContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            return ((IObjectContextAdapter)context).ObjectContext?.CreateDatabaseScript() 
                ?? "Database script cannot be created";
        }
    }
}