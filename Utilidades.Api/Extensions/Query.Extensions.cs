using Microsoft.EntityFrameworkCore;
using Utilidades.Api.Models.Pagination;

namespace Utilidades.Api.Extensions;

public static class QueryExtensions {
    public static (T[] Data, PaginationResponse Pagination)
        Paginate<T>(this IQueryable<T> query, Pagination pagination) {
        var p = new PaginationResponse(pagination);

        return (query.Skip(p.Skip).Take(p.Take).ToArray(), p with {
            Total = query.Count(),
        });
    }

    public static async Task<(T[] Data, PaginationResponse Pagination)>
        PaginateAsync<T>(this IQueryable<T> query, Pagination pagination,
            CancellationToken cancellationToken = default) {
        var p = new PaginationResponse(pagination);

        return (await query.Skip(p.Skip).Take(p.Take).ToArrayAsync(cancellationToken), p with {
            Total = await query.CountAsync(cancellationToken),
        });
    }
}