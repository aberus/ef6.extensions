﻿using System;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq;
using Aberus.Data.Entity.Common;

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
            Check.NotNull(type, nameof(type));

            if (type.GetCustomAttributes(typeof(TableAttribute), false).FirstOrDefault() is TableAttribute tableAttribute)
                return tableAttribute.Name;

            return SnakeCaseConverter.Convert(type.Name);
        }
    }
}
