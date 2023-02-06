using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using Aberus.Data.Entity.Common;

namespace Aberus.Data.Entity.ModelConfiguration.Conventions
{
    internal static class Helper
    {
        internal static string Uniquify(IEnumerable<string> inputStrings, string targetString)
        {
            Check.NotNull(inputStrings, nameof(inputStrings));
            Check.NotNull(targetString, nameof(targetString));

            string uniqueString = targetString;
            int i = 0;

            while (inputStrings.Any(n => string.Equals(n, uniqueString, StringComparison.Ordinal)))
                uniqueString = targetString + ++i;

            return uniqueString;
        }

        internal static EntityType GetRootType(EntityType entityType)
        {
            Check.NotNull(entityType, nameof(entityType));

            if (entityType.BaseType != null)
                return GetRootType((EntityType)entityType.BaseType);
            return entityType;
        }
    }
}
