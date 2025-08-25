using Utilidades.Api.Models.Identity;
using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Identity.Interface;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Services;

public interface IAuthenticationService {
    public Response<IUserLogin> AuthenticateAndGenerateToken(UserLoginDto loginDto);
    public Task<Response<IUserLogin>> AuthenticateAndGenerateTokenAsync(UserLoginDto loginDto);
}