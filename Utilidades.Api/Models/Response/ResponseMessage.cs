namespace Utilidades.Api.Models.Response;
public enum MessageType {
    Warning,
    Error,
    Success,
    Info,
}
public class ResponseMessage {


    public string Message { get; set; }
    public MessageType Type { get; set; }
}