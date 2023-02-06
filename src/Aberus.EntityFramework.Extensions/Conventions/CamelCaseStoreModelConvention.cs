using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Aberus.Data.Entity.Common;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    public class CamelCaseStoreModelConvention : IStoreModelConvention<EntityType>, IStoreModelConvention<EdmProperty>, IStoreModelConvention<AssociationType>
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
                        .Select(n => n.Table)
                        , CamelCaseConverter.Convert(entitySet.Table));
            }
        }

        public void Apply(EdmProperty item, DbModel model)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(model, nameof(model));

            string preferredName = (string)item.MetadataProperties.FirstOrDefault(x => x.Name == "PreferredName")?.Value;
            if (preferredName == item.Name)
                item.Name = CamelCaseConverter.Convert(item.Name);
        }

        public void Apply(AssociationType item, DbModel model)
        {
            Check.NotNull(item, nameof(item));
            Check.NotNull(model, nameof(model));

            if (!item.IsForeignKey)
            {
                return;
            }

            var associationsSetMappings = model.ConceptualToStoreMapping.AssociationSetMappings;

            foreach (var associationSetMapping in associationsSetMappings)
            {
                var associationSetEnds = associationSetMapping.AssociationSet.AssociationSetEnds;
                associationSetMapping.StoreEntitySet.Table = string.Format("{0}_{1}",
                    CamelCaseConverter.Convert(associationSetEnds[0].EntitySet.ElementType.Name),
                    CamelCaseConverter.Convert(associationSetEnds[1].EntitySet.ElementType.Name));
            }

        }
    }
}
