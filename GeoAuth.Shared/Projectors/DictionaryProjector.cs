using System.Linq.Expressions;
using System.Reflection;

namespace GeoAuth.Shared.Projectors;

public static class DictionaryProjector<T>
{
    public static Func<T, Dictionary<string, object>> Serialise()
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

    public static Func<Dictionary<string, object>, T> Hydrator()
    {
        var dictParam = Expression.Parameter(typeof(Dictionary<string, object>), "dict");
        var resultVar = Expression.Variable(typeof(T), "result");

        var newResult = Expression.New(typeof(T));
        var assignResult = Expression.Assign(resultVar, newResult);

        var tryGetValue = typeof(Dictionary<string, object>)
            .GetMethod("TryGetValue", new[] { typeof(string), typeof(object).MakeByRefType() });

        var variables = new List<ParameterExpression> { resultVar };
        var expressions = new List<Expression> { assignResult };

        foreach (var prop in typeof(T).GetProperties().Where(p => p.CanWrite))
        {
            var outVar = Expression.Variable(typeof(object), "out_" + prop.Name);
            variables.Add(outVar); // declare it in the block scope

            var tryCall = Expression.Call(dictParam, tryGetValue,
                Expression.Constant(prop.Name),
                outVar);

            var assignProp = Expression.Assign(
                Expression.Property(resultVar, prop),
                Expression.Convert(outVar, prop.PropertyType));

            var ifTrue = Expression.IfThen(tryCall, assignProp);

            expressions.Add(ifTrue);
        }

        expressions.Add(resultVar); // return result

        var body = Expression.Block(variables, expressions);

        return Expression.Lambda<Func<Dictionary<string, object>, T>>(body, dictParam).Compile();
    }

}
