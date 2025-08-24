namespace Utilidades.Api.Models.Identity.Cache;

public record UserCreatedCache {
    public string ConnectionId { get; set; }
    public int[] UserIds { get; set; } = [];
    public int InPort { get; set; }
    public int OutPort { get; set; }
}