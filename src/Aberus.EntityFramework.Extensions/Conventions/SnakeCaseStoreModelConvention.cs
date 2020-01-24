using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
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

            object tableName = item.MetadataProperties.FirstOrDefault(x => x.Name == "TableName")?.Value;

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

            string preferredName = (string)item.MetadataProperties.FirstOrDefault(x => x.Name == "PreferredName")?.Value;
            if (preferredName == item.Name)
                item.Name = SnakeCaseConverter.ConvertToSnakeCase(item.Name);
        }

        private string Uniquify(IEnumerable<string> inputStrings, string targetString)
        {
            string uniqueString = targetString;
            int i = 0;

            while (inputStrings.Any(n => string.Equals(n, uniqueString, StringComparison.Ordinal)))
                uniqueString = targetString + ++i;

            return uniqueString;
        }

        private EntityType GetRootType(EntityType entityType)
        {
            if (entityType.BaseType != null)
                return GetRootType((EntityType)entityType.BaseType);
            return entityType;
        }

        public void Apply(AssociationType item, DbModel model)
        {
            if (item == null)
                throw new ArgumentNullException(nameof(item));

            if (model == null)
                throw new ArgumentNullException(nameof(model));

            var associationsSetMappings = model.ConceptualToStoreMapping.AssociationSetMappings;

            foreach (var associationSetMapping in associationsSetMappings)
            {
                var associationSetEnds = associationSetMapping.AssociationSet.AssociationSetEnds;
                associationSetMapping.StoreEntitySet.Table = string.Format("{0}_{1}",
                    SnakeCaseConverter.ConvertToSnakeCase(associationSetEnds[0].EntitySet.ElementType.Name),
                    SnakeCaseConverter.ConvertToSnakeCase(associationSetEnds[1].EntitySet.ElementType.Name));
            }

        }
    }
}
