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

    public ResponseMessage(string message, MessageType type) {
        Message = message;
        Type = type;
    }

    public ResponseMessage() { }
}