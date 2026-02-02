using SistemaContable.Entities;
using SistemaContable.Repository;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace SistemaContable.Services
{
    public class Auth
    {
        private readonly SistemaContable.Repository.Auth _pantallaRepository;
        private readonly BitacoraRepository _bitacoraRepository;

        public Auth(SistemaContable.Repository.Auth authRepository, BitacoraRepository bitacoraRepository)
        {
            _pantallaRepository = authRepository;
            _bitacoraRepository = bitacoraRepository;
        }

        public async Task<SistemaContable.Entities.Usuario?> ObtenerUsuarioPorUsername(
             string userName,
             string passwordConsultado)
        {
            Usuario? usuarioConsultado =
                await _pantallaRepository.ObtenerUsuarioPorUsername(userName);

             if (usuarioConsultado == null || usuarioConsultado.password != passwordConsultado)
            {
                throw new Exception("Usuario o contraseña incorrecta");
            }

            return usuarioConsultado;
        }

    }
}
