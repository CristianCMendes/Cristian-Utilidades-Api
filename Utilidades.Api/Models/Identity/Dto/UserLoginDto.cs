using Microsoft.CodeAnalysis.Elfie.Extensions;
using Utilidades.Api.Extensions;

namespace Utilidades.Api.Models.Identity.Dto;

public class UserLoginDto {
    private string _password;
    public string Email { get; set; }

    public string Password {
        get => _password;
        set => _password = value.ToSHA256String();
    }
};