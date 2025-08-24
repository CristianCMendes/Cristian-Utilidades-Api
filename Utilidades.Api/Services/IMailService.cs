using Microsoft.AspNetCore.Html;

namespace Utilidades.Api.Services;

public interface IMailService {
    Task SendMailAsync(string subject, HtmlString body, string toEmail, CancellationToken cancellationToken = default);
}