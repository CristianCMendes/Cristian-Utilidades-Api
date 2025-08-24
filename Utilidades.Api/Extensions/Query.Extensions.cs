using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Extensions;

public static class QueryExtensions {
    public static (T[] Data, PaginationResponse Pagination)
        Paginate<T>(this IQueryable<T> query, Pagination pagination) {
        return (query.Skip(pagination.Skip).Take(pagination.Take).ToArray(), new() {
            Total = query.Count(),
        });
    }

    public static async Task<(T[] Data, PaginationResponse Pagination)>
        PaginateAsync<T>(this IQueryable<T> query, Pagination pagination,
            CancellationToken cancellationToken = default) {
        return (await query.Skip(pagination.Skip).Take(pagination.Take).ToArrayAsync(cancellationToken), new(pagination) {
            Total = await query.CountAsync(cancellationToken),
        });
    }
}