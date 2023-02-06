using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Reflection;
using Aberus.Data.Entity.Common;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public class CamelCaseColumnNameConvention : Convention
    {
        public CamelCaseColumnNameConvention()
        {
            Properties().Configure(config => config.HasColumnName(GetColumnName(config.ClrPropertyInfo)));
        }

        private string GetColumnName(PropertyInfo propertyInfo)
        {
            Check.NotNull(propertyInfo, nameof(propertyInfo));

            var columnAttribute = propertyInfo.GetCustomAttributes<ColumnAttribute>().FirstOrDefault();
            if (columnAttribute != null)
                return columnAttribute.Name;

            return CamelCaseConverter.Convert(propertyInfo.Name);
        }
    }
}
