namespace Utilidades.Api.Models.Response;

public record LinkReference {
    public string? Href { get; set; }
    public string? Method { get; set; }
    public string? Rel { get; set; }
}