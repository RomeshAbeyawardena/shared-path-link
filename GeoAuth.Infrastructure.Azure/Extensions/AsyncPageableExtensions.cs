using Azure;
using Azure.Data.Tables;

namespace GeoAuth.Infrastructure.Azure.Extensions;

public static class AsyncPageableExtensions
{
    public static async Task<T?> FirstOrDefaultAsync<T>(this AsyncPageable<T> source, CancellationToken cancellationToken = default)
        where T : class, ITableEntity
    {
        await foreach (var item in source.WithCancellation(cancellationToken))
        {
            return item; // Return the first item found
        }

        return default; // No match found
    }
}
