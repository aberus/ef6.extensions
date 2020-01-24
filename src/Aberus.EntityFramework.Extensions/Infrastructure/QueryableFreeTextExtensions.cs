using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Aberus.Data.Entity.Infrastructure
{
    /// <summary>
    /// IQueryableFreeTextExtensions, this extension provides composable
    /// access to FreeText (or Contains) searches of Entity Framework
    /// "Code First" objects. Assuming they are stored in in SQL Server tables
    /// that have FullText catalogs
    /// </summary>
    public static class QueryableFreeTextExtensions
    {
        #region Entity Framework Utilities
        //we cache table names in a a dictionary because
        //they take a lot of work to figure out
        static readonly object _LockProxy = new object();
        static readonly Dictionary<Type, string> _TableNameCache = new Dictionary<Type, string>();

        /// <summary>
        /// Generate an Entity set for TEntity so that we can inspect the metadata
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private static EntitySet GetMetaDataEntitySet<TEntity>(DbContext dbContext)
        {
            var entityType = typeof(TEntity);
            string entityName = entityType.Name;
            var metaDataWS = ((IObjectContextAdapter)dbContext).ObjectContext.MetadataWorkspace;

            //IEnumerable<EntitySet> entitySets;
            var entitySets = metaDataWS.GetItemCollection(DataSpace.SSpace)
                             .GetItems<EntityContainer>()
                             .Single()
                             .BaseEntitySets
                             .OfType<EntitySet>()
                             .Where(entitySet => !entitySet.MetadataProperties.Contains("Type")
                                                || entitySet.MetadataProperties["Type"].ToString() == "Tables");

            var provisionedTables = entitySets.ToList();
            var returnValue = provisionedTables.FirstOrDefault(t => t.Name == entityName);
            //When an Entity inherits a base class, the corresponding
            //table is sometimes named for the base class
            while (null == returnValue && null != entityType)
            {
                entityType = entityType.BaseType;
                entityName = entityType.Name;
                returnValue = provisionedTables.FirstOrDefault(t => t.Name == entityName);
            }
            return returnValue;
        }

        /// <summary>
        /// Extract a named property value from the metadata properties
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        private static string GetStringPropertyFromEntityMetaData(MetadataItem entitySet, string propertyName)
        {
            string returnValue = string.Empty;
            if (entitySet == null) throw new ArgumentNullException("entitySet");
            if (entitySet.MetadataProperties.TryGetValue(propertyName, false, out var metaProp))
            {
                if (metaProp != null && metaProp.Value != null)
                {
                    returnValue = metaProp.Value as string;
                }
            }
            return returnValue;
        }

        /// <summary>
        /// Get the provisioned table name for a Class (TEntity)
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="dbContext"></param>
        /// <returns></returns>
        private static string GetProvisionedTableName<TEntity>(DbContext dbContext, bool includeSchemaPrefix = false) where TEntity : class
        {
            var tableAttribute = typeof(TableAttribute);
            var tableNameAttribute = typeof(TEntity).GetCustomAttributes(tableAttribute, false).FirstOrDefault() as TableAttribute;
            if (null != tableNameAttribute)
            {
                //if its specified use that
                return tableNameAttribute.Name;
            }
            else
            {
                var entityType = typeof(TEntity);
                string returnValue;
                lock (_LockProxy)
                {
                    //Its expensive to create sort out the object name so
                    //we will cache the name, it needs to be verified
                    //because for some reason the table names are not
                    //always aligned with the EntitySet name
                    if (!_TableNameCache.TryGetValue(entityType, out returnValue))
                    {
                        //Ok its not in the cache so try to figure it out
                        var entitySet = GetMetaDataEntitySet<TEntity>(dbContext);
                        if (entitySet == null)
                        {
                            //This should be typed better, but for our example its a simple Exception
                            throw new ApplicationException("Unable to find entity set '{0}' in edm metadata" + entityType.Name);
                        }
                        returnValue = GetStringPropertyFromEntityMetaData(entitySet, "Schema") + "." + GetStringPropertyFromEntityMetaData(entitySet, "Table");
                        if (string.IsNullOrEmpty(returnValue)) throw new ArgumentException("Could not find Entity Table name for " + entityType.Name);
                        _TableNameCache.Add(entityType, returnValue);
                    }
                }
                if (!includeSchemaPrefix)
                {
                    string[] nameParts = returnValue.Split('.');
                    returnValue = nameParts.Last();
                }
                return returnValue;
            }
        }


        /// <summary>
        /// Gets a column name suitable for naming constraints
        /// and indices, not actually validated with the Model
        /// since there dont seem to be any conflicts other
        /// than ComplexTypes which are simple to calculate
        /// </summary>
        /// <param name="columnProp"></param>
        /// <returns></returns>
        private static string GetSimpleColumnName(PropertyInfo columnProp)
        {
            var columnAttribute = typeof(ColumnAttribute);
            var columnNameAttribute = columnProp.GetCustomAttributes(columnAttribute, false).FirstOrDefault() as ColumnAttribute;
            if (null != columnNameAttribute && !string.IsNullOrEmpty(columnNameAttribute.Name))
            {
                return columnNameAttribute.Name;
            }
            else
            {
                return columnProp.Name;
            }
        }

        /// <summary>
        /// Gets the names of columns with the Key attribute
        /// and return their names and types
        /// </summary>
        /// <param name="tableType"></param>
        /// <returns></returns>
        private static string[] GetKeyColumnNamesAndTypes(Type tableType, out Type[] columnTypes)
        {
            var returnValue = new List<string>();
            var keyTypes = new List<Type>();
            var keyColumns = tableType.GetProperties().Where(prop => Attribute.IsDefined(prop, typeof(System.ComponentModel.DataAnnotations.KeyAttribute))).ToArray();
            foreach (var columnInfo in keyColumns)
            {
                returnValue.Add(GetSimpleColumnName(columnInfo));
                keyTypes.Add(columnInfo.PropertyType);
            }
            columnTypes = keyTypes.ToArray();
            return returnValue.ToArray();
        }

        /// <summary>
        /// convert generic object collection to a typed collection
        /// this only works with primitives or things with explicit
        /// conversion operators
        /// </summary>
        /// <typeparam name="T">type of objects in the untypedList</typeparam>
        /// <param name="untypedList">collection of untyped objects</param>
        /// <returns>Typed collection</returns>
        private static HashSet<T> ConvertUntypedCollectionToTypedHashSet<T>(ArrayList untypedList)
        {
            var returnValue = new HashSet<T>();
            foreach (object untypedObject in untypedList)
            {
                returnValue.Add((T)untypedObject);
            }
            return returnValue;
        }
        #endregion

        #region FreeText operations
        /// <summary>
        /// Build and execute a FreeText or Contains query
        /// </summary>
        /// <param name="dbContext">active DbContext</param>
        /// <param name="tableName">Table name</param>
        /// <param name="primaryKeyColumnName">Table primary key column name</param>
        /// <param name="searchText">FREETEXT or CONTAINS formated query text</param>
        /// <param name="columnNames">array of full text indexed column names, null or 0 length means use "*"</param>
        /// <param name="useContains">this is a Contains query</param>
        /// <returns></returns>
        private static ArrayList ExecuteFreeTextSearch(DbContext dbContext, string tableName, string primaryKeyColumnName, string searchText, string[] columnNames, bool useContains)
        {
            if (string.IsNullOrWhiteSpace(searchText))
                throw new ArgumentNullException(nameof(searchText));

            string columnNameText = "*";
            if (null != columnNames && columnNames.Length > 0 && columnNames[0].Trim() != "*")
            {
                columnNameText = "(" + string.Join(",", columnNames) + ")";
            }
            var returnValue = new ArrayList();
            if (searchText.StartsWith("'")) searchText = searchText.Trim(new char[] { '\'' });
            string commandType = useContains ? "CONTAINS" : "FREETEXT";
            string sqlQuery = string.Format("SELECT {0} FROM {1} WHERE {2}({3},'{4}')", primaryKeyColumnName, tableName, commandType, columnNameText, searchText);

            dbContext.Database.Connection.Open();
            try
            {
                //Execute the query
                using (var dbCmd = dbContext.Database.Connection.CreateCommand())
                {
                    dbCmd.CommandText = sqlQuery;
                    dbCmd.CommandType = CommandType.Text;
                    using (var dbRead = dbCmd.ExecuteReader())
                    {
                        while (dbRead.Read())
                        {
                            returnValue.Add(dbRead.GetValue(0));
                        }
                    }
                }
            }
            finally
            {
                //Don't dispose it, it will be needed again
                dbContext.Database.Connection.Close();
            }
            return returnValue;
        }

        /// <summary>
        /// Internal processing of the free text query
        /// </summary>
        /// <typeparam name="TEntity">entity for our table</typeparam>
        /// <param name="query"></param>
        /// <param name="dbContext">active DbContext</param>
        /// <param name="searchText">FREETEXT or CONTAINS formated query text</param>
        /// <param name="columnNames">array of full text indexed column names, null or 0 length means use "*"</param>
        /// <returns></returns>
        internal static IQueryable<TEntity> FreeTextExInternal<TEntity>(IQueryable<TEntity> query, DbContext dbContext, string searchText, string[] columnNames, bool useContains) where TEntity : class
        {
            ///Assuming that the table has been preped for Full Text indexing we will execute
            ///a query for the primary key of matchign results, then we will convert our results
            ///to a typed collection and build an expression that will use the "Contains" method
            ///of the typed results to build this component of the expression tree. The
            ///remainder of the expression remains fully composable so things like Include and other
            ///join related queries will still work.
            ///
            /// Essentially mimicking something like this (assuming a guid based Key named UserId):
            ///     HashSet<Guid> userMatchs = "SELECT UserId from Users WHERE ....... "
            ///     (from secUser in secCtx.Users.Where(usr => userMatchs.Contains(usr.UserId)) select secUser);

            if (query == null)
                throw new ArgumentNullException(nameof(query));
            if (dbContext == null)
                throw new ArgumentNullException(nameof(dbContext));


            //Query the data model for the actual table name to query against
            string tableName = GetProvisionedTableName<TEntity>(dbContext);
            //Query the entity object for the column name
            string[] entityKeyNames = GetKeyColumnNamesAndTypes(typeof(TEntity), out var keyColumnTypes);
            if (entityKeyNames.Length == 0 || entityKeyNames.Length > 1)
            {
                //We need a single Key property , both for our Query and because its
                //a requirement of the FullText Catalog

                //This should be typed better, but for our example its a simple ApplicationException
                throw new ApplicationException(typeof(TEntity).Name + " as (dbo." + tableName + ") does not have a a valid index to support a full-text search key.");
            }
            //Ok get the matching TEntity parameter
            var parameterExpression = Expression.Parameter(query.ElementType, "p");
            //And now project the propery name on TEntity (we know its only one level deep so no need to evaluate dotted naming)
            var memberPropertyAccess = Expression.Property(parameterExpression, entityKeyNames[0]);

            //Now get the untyped collection of results from the query
            var untypedResults = ExecuteFreeTextSearch(dbContext, tableName, entityKeyNames[0], searchText, columnNames, useContains);

            // Convert the results to a typed collection so that the comparisonCondition can be correctly constructed , if we dont
            // we will get errors like this (assuming a key of type guid and an untyped array)
            //  " generic type 'System.Guid' cannot be used for parameter of type 'System.Object' of method 'Boolean Contains(System.Object)'"
            Expression comparisonCondition = null;
            LambdaExpression lambdaExpression = null;
            MethodInfo containsMethod = null;
            switch (keyColumnTypes[0].Name)
            {
                case "Guid":
                    {
                        var containedIn = ConvertUntypedCollectionToTypedHashSet<Guid>(untypedResults);
                        containsMethod = typeof(HashSet<Guid>).GetMethod("Contains", new Type[] { typeof(Guid) });
                        comparisonCondition = Expression.Call(Expression.Constant(containedIn), containsMethod, memberPropertyAccess);
                        lambdaExpression = Expression.Lambda(comparisonCondition, parameterExpression);
                    }
                    break;
                case "String":
                    {
                        var containedIn = ConvertUntypedCollectionToTypedHashSet<string>(untypedResults);
                        containsMethod = typeof(HashSet<string>).GetMethod("Contains", new Type[] { typeof(string) });
                        comparisonCondition = Expression.Call(Expression.Constant(containedIn), containsMethod, memberPropertyAccess);
                        lambdaExpression = Expression.Lambda(comparisonCondition, parameterExpression);
                    }
                    break;
                case "Int32":
                    {
                        var containedIn = ConvertUntypedCollectionToTypedHashSet<int>(untypedResults);
                        containsMethod = typeof(HashSet<int>).GetMethod("Contains", new Type[] { typeof(int) });
                        comparisonCondition = Expression.Call(Expression.Constant(containedIn), containsMethod, memberPropertyAccess);
                        lambdaExpression = Expression.Lambda(comparisonCondition, parameterExpression);
                    }
                    break;
                case "Int64":
                    {
                        var containedIn = ConvertUntypedCollectionToTypedHashSet<Int64>(untypedResults);
                        containsMethod = typeof(HashSet<Int64>).GetMethod("Contains", new Type[] { typeof(long) });
                        comparisonCondition = Expression.Call(Expression.Constant(containedIn), containsMethod, memberPropertyAccess);
                        lambdaExpression = Expression.Lambda(comparisonCondition, parameterExpression);
                    }
                    break;
                case "Int16":
                    {
                        var containedIn = ConvertUntypedCollectionToTypedHashSet<Int16>(untypedResults);
                        containsMethod = typeof(HashSet<Int16>).GetMethod("Contains", new Type[] { typeof(short) });
                        comparisonCondition = Expression.Call(Expression.Constant(containedIn), containsMethod, memberPropertyAccess);
                        lambdaExpression = Expression.Lambda(comparisonCondition, parameterExpression);
                    }
                    break;
                default:
                    throw new MissingPrimaryKeyException("unsupported primary key type " + keyColumnTypes[0].Name + " for " + tableName);
            }

            //and now complete our composition by adding the Where "our free text result keys contains(...)" clause
            var conditionResult = Expression.Call(typeof(Queryable), "Where", new[] { query.ElementType }, query.Expression, lambdaExpression);
            return query.Provider.CreateQuery<TEntity>(conditionResult);

        }
        #endregion

        #region public FreeTextSearch extension
        /// <summary>
        /// FreeTextSearch Executes a free text search (using FREETEXT) for Entities that were processed with the 
        /// SqlFullTextIndexAttribute (or are just in a free text catalog). It queries the FreeText catalog for the primary keys
        /// of the TEntity objects that are in scope for the query, then creates an expression
        /// tree to include those entities.The DbContext is used to execute these initial
        /// queries 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="dbContext">DbContext attached to TEntity</param>
        /// <param name="containsSearchText">FREETEXT formated search expresssion</param>
        /// <returns>IQueryable<TEntity></returns>
        public static IQueryable<TEntity> FreeTextSearch<TEntity>(this IQueryable<TEntity> query, DbContext dbContext, string freetextSearchText) where TEntity : class
        {
            return FreeTextExInternal(query, dbContext, freetextSearchText, null, false);
        }

        /// <summary>
        /// FreeTextSearch Executes a free text search (using FREETEXT) for Entities that were processed with the 
        /// SqlFullTextIndexAttribute (or are just in a free text catalog). It queries the FreeText catalog for the primary keys
        /// of the TEntity objects that are in scope for the query, then creates an expression
        /// tree to include those entities.The DbContext is used to execute these initial
        /// queries 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="dbContext">DbContext attached to TEntity</param>
        /// <param name="containsSearchText">FREETEXT formated search expresssion</param>
        /// <param name="columnNames">array of full text indexed column names, null or 0 length means use "*"</param>
        /// <returns>IQueryable<TEntity></returns>
        public static IQueryable<TEntity> FreeTextSearch<TEntity>(this IQueryable<TEntity> query, DbContext dbContext, string freetextSearchText, string[] columnNames) where TEntity : class
        {
            return FreeTextExInternal(query, dbContext, freetextSearchText, columnNames, false);
        }
        #endregion

        #region public FreeTextContains extension
        /// <summary>
        /// FreeTextContains Executes a free text search (using CONTAINS) for Entities that were processed with the 
        /// SqlFullTextIndexAttribute (or are just in a free text catalog). It queries the FreeText catalog for the primary keys
        /// of the TEntity objects that are in scope for the query, then creates an expression
        /// tree to include those entities. TThe DbContext is used to execute these initial
        /// queries 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="dbContext">DbContext attached to TEntity</param>
        /// <param name="containsSearchText">CONTAINS formated search expresssion</param>
        /// <returns>IQueryable<TEntity></returns>
        public static IQueryable<TEntity> FreeTextContains<TEntity>(this IQueryable<TEntity> query, DbContext dbContext, string containsSearchText) where TEntity : class
        {
            return FreeTextExInternal(query, dbContext, containsSearchText, null, true);
        }


        /// <summary>
        /// FreeTextContains Executes a free text search (using CONTAINS) for Entities that were processed with the 
        /// SqlFullTextIndexAttribute (or are just in a free text catalog). It queries the FreeText catalog for the primary keys
        /// of the TEntity objects that are in scope for the query, then creates an expression
        /// tree to include those entities. The DbContext is used to execute these initial
        /// queries 
        /// </summary>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="query"></param>
        /// <param name="dbContext">DbContext attached to TEntity</param>
        /// <param name="containsSearchText">CONTAINS formated search expresssion</param>
        /// <param name="columnNames">array of full text indexed column names, null or 0 length means use "*"</param>
        /// <returns>IQueryable<TEntity></returns>
        public static IQueryable<TEntity> FreeTextContains<TEntity>(this IQueryable<TEntity> query, DbContext dbContext, string containsSearchText, string[] columnNames) where TEntity : class
        {
            return FreeTextExInternal(query , dbContext, containsSearchText, columnNames, true);
        }
        #endregion
    }
}
