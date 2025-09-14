using Utilidades.Api.Extensions;

namespace Utilidades.Api.Models.Identity.Dto;

public record UserCreateDto : UserLoginDto {
    private string _name;

    public string Name {
        get => _name;
        set => _name = value.ToCapitalized();
    }
};