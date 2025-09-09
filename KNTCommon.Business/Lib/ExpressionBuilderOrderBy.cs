using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;


#pragma warning disable CS8602, CS8603 // this is external library supress all warnings


namespace Clifton.Lib // TODO this is not correct namespace, but it is used at same time
{
    // Copied from:
    // https://stackoverflow.com/a/40572006 -> https://stackoverflow.com/questions/2728340/how-can-i-do-an-orderby-with-a-dynamic-string-parameter
    // https://stackoverflow.com/a/16208620/111438
    public static class ExpressionBuilderOrderBy
    {
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return query;

            var keys = key.Split(" ");
            var column = keys.First();
            var ascending = (keys.Count() > 1 && keys[1].ToLower() == "desc") ? false : true;
            var ascendingStr = ascending ? "asc" : "desc";
            return query.OrderBy(column, ascending);
            //return query.OrderBy(column, ascendingStr);
        }

        /*
        /// <summary>
        /// Sorts the elements of a sequence according to a key and the sort order.
        /// </summary>
        /// <typeparam name="TSource">The type of the elements of <paramref name="query" />.</typeparam>
        /// <param name="query">A sequence of values to order.</param>
        /// <param name="key">Name of the property of <see cref="TSource"/> by which to sort the elements.</param>
        /// <param name="ascending">True for ascending order, false for descending order.</param>
        /// <returns>An <see cref="T:System.Linq.IOrderedQueryable`1" /> whose elements are sorted according to a key and sort order.</returns>
        public static IQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> query, string key, bool ascending = true)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return query;
            }

            var lambda = (dynamic)CreateExpression(typeof(TSource), key);

            return ascending
                ? Queryable.OrderBy(query, lambda)
                : Queryable.OrderByDescending(query, lambda);
        }

        private static LambdaExpression CreateExpression(Type type, string propertyName)
        {
            var param = Expression.Parameter(type, "x");

            Expression body = param;
            foreach (var member in propertyName.Split('.'))
            {
                body = Expression.PropertyOrField(body, member);
            }

            return Expression.Lambda(body, param);
        }
        */




        /*
        // bit faster then upper orderby
        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string field, string dir = "asc")
        {
            // parametro => expressão
            var parametro = Expression.Parameter(typeof(TSource), "r");
            var expressao = Expression.Property(parametro, field);
            var lambda = Expression.Lambda(expressao, parametro); // r => r.AlgumaCoisa
            var tipo = typeof(TSource).GetProperty(field).PropertyType;

            var nome = "OrderBy";
            if (string.Equals(dir, "desc", StringComparison.InvariantCultureIgnoreCase))
            {
                nome = "OrderByDescending";
            }
            var metodo = typeof(Queryable).GetMethods().First(m => m.Name == nome && m.GetParameters().Length == 2);
            var metodoGenerico = metodo.MakeGenericMethod(new[] { typeof(TSource), tipo });
            return metodoGenerico.Invoke(source, new object[] { source, lambda }) as IOrderedQueryable<TSource>;

        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, string field, string dir = "asc")
        {
            var parametro = Expression.Parameter(typeof(TSource), "r");
            var expressao = Expression.Property(parametro, field);
            var lambda = Expression.Lambda<Func<TSource, string>>(expressao, parametro); // r => r.AlgumaCoisa
            var tipo = typeof(TSource).GetProperty(field).PropertyType;

            var nome = "ThenBy";
            if (string.Equals(dir, "desc", StringComparison.InvariantCultureIgnoreCase))
            {
                nome = "ThenByDescending";
            }

            var metodo = typeof(Queryable).GetMethods().First(m => m.Name == nome && m.GetParameters().Length == 2);
            var metodoGenerico = metodo.MakeGenericMethod(new[] { typeof(TSource), tipo });
            return metodoGenerico.Invoke(source, new object[] { source, lambda }) as IOrderedQueryable<TSource>;
        }
        */


        private static MethodInfo orderByMethod = typeof(Queryable).GetMethods().First(m => m.Name == "OrderBy" && m.GetParameters().Length == 2);
        private static MethodInfo OrderByDescendingMethod = typeof(Queryable).GetMethods().First(m => m.Name == "OrderByDescending" && m.GetParameters().Length == 2);

        public static IOrderedQueryable<TSource> OrderBy<TSource>(this IQueryable<TSource> source, string field, bool asc)
        {
            if (string.IsNullOrEmpty(field))
                throw new Exception("OrderBy field cannot be empty!");

            // parametro => expressão
            var parametro = Expression.Parameter(typeof(TSource), "r");
            var expressao = Expression.Property(parametro, field);
            var lambda = Expression.Lambda(expressao, parametro); // r => r.AlgumaCoisa
            var tipo = typeof(TSource).GetProperty(field).PropertyType;
                        

            var metodo = (asc) ? orderByMethod: OrderByDescendingMethod;
            var metodoGenerico = metodo.MakeGenericMethod(new[] { typeof(TSource), tipo });
            return metodoGenerico.Invoke(source, new object[] { source, lambda }) as IOrderedQueryable<TSource>;

        }

        public static IOrderedQueryable<TSource> ThenBy<TSource>(this IOrderedQueryable<TSource> source, string field, string dir = "asc")
        {
            var parametro = Expression.Parameter(typeof(TSource), "r");
            var expressao = Expression.Property(parametro, field);
            var lambda = Expression.Lambda<Func<TSource, string>>(expressao, parametro); // r => r.AlgumaCoisa
            var tipo = typeof(TSource).GetProperty(field).PropertyType;

            var nome = "ThenBy";
            if (string.Equals(dir, "desc", StringComparison.InvariantCultureIgnoreCase))
            {
                nome = "ThenByDescending";
            }

            var metodo = typeof(Queryable).GetMethods().First(m => m.Name == nome && m.GetParameters().Length == 2);
            var metodoGenerico = metodo.MakeGenericMethod(new[] { typeof(TSource), tipo });
            return metodoGenerico.Invoke(source, new object[] { source, lambda }) as IOrderedQueryable<TSource>;
        }
    }
}
#pragma warning restore CS8602, CS8603
