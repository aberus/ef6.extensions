using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public class SnakeCaseColumnNameConvention : Convention
    {
        public SnakeCaseColumnNameConvention()
        {
            Properties().Configure(config => config.HasColumnName(GetColumnName(config.ClrPropertyInfo)));
        }

        private string GetColumnName(PropertyInfo propertyInfo)
        {
            var columnAttribute = propertyInfo.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
            if (columnAttribute != null)
                return columnAttribute.Name;

            return SnakeCaseConverter.ConvertToSnakeCase(propertyInfo.Name);
        }
    }
}
