using System.Linq.Expressions;

namespace Flavian.Persistence.Helper;

public static class QueryableExtensions
{
    public static IQueryable<T> WhereNotDeleted<T>(this IQueryable<T> query)
    {
        if (typeof(T).GetProperty("IsDeleted")?.PropertyType != typeof(bool)) return query;
        var parameter = Expression.Parameter(typeof(T), "e");
        var property = Expression.Property(parameter, "IsDeleted");
        var condition = Expression.Equal(property, Expression.Constant(false));
        var lambda = Expression.Lambda<Func<T, bool>>(condition, parameter);
        return query.Where(lambda);
    }
}
