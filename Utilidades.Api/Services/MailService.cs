using System.Net;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Html;
using MimeKit;

namespace Utilidades.Api.Services;

public class MailService(IConfiguration configuration) : IMailService {
    /// <inheritdoc />
    public async Task SendMailAsync(string subject, HtmlString body, string toEmail,
        CancellationToken cancellationToken = default) {

        var section = configuration.GetSection("Mail");
        var mailClient = new SmtpClient();
        var host = section["HOST"];
        var port = int.Parse(section["PORT"] ?? "587");
        var username = section["USERNAME"];
        var password = section["PASSWORD"];

        if (string.IsNullOrEmpty(host)) {
            throw new NullReferenceException("Host de email não definido nas variaveis do ambiente");
        }

        if (string.IsNullOrEmpty(username)) {
            throw new NullReferenceException("Username de e-mail não definido nas variaveis do ambiente");
        }

        if (string.IsNullOrEmpty(password)) {
            throw new NullReferenceException("Senha de e-mail não definido nas variaveis do ambiente");
        }

        await mailClient.ConnectAsync(host,
            port, cancellationToken: cancellationToken);

        await mailClient.AuthenticateAsync(new NetworkCredential() {
            UserName = username,
            Password = password
        }, cancellationToken);

        await mailClient.SendAsync(FormatOptions.Default, MimeMessage.CreateFromMailMessage(new() {
            From = new(username),
            Body = body.ToString(),
            IsBodyHtml = true,
            To = { toEmail },
            Subject = subject,
        }), cancellationToken);

    }
}