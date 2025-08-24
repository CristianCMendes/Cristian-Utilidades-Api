using System.Text.Json.Serialization;
using Utilidades.Api.Models.Identity.Dto;

namespace Utilidades.Api.Models.Identity;

public record User : UserResponse {
    [JsonIgnore] public string Password { get; set; }
    
    public static implicit operator User(UserCreateDto userCreate) {
        return new() {
            Name = userCreate.Name,
            Email = userCreate.Email,
            Password = userCreate.Password
        };
    }
}