namespace GeoAuth.Shared.Models.Records;

public abstract record MappableBase<T> : IMappable<T>
{
    protected abstract T Source { get; }
    public abstract void Map(T source);
    
    public TResult Map<TResult>() where TResult : IMappable<T>, new()
    {
        return Map(() => new TResult());
    }

    public TResult Map<TResult>(Func<TResult> instanceFactory) where TResult : IMappable<T>
    {
        var result = instanceFactory();
        result.Map(Source);
        return result;
    }
}
