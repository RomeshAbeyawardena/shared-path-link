using GeoAuth.Shared.Models;

namespace GeoAuth.Infrastructure;

public interface IFilter
{

}

public interface IFilter<TFilter> : IMappable<TFilter>, IFilter
{

}
