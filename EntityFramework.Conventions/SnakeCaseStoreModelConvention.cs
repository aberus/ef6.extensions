using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EnitityFramework.Conventions
{
    public class SnakeCaseStoreModelConvention : IStoreModelConvention<EntitySet>, IStoreModelConvention<EdmProperty>
    {
        public void Apply(EntitySet item, DbModel model)
        {
            item.Table = SnakeCaseConverter.ConvertToSnakeCase(item.Table);
        }

        public void Apply(EdmProperty item, DbModel model)
        {
            item.Name = SnakeCaseConverter.ConvertToSnakeCase(item.Name);
        }

    }

}
