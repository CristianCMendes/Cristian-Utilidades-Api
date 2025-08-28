namespace Utilidades.Api.Models.Response;

public enum Method {
    GET,
    POST,
    PUT,
    DELETE
}

public record LinkReference {
    public string? Href { get; set; }
    public Method? Method { get; set; }
    public string? Rel { get; set; }

    public LinkReference() { }

    public LinkReference(string? href, Method? method, string? rel) {
        Href = href;
        Method = method;
        Rel = rel;
    }
}