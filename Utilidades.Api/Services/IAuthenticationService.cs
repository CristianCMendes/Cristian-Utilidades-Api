using Utilidades.Api.Models.Identity.Dto;
using Utilidades.Api.Models.Response;

namespace Utilidades.Api.Services;

public interface IAuthenticationService {
    public Response<UserLoginResponse> AuthenticateAndGenerateToken(UserLoginDto loginDto);
    public Task<Response<UserLoginResponse>> AuthenticateAndGenerateTokenAsync(UserLoginDto loginDto);
}