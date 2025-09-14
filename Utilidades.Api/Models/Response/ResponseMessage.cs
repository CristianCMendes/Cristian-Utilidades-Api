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
    public bool Important { get; set; } = false;

    public ResponseMessage(string message, MessageType type, bool important = false) {
        Message = message;
        Type = type;
    }

    public ResponseMessage() { }
}