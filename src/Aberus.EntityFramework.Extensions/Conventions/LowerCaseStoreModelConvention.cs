using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Aberus.Data.Entity.Common;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public class LowerCaseStoreModelConvention : IStoreModelConvention<EntityType>, IStoreModelConvention<EdmProperty>, IStoreModelConvention<AssociationType>
    {
        public void Apply(EntityType item, DbModel model)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(model, nameof(model));

            var entitySet = model.StoreModel.Container.EntitySets.SingleOrDefault(es => es.ElementType == Helper.GetRootType(item));
            object tableName = item.MetadataProperties.FirstOrDefault(x => x.Name == "TableName")?.Value;

            if (tableName == null)
            {
                entitySet.Table
                    = Helper.Uniquify(model.StoreModel.Container.EntitySets
                        .Where(es => es.Schema == entitySet.Schema)
                        .Except(new[] { entitySet })
                        .Select(n => n.Table),
                        entitySet.Table.ToLowerInvariant());

            }
        }

        public void Apply(EdmProperty item, DbModel model)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(model, nameof(model));

            string preferredName = (string)item.MetadataProperties.FirstOrDefault(x => x.Name == "PreferredName")?.Value;
            if (preferredName == item.Name)
                item.Name = item.Name.ToLowerInvariant();
        }

        public void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(model, nameof(model));

            if (item.IsForeignKey)
            {
                foreach (var property in item.Constraint.FromProperties)
                {
                    property.Name = property.Name.ToLowerInvariant();
                }

                foreach (var property in item.Constraint.ToProperties)
                {
                    property.Name = property.Name.ToLowerInvariant();
                }
            }
        }
    }
}
