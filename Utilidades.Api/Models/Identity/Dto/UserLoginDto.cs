using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Elfie.Extensions;
using Utilidades.Api.Extensions;
using Utilidades.Api.Models.Crypt;

namespace Utilidades.Api.Models.Identity.Dto;

public class UserLoginDto {
    public string Email { get; set; }
    public string Password { get; set; }

    public string Encrypt(int i) {
        return $"{CryptWord.WordsByDay[i]}{Password}".ToSHA256String();
    }

    public string Encrypt() {
        return $"{CryptWord.WordsByDay[DateTime.Now.Day]}{Password}".ToSHA256String();
    }
};