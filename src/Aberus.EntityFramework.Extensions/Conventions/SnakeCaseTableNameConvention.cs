using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public class SnakeCaseTableNameConvention : Convention
    {
        public SnakeCaseTableNameConvention()
        {
            Types().Configure(c => c.ToTable(GetTableName(c.ClrType)));
        }

        private string GetTableName(Type type)
        {
            if (type == null)
                throw new ArgumentNullException(nameof(type));

            if (type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() is TableAttribute tableAttribute)
                return tableAttribute.Name;

            return SnakeCaseConverter.ConvertToSnakeCase(type.Name);
        }
    }
}
