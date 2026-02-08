using SistemaContable.Entities;
using SistemaContable.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SistemaContable.Services
{
    public class Auth
    {
        private readonly SistemaContable.Repository.Auth _pantallaRepository;
        private readonly BitacoraRepository _bitacoraRepository;
        private readonly SistemaContable.Services.UsuarioService _usuarioService;

        public Auth(SistemaContable.Repository.Auth authRepository, BitacoraRepository bitacoraRepository,SistemaContable.Services.UsuarioService usuarioService)
        {
            _pantallaRepository = authRepository;
            _bitacoraRepository = bitacoraRepository;
            _usuarioService = usuarioService;
        }

        public async Task<SistemaContable.Entities.UsuarioP?> ObtenerUsuarioPorUsername(
             string userName,
             string passwordConsultado)
        {
            UsuarioP? usuarioConsultado =//
                await _pantallaRepository.ObtenerUsuarioPorUsername(userName);

            passwordConsultado = _usuarioService.HashPassword(passwordConsultado);
            if (usuarioConsultado == null || usuarioConsultado.password != passwordConsultado)
            {
                throw new Exception("Usuario o contraseña incorrecta");
            }

            return usuarioConsultado;
        }
        

    }
}
