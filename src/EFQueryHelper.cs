using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace QueryHelper
{
    class EFQueryHelper<T>
    {
        private static readonly MethodInfo StringContainsMethod = typeof(string).GetMethod("Contains", new[] { typeof(string) });
        private static readonly ParameterExpression ParameterExpression = Expression.Parameter(typeof (T)); 

        public Expression<Func<T, bool>>  CreateFilterExpression(string whereClause)
        {
            if (string.IsNullOrEmpty(whereClause)) return null;
            //TODO: only supports *simple* expressions. e.g. the value can't have commas or equal signs
            var whereStatements = whereClause.Split(',');
            Expression<Func<T, bool>> expression = null;
            foreach (var whereStatement in whereStatements)
            {
                var binaryExpression = BuildExpression(whereStatement);
                var whereExpression = Expression.Lambda<Func<T, bool>>(binaryExpression, ParameterExpression);
                expression = expression == null
                        ? whereExpression
                        : AndWhere(expression, whereExpression);
            }
            return expression;
        }

        public Expression<Func<T, bool>> CreateSearchExpression(string searchClause)
        {
            if (string.IsNullOrEmpty(searchClause)) return null;
            //TODO: only supports *simple* expressions. e.g. the value can't have commas or equal signs
            var searchStatements = searchClause.Split(',');
            Expression<Func<T, bool>> expression = null;
            foreach (var searchStatement in searchStatements)
            {
                var tuple = searchStatement.Split('~');
                ValidatePropertyValuePair(tuple);
                ValidateSearchValue(tuple);
                var callExpression = StringContainsExpression(tuple[0], tuple[1]);
                var whereExpression = Expression.Lambda<Func<T, bool>>(callExpression, ParameterExpression);
                expression = expression == null
                        ? whereExpression
                        : AndWhere(expression, whereExpression);
            }
            return expression;
        }

        public IOrderedQueryable<T> AddOrderByClause(IQueryable<T> query, string orderByClause, bool desc)
        {
            var orderByStatements = orderByClause.Split(',');
            IOrderedQueryable<T> orderedQuery = null;
            foreach (var orderByStatement in orderByStatements)
            {
                ValidatePropertyName(orderByStatement);
                Expression orderByProperty = Expression.Property(ParameterExpression, orderByStatement);
                var orderByExpression = Expression.Lambda<Func<T, object>>(orderByProperty, ParameterExpression);

                orderedQuery = orderedQuery == null
                    ? (desc ? query.OrderByDescending(orderByExpression) : query.OrderBy(orderByExpression))
                    : (desc ? orderedQuery.ThenByDescending(orderByExpression) : orderedQuery.ThenBy(orderByExpression));
            }
            return orderedQuery;
        }

        private PropertyInfo GetPocoPropertyInfo(string propertyName)
        {
            var pocoMeta = PocoPropertyCache.GetPocoMeta(typeof(T));
            return pocoMeta.MappedProperties
                .FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase));
        }

        private BinaryExpression BuildExpression(string whereStatement)
        {
            BinaryExpression expression = null;
            if (whereStatement.Contains(">="))
            {
                var tuple = whereStatement.Split(new[] { ">=" }, StringSplitOptions.None);
                ValidatePropertyValuePair(tuple);
                var parsedValue = ParseValue(tuple[0], tuple[1]);
                expression = Expression.GreaterThanOrEqual(Expression.Property(ParameterExpression, tuple[0]), Expression.Constant(parsedValue));
            }
            else if (whereStatement.Contains("<="))
            {
                var tuple = whereStatement.Split(new[] { "<=" }, StringSplitOptions.None);
                ValidatePropertyValuePair(tuple);
                var parsedValue = ParseValue(tuple[0], tuple[1]);
                expression = Expression.LessThanOrEqual(Expression.Property(ParameterExpression, tuple[0]), Expression.Constant(parsedValue));
            }
            else if (whereStatement.Contains("!="))
            {
                var tuple = whereStatement.Split(new[] { "!=" }, StringSplitOptions.None);
                ValidatePropertyValuePair(tuple);
                var parsedValue = ParseValue(tuple[0], tuple[1]);
                expression = Expression.NotEqual(Expression.Property(ParameterExpression, tuple[0]), Expression.Constant(parsedValue));
            }
            else if (whereStatement.Contains(">"))
            {
                var tuple = whereStatement.Split(new[] { ">" }, StringSplitOptions.None);
                ValidatePropertyValuePair(tuple);
                var parsedValue = ParseValue(tuple[0], tuple[1]);
                expression = Expression.GreaterThan(Expression.Property(ParameterExpression, tuple[0]), Expression.Constant(parsedValue));
            }
            else if (whereStatement.Contains("<"))
            {
                var tuple = whereStatement.Split(new[] { "<" }, StringSplitOptions.None);
                ValidatePropertyValuePair(tuple);
                var parsedValue = ParseValue(tuple[0], tuple[1]);
                expression = Expression.LessThan(Expression.Property(ParameterExpression, tuple[0]), Expression.Constant(parsedValue));
            }
            else if (whereStatement.Contains("="))
            {
                var tuple = whereStatement.Split('=');
                ValidatePropertyValuePair(tuple);
                var parsedValue = ParseValue(tuple[0], tuple[1]);
                expression = Expression.Equal(Expression.Property(ParameterExpression, tuple[0]), Expression.Constant(parsedValue));
            }
            else
            {
                throw new ArgumentException("Bad where clause: The clause '" + whereStatement + "' is invalid. It must contain =, !=, >, >=, <, <=, ~, with a valid property name and value.");
            }
            return expression;
        }

        private object ParseValue(string propertyName, string value)
        {
            var propertyInfo = GetPocoPropertyInfo(propertyName);
            if (propertyInfo.PropertyType == typeof (string))
            {
                return value;
            }
            if (propertyInfo.PropertyType == typeof (int))
            {
                int intValue = 0;
                if (int.TryParse(value, out intValue))
                {
                    return intValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid integer.");
            }
            if (propertyInfo.PropertyType == typeof(long))
            {
                long longValue = 0;
                if (long.TryParse(value, out longValue))
                {
                    return longValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid long integer.");
            }
            if (propertyInfo.PropertyType == typeof(uint))
            {
                uint intValue = 0;
                if (uint.TryParse(value, out intValue))
                {
                    return intValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid unsigned integer.");
            }
            if (propertyInfo.PropertyType == typeof(ulong))
            {
                ulong longValue = 0;
                if (ulong.TryParse(value, out longValue))
                {
                    return longValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid unsigned long integer.");
            }
            if (propertyInfo.PropertyType == typeof(short))
            {
                short shortValue = 0;
                if (short.TryParse(value, out shortValue))
                {
                    return shortValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid short integer.");
            }
            if (propertyInfo.PropertyType == typeof(ushort))
            {
                ushort shortValue = 0;
                if (ushort.TryParse(value, out shortValue))
                {
                    return shortValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid unsigned short integer.");
            }
            if (propertyInfo.PropertyType == typeof(char))
            {
                char charValue = '\0';
                if (char.TryParse(value, out charValue))
                {
                    return charValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid character.");
            }
            if (propertyInfo.PropertyType == typeof(DateTime))
            {
                DateTime dtValue;
                if (DateTime.TryParse(value, out dtValue))
                {
                    return dtValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid date-time.");
            }
            if (propertyInfo.PropertyType == typeof(float))
            {
                float floatValue = 0;
                if (float.TryParse(value, out floatValue))
                {
                    return floatValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid float.");
            }
            if (propertyInfo.PropertyType == typeof(double))
            {
                double doubleValue = 0;
                if (double.TryParse(value, out doubleValue))
                {
                    return doubleValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid double.");
            }
            if (propertyInfo.PropertyType == typeof(decimal))
            {
                decimal decimalValue = 0;
                if (decimal.TryParse(value, out decimalValue))
                {
                    return decimalValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid decimal.");
            }
            if (propertyInfo.PropertyType == typeof (TimeSpan))
            {
                TimeSpan tsValue;
                if (TimeSpan.TryParse(value, out tsValue))
                {
                    return tsValue;
                }
                throw new ArgumentException("The value '" + value + "' must be a valid time-span, usually in the format of DD.HH:MM:SS.");
            }
            return null; 
        }

        private void ValidatePropertyValuePair(string[] tuple)
        {
            if (tuple.Length != 2 || string.IsNullOrEmpty(tuple[0])) throw new ArgumentException("Error. Missing property name or comparison operator. The clause must have a valid property name.");
            ValidatePropertyName(tuple[0]);
        }

        private void ValidatePropertyName(string propertyName)
        {
            var propertyInfo = GetPocoPropertyInfo(propertyName);
            if (propertyInfo == null) throw new ArgumentException("Error. Invalid property name. The specified property '" + propertyName + "' is not a valid property name.");
        }

        private void ValidateSearchValue(string[] tuple)
        {
            if (string.IsNullOrEmpty(tuple[1])) throw new ArgumentException("Error. Missing property value or '~' operator. The search clause must have a valid string value.");
            var propertyInfo = GetPocoPropertyInfo(tuple[0]);
            if (propertyInfo.PropertyType != typeof(string)) throw new ArgumentException("You can only perform searches on properties with a String data type. The property '" + tuple[0] + "' is not a String.");
        }

        private Expression<Func<T, bool>> AndWhere(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
        {
            var combined = Expression.Lambda<Func<T, bool>>(
                Expression.AndAlso(left.Body, right.Body), ParameterExpression);
            return combined;
        }

        private MethodCallExpression StringContainsExpression(string propertyName, string propertyValue)
        {
            var callExpression = Expression.Call(
                Expression.Property(ParameterExpression, propertyName),
                StringContainsMethod,
                Expression.Constant(propertyValue, typeof (string)));
            return callExpression;
        }
    }
}