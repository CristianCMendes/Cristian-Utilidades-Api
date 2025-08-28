namespace Utilidades.Api.Models.Response;
public enum MessageType {
    warning,
    error,
    success,
    info
}
public class ResponseMessage {
    public string Message { get; set; }
    public MessageType Type { get; set; }
}