using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Crypt;

namespace Utilidades.Api.Models.Identity.Dto;

public record UserLoginDto {
    public string Email { get; set; }
    public string Password { get; set; }
    public bool IsPwdEncrypted { get; private set; } = false;

    public string Encrypt(int i) {
        return $"{CryptWord.WordsByDay[i - 1]}{Password}".ToSHA256String();
    }

    public string Encrypt() {
        return $"{CryptWord.WordsByDay[DateTime.Now.Day - 1]}{Password}".ToSHA256String();
    }

    public void SetEncrypted() {
        Password = Encrypt();
        IsPwdEncrypted = true;
    }
};