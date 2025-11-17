using System.Linq.Expressions;
using System.Reflection;

namespace GeoAuth.Shared.Projectors;

public static class DictionaryProjector<T>
{
    public static Func<T, Dictionary<string, object>> Create()
    {
        var type = typeof(T);
        var param = Expression.Parameter(type, "obj");

        // Dictionary<string, object> ctor
        var dictCtor = typeof(Dictionary<string, object>).GetConstructor(Type.EmptyTypes) ?? throw new NullReferenceException("Can not find constructor");
        var dictVar = Expression.New(dictCtor);

        // Dictionary.Add method
        var addMethod = typeof(Dictionary<string, object>).GetMethod("Add");

        // Build element initializers for each readable property
        var initializers = type.GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead)
            .Select(p =>
                Expression.ElementInit(
                    addMethod ?? throw new MissingMethodException("The method add does not exist"),
                    Expression.Constant(p.Name),
                    Expression.Convert(Expression.Property(param, p), typeof(object))
                )
            );

        // ListInit expression: new Dictionary<string, object> { { "Prop", obj.Prop }, ... }
        var body = Expression.ListInit(dictVar, initializers);

        // Lambda: obj => new Dictionary<string, object> { ... }
        return Expression.Lambda<Func<T, Dictionary<string, object>>>(body, param).Compile();
    }
}
