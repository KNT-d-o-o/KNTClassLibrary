#pragma warning disable CS8603, CS8601 // this is external library supress all warnings

//using Google.Protobuf.WellKnownTypes;
using KNTCommon.Business.Models;
using KNTCommon.Business.Repositories;
using KNTCommon.Data.Models;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.Collections;
using System.Linq.Expressions;
using System.Reflection;

namespace Clifton.Lib
{
    public enum Op
    {
        Equals,
        GreaterThan,
        LessThan,
        GreaterThanOrEqual,
        LessThanOrEqual,
        Contains,
        StartsWith,
        EndsWith,
        ApproximatelyEqual,
        ContainsEnumrable,
    }

    public class Filter
    {
        public required string PropertyName { get; set; }
        public required Op Operation { get; set; }
        public required object Value { get; set; }
    }

    public static class ExpressionBuilder
    {
        private static MethodInfo containsMethod = typeof(string).GetMethod("Contains", new Type[] { typeof(string) });
        private static MethodInfo toLowerMethod = typeof(string).GetMethod("ToLower", new Type[0]);
        private static MethodInfo startsWithMethod = typeof(string).GetMethod("StartsWith", new Type[] { typeof(string) });
        private static MethodInfo endsWithMethod = typeof(string).GetMethod("EndsWith", new Type[] { typeof(string) });
        private static MethodInfo containsEnumerableMethod = typeof(Enumerable)
                                                                .GetMethods()
                                                                .First(m => m.Name == "Contains" &&
                                                                            m.GetParameters().Length == 2)
                                                                .MakeGenericMethod(typeof(int));

        public static BinaryExpression ApproximatelyEqual(Expression param, Expression actualValue)
        {
            ////meas = AutoMapper.Map<List<MeasurementDTO>>(meas.Where(x => Math.Abs(x.pDelta ?? 0) > Math.Abs(val / 1.1) && Math.Abs(x.pDelta ?? 0) < Math.Abs(val * 1.1)));
            var valx = Expression.Lambda(actualValue).Compile().DynamicInvoke();
            var val = double.Parse(valx?.ToString());

            // x.pMeasured ?? 0
            var zero = Expression.Constant(0.0, typeof(double));
            var coalesce = Expression.Coalesce(param, zero);

            // Math.Abs(x.pMeasured ?? 0)
            var absMethod = typeof(Math).GetMethod(nameof(Math.Abs), new[] { typeof(double) });
            var absPM = Expression.Call(absMethod!, coalesce);

            // Math.Abs(val / 1.1)
            var valLow = Expression.Constant(Math.Abs(val / 1.1));
            var valHigh = Expression.Constant(Math.Abs(val * 1.1));

            // Math.Abs(pMeasured) > val / 1.1
            // Math.Abs(pMeasured) < val * 1.1
            var greaterThan = Expression.GreaterThan(absPM, valLow);
            var lessThan = Expression.LessThan(absPM, valHigh);

            return Expression.AndAlso(greaterThan, lessThan);
        }

        public static Op GetOperator(string operatorstr)
        {
            switch (operatorstr)
            {
                case "=": return Op.Equals;
                case ">": return Op.GreaterThan;
                case ">=": return Op.GreaterThanOrEqual;
                case "<": return Op.LessThan;
                case "<=": return Op.LessThanOrEqual;
                case "%": return Op.Contains;
                case "%E": return Op.ContainsEnumrable;
                //case Op.StartsWith: return Op.StartsWith;
                //case "": return Op.EndsWith;
                case "=.": return Op.ApproximatelyEqual;
                default: return Op.Equals; //TODO 
            }
        }

        public static object GetValueFromString<T>(string column, string value)
        {
            var property = typeof(T).GetProperty(column);

            if (property is null)
                throw new Exception($"GetValueFromString column: {column} is not found");

