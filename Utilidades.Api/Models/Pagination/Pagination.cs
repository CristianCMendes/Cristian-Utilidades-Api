using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace Utilidades.Api.Models.Pagination;

public class Pagination : IPagination {
    private int _page = 1;
    private int _pageSize = 10;

    /// <inheritdoc />
    [FromQuery]
    public int Page {
        get => _page;
        set => _page = int.Max(1, value);
    }

    /// <inheritdoc />
    [FromQuery]
    public int PageSize {
        get => _pageSize;
        set => _pageSize = int.Clamp(value, 1, 50);
    }

    [JsonIgnore]public int Skip => int.Max(0, (Page - 1) * PageSize);
    [JsonIgnore]public int Take => int.Max(1, PageSize);
}