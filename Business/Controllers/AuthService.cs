using Azure.Core;
using Business.Authentication;
using Business.Utils;
using Data.Repositories;
using Domain.Controller.Private.Auth;
using Microsoft.AspNetCore.Http;

namespace Business.Controllers
{
    public class AuthService(AuthenticationService authService, UsuarioRepository usuarioRepository)
    {
        private readonly AuthenticationService _authService = authService;
        private readonly UsuarioRepository _usuarioRepository = usuarioRepository;

        public async Task<string?> GenerateAuthUserToken (GenerateUserTokenDto data)
        {
            if (string.IsNullOrEmpty(data.UsuarioNombre) || string.IsNullOrEmpty(data.Password))
            {
                return null;
            }

            var usuario = await _usuarioRepository.GetOneByFilter(x => x.UsuarioNombre.Equals(data.UsuarioNombre) && !x.Eliminado);

            if (usuario == null || usuario.Activo == false) {
                return null;
            }

            var isCorrectPassword = PasswordHasher.VerifyPassword(data.Password, usuario.Password);

            if (!isCorrectPassword)
            {
                return null;
            }

            return _authService.GenerateUserToken(usuario.Id, usuario.RolId);
        }

        public async Task<GetUserInfoResponse?> GetUserInfoByToken(string token)
        {
            var tokenInfo = _authService.GetTokenUserInfo(token);

            if (tokenInfo == null) { 
                return null;
            }

            var usuario = await _usuarioRepository.GetById(tokenInfo.UsuarioId, "Rol");

            if (usuario == null || usuario.Activo == false || usuario.Eliminado == true)
            {
                return null;
            }

            return new()
            {
                Id = usuario.Id,
                Rol = new()
                {
                    Id = usuario.Rol.Id,
                    Nombre = usuario.Rol.Nombre
                },
                Nombre = usuario.Nombre,
                UsuarioNombre = usuario.UsuarioNombre
            };
        }

        public async Task<GetUserInfoResponse?> GetUserInfoByHeader(string? authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return null;
            }

            string token = authHeader["Bearer ".Length..].Trim();
            
            return await GetUserInfoByToken(token);
        }
    }
}