            if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                return DateTime.Parse(value);
            else if (property.PropertyType == typeof(string))
                return value;
            else if (property.PropertyType == typeof(int) || property.PropertyType == typeof(int?))
                return int.Parse(value);
            else if (property.PropertyType == typeof(bool) || property.PropertyType == typeof(bool?))
                return bool.Parse(value);
            else if (property.PropertyType == typeof(float) ||  property.PropertyType == typeof(float?))
                return float.Parse(value);
            else if (property.PropertyType == typeof(double) || property.PropertyType == typeof(double?))
                return double.Parse(value);

            throw new Exception($"GetValueFromString did not found type: '{property.PropertyType}'");
        }

        public static List<Filter> CreateFilters<TDto>(string[] columns, string[] par, string[] cond)
        {
            var filters = new List<Filter>();

            for (var i = 0; i < par.Length; i++)
            {
                var column = columns[i];
                var condition = cond[i];
                var value = par[i];

                if (column == "" || value == "")
                    continue;

                if (condition == "today")
                {
                    filters.Add(new Filter()
                    {
                        PropertyName = column,
                        Operation = GetOperator(">="),
                        Value = GetValueFromString<TDto>(column, value)
                    });

                    filters.Add(new Filter()
                    {
                        PropertyName = column,
                        Operation = GetOperator("<="),
                        Value = GetValueFromString<TDto>(column, DateTime.Parse(value).AddDays(1).ToString())
                    });
                }
                else
                {
                    var filter = new Filter()
                    {
                        PropertyName = column,
                        Operation = GetOperator(condition),
                        Value = GetValueFromString<TDto>(column, value)
                    };

                    filters.Add(filter);
                }
            }

            return filters;
        }

        public static List<Filter> CreateFilters<TDto>(SearchPageArgs searchPageArgs)
        {
            SetDefaultFilters(searchPageArgs);

            var filters = new List<Filter>();

            foreach (var column in searchPageArgs.columns)
            {
                if (column.FilterParam is null || column.FilterParam == "")
                    continue;

                if (column.FilterCondition == "today")
                {
                    filters.Add(new Filter()
                    {
                        PropertyName = column.FilterColumn,
                        Operation = GetOperator(">="),
                        Value = GetValueFromString<TDto>(column.FilterColumn, column.FilterParam.ToString())
                    });

                    filters.Add(new Filter()
                    {
                        PropertyName = column.FilterColumn,
                        Operation = GetOperator("<="),
                        Value = GetValueFromString<TDto>(column.FilterColumn, DateTime.Parse(column.FilterParam.ToString()).AddDays(1).ToString())
                    });
                }
                else if (column.FilterParam is IList)
                {
                    var filter = new Filter()
                    {
                        PropertyName = column.FilterColumn,
                        Operation = GetOperator(column.FilterCondition),
                        Value = column.FilterParam
                    };

                    if (filter.Operation == Op.Contains) // change to case insensetive search, also fixed in expression
                        filter.Value = filter.Value.ToString().ToLower();
                    filters.Add(filter);                
                }
                else
                {
                    var filter = new Filter()
                    {
                        PropertyName = column.FilterColumn,
                        Operation = GetOperator(column.FilterCondition),
                        Value = GetValueFromString<TDto>(column.FilterColumn, column.FilterParam.ToString())
                    };

                    if (filter.Operation == Op.Contains) // change to case insensetive search, also fixed in expression
                        filter.Value = filter.Value.ToString().ToLower();
                    filters.Add(filter);
                }
            }

            return filters;
        }


        public static void SetDefaultFilters(SearchPageArgs searchPageArgs)
        {
            foreach (var column in searchPageArgs.columns)
            {
                if (string.IsNullOrEmpty(column.FilterCondition))
                    column.FilterCondition = "=";
                if (column.FilterParam is null || column.FilterParam.ToString() == "..." || column.FilterParam.ToString() == ResultRepository.ResultIdEmpty.ToString()) // TODO TEMP SOLUTION for result description ... mean empty
                    column.FilterParam = string.Empty;
            }                
        }

