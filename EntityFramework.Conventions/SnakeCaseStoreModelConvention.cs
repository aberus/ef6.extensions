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
    public class SnakeCaseStoreModelConvention : IStoreModelConvention<EntityType>, IStoreModelConvention<EdmProperty>, IStoreModelConvention<AssociationType>
    {
        public void Apply(EntityType item, DbModel model)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var entitySet = model.StoreModel.Container.EntitySets.SingleOrDefault(es => es.ElementType == GetRootType(item));

            var tableName = item.MetadataProperties.FirstOrDefault(x => x.Name == "TableName");

            if (tableName == null)
            {
                entitySet.Table
                    = Uniquify(model.StoreModel.Container.EntitySets
                        .Where(es => es.Schema == entitySet.Schema)
                        .Except(new[] { entitySet })
                        .Select(n => n.Table)
                        , SnakeCaseConverter.ConvertToSnakeCase(entitySet.Table));

            }
        }

        public void Apply(EdmProperty item, DbModel model)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var preferredName = (string)item.MetadataProperties.FirstOrDefault(x => x.Name == "PreferredName")?.Value;
            if (preferredName == item.Name)
                item.Name = SnakeCaseConverter.ConvertToSnakeCase(item.Name);
        }

        public string Uniquify(IEnumerable<string> inputStrings, string targetString)
        {
            string uniqueString = targetString;
            int i = 0;

            while (inputStrings.Any(n => string.Equals(n, uniqueString, StringComparison.Ordinal)))
                uniqueString = targetString + ++i;

            return uniqueString;
        }

        EntityType GetRootType(EntityType entityType)
        {
            if (entityType.BaseType != null)
                return GetRootType((EntityType)entityType.BaseType);
            return entityType;
        }

        public void Apply(AssociationType item, DbModel model)
        {
            var associationsSetMappings = model.ConceptualToStoreMapping.AssociationSetMappings;

            foreach (var associationSetMapping in associationsSetMappings)
            {
                var associationSetEnds = associationSetMapping.AssociationSet.AssociationSetEnds;
                associationSetMapping.StoreEntitySet.Table = String.Format("{0}_{1}",
                    SnakeCaseConverter.ConvertToSnakeCase(associationSetEnds[0].EntitySet.ElementType.Name),
                    SnakeCaseConverter.ConvertToSnakeCase(associationSetEnds[1].EntitySet.ElementType.Name));
            }

        }
    }
}