        public static IQueryable<T> Where<T>(this IQueryable<T> query, SearchPageArgs searchPageArgs)
        {
            var target = Expression.Parameter(typeof(T));

            var searchPageArgsCopy = searchPageArgs;

            // TODO check if contain any case with multiple ids
            //if (searchByMultipleResultIds)
            {
                // searchPageArgs is reference type, when searching by result in some cases same description have multiple ids, 
                // so changes are made on searchPageArgsCopy to include list of ids, original searchPageArgs is then used on FE search result page
                searchPageArgsCopy = searchPageArgs.DeepCopy();
                AddMultipleResultIds(searchPageArgsCopy);
            }            

            var filters = ExpressionBuilder.CreateFilters<T>(searchPageArgsCopy);
            if (filters.Count == 0)
                return query;

            return query.Provider.CreateQuery<T>(CreateWhereClause<T>(target, query.Expression, filters));
        }

        /// <summary>
        /// Some Results have same ResultDescription, when selecting NOK search by multiple ResultIds
        /// </summary>
        /// <param name="searchPageArgs"></param>
        static void AddMultipleResultIds(SearchPageArgs searchPageArgs)
        {
            using var context = new EdnKntControllerMysqlContext();
            var results = context.Results.ToList();

            var column = searchPageArgs.columns.Where(x => x.FilterColumn == nameof(Results.ResultId)).FirstOrDefault();

            if (column is null || column.FilterParam is null || string.IsNullOrEmpty(column.FilterParam.ToString()) || Convert.ToInt32(column.FilterParam) == ResultRepository.ResultIdEmpty)
                return;

            var description = results.Where(x => x.ResultId == (int)column.FilterParam).Select(x => x.ResultDescription).First();

            var ids = results.Where(x => x.ResultDescription == description).Select(x => x.ResultId).ToList();

            column.FilterCondition = "%E";
            column.FilterParam = ids;
        }

        // Modified from: https://stackoverflow.com/a/40090636
        // and using pieces from: https://www.codeproject.com/Tips/582450/Build-Where-Clause-Dynamically-in-Linq
        public static IQueryable<T> Where<T>(this IQueryable<T> query, List<Filter> filters)
        {
            var target = Expression.Parameter(typeof(T));

            if (filters.Count == 0)
                return query;

            return query.Provider.CreateQuery<T>(CreateWhereClause<T>(target, query.Expression, filters));
        }

        private static Expression CreateWhereClause<T>(ParameterExpression target, Expression expression, List<Filter> filters)
        {
            var predicate = Expression.Lambda(CreateComparison<T>(target, filters), target);

            return Expression.Call(typeof(Queryable), nameof(Queryable.Where), new[] { target.Type }, expression, Expression.Quote(predicate));
        }

        private static Expression CreateComparison<T>(ParameterExpression target, List<Filter> filters)
        {
            Expression exp = null;

            filters.ForEach(filter =>
            {
                var memberAccess = CreateMemberAccess(target, filter.PropertyName);
                var exp2 = GetExpression<T>(memberAccess, filter);

                //exp = exp == null ? exp2 : Expression.Or(exp, exp2);
                exp = exp == null ? exp2 : Expression.And(exp, exp2);
            });

            return exp;
        }

        private static Expression CreateMemberAccess(Expression target, string selector)
        {
            return selector.Split('.').Aggregate(target, Expression.PropertyOrField);
        }

        private static PropertyInfo GetPropertyInfo(Type baseType, string propertyName)
        {
            string[] parts = propertyName.Split('.');

            return (parts.Length > 1)
                ? GetPropertyInfo(baseType.GetProperty(parts[0]).PropertyType, parts.Skip(1).Aggregate((a, i) => a + "." + i))
                : baseType.GetProperty(propertyName);
        }

        public static Expression GetTypedSelector<T, R>(Filter filter) where R : struct
        {
            // We actually need to get the property type, chaining from the container class,
            // and converting the value type to the property type using Expression.Convert
            var pi = GetPropertyInfo(typeof(T), filter.PropertyName);

            // This seems to be the preferred way.
            // Alternate: if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Nullable<>))
            var propIsNullable = Nullable.GetUnderlyingType(pi.PropertyType) != null;

            Expression<Func<object>> valueSelector = () => filter.Value;
            Expression expr = propIsNullable ? Expression.Convert(valueSelector.Body, typeof(R?)) : Expression.Convert(valueSelector.Body, typeof(R));

            return expr;
        }

        public static Expression GetSelector<T>(Filter filter)
        {
            switch (filter.Value)
            {
                case int t1: return GetTypedSelector<T, int>(filter);
                case float f1: return GetTypedSelector<T, float>(filter);
                case double d1: return GetTypedSelector<T, double>(filter);
                case long l1: return GetTypedSelector<T, long>(filter);
                case DateTime dt1: return GetTypedSelector<T, DateTime>(filter);
                case bool b1: return GetTypedSelector<T, bool>(filter);
                case decimal d1: return GetTypedSelector<T, decimal>(filter);
                case char c1: return GetTypedSelector<T, char>(filter);
                case byte by1: return GetTypedSelector<T, byte>(filter);
                case short sh1: return GetTypedSelector<T, short>(filter);
                case ushort ush1: return GetTypedSelector<T, ushort>(filter);
                case uint ui1: return GetTypedSelector<T, uint>(filter);
                case ulong ul1: return GetTypedSelector<T, ulong>(filter);
                case string s1:
                    {
                        Expression<Func<string>> valueSelector = () => (string)filter.Value;
                        return valueSelector.Body;
                    }
                default: return null;
            }
        }

        private static Expression GetExpression<T>(Expression member, Filter filter)
        {
            // How do we turn this into a SQL parameter so we're not susceptible to SQL injection attacks?
            // Like this: https://stackoverflow.com/a/71019524
            //Expression<Func<object>> valueSelector = () => filter.Value;
            //var actualValue = valueSelector.Body;

            var actualValue = GetSelector<T>(filter);

            switch (filter.Operation)
            {
                case Op.Equals: return Expression.Equal(member, actualValue);
                case Op.GreaterThan: return Expression.GreaterThan(member, actualValue);
                case Op.GreaterThanOrEqual: return Expression.GreaterThanOrEqual(member, actualValue);
                case Op.LessThan: return Expression.LessThan(member, actualValue);
                case Op.LessThanOrEqual: return Expression.LessThanOrEqual(member, actualValue);
                case Op.Contains:
                    var memeberToLower = Expression.Call(member, toLowerMethod);
                    return Expression.Call(memeberToLower, containsMethod, actualValue);
                
                case Op.ContainsEnumrable:                    
                    var convertedProperty = Expression.Convert(member, typeof(int));
                    return Expression.Call(null, containsEnumerableMethod, Expression.Constant(filter.Value), convertedProperty);

                case Op.StartsWith: return Expression.Call(member, startsWithMethod, actualValue);
                case Op.EndsWith: return Expression.Call(member, endsWithMethod, actualValue);
                case Op.ApproximatelyEqual: return ApproximatelyEqual(member, actualValue);
            }

            return null;
        }
        /*
         https://gist.github.com/afreeland/6733381?permalink_comment_id=2599909#gistcomment-2599909
         Contains, does not work if there is case sensitive data. So i made a slight change to contains, in case anyone needs it. Basically, use indexof

        var pi = param.Type.GetProperty(filter.PropertyName);
        var propertyAccess = Expression.MakeMemberAccess(param, pi);
        var indexOf = Expression.Call(propertyAccess, "IndexOf", null, Expression.Constant(constant.Value, typeof(string)), Expression.Constant(StringComparison.InvariantCultureIgnoreCase));
        return Expression.GreaterThanOrEqual(indexOf, Expression.Constant(0));
        */
    }
}


#pragma warning restore CS8603, CS8601
