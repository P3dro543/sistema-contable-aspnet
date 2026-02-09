using SistemaContable.Entities.ViewModels;
using SistemaContable.Repository;
using System.Text.Json;
using SistemaContable.Entities; // Para Bitacora

namespace SistemaContable.Services
{
    public class CierreContableService
    {
        private readonly CierreContableRepository _repo;
        private readonly PeriodoContableRepository _repoPeriodos;
        private readonly BitacoraRepository _bitacora;

        public CierreContableService(CierreContableRepository repo, PeriodoContableRepository repoPeriodos, BitacoraRepository bitacora)
        {
            _repo = repo;
            _repoPeriodos = repoPeriodos;
            _bitacora = bitacora;
        }

        public async Task<ResultadoCierreViewModel> PrevisualizarCierre(int idPeriodo, int pagina = 1)
        {
            const int TAMANO_PAGINA = 10;
            var periodo = await _repoPeriodos.ObtenerPorId(idPeriodo);
            var resultado = new ResultadoCierreViewModel { PaginaActual = pagina };

            if (periodo == null || periodo.Estado == 2)
            {
                resultado.MensajeError = "Periodo no válido o ya cerrado.";
                return resultado;
            }

            // Paginación de los movimientos
            var (items, totalRegistros) = await _repo.ObtenerMovimientosPaginados(idPeriodo, pagina, TAMANO_PAGINA);
            resultado.Cuentas = items;
            resultado.TotalPaginas = (int)Math.Ceiling((double)totalRegistros / TAMANO_PAGINA);

            
            var todosLosMovimientos = await _repo.ObtenerMovimientosDelPeriodo(idPeriodo); // El método que ya tenías
            resultado.TotalDebe = todosLosMovimientos.Sum(x => x.MovimientoDebe);
            resultado.TotalHaber = todosLosMovimientos.Sum(x => x.MovimientoHaber);
            resultado.EsBalanceado = (resultado.TotalDebe == resultado.TotalHaber);

            return resultado;
        }

        public async Task<(bool exito, string mensaje)> ConfirmarCierre(int idPeriodo, string usuario)
        {
            var previsualizacion = await PrevisualizarCierre(idPeriodo);

            if (!string.IsNullOrEmpty(previsualizacion.MensajeError))
                return (false, previsualizacion.MensajeError);

            if (!previsualizacion.EsBalanceado)
                return (false, "No se puede cerrar un periodo desbalanceado.");

            await _repo.CerrarPeriodo(idPeriodo, usuario);

            // Bitácora usando el formato JSON correcto
            var log = new Bitacora
            {
                Fecha = DateTime.Now,
                Usuario = usuario,
                Accion = "Cierre Contable",
                DetalleJson = JsonSerializer.Serialize(new
                {
                    IdPeriodo = idPeriodo,
                    Total = previsualizacion.TotalDebe,
                    Estado = "Exitoso"
                })
            };
            await _bitacora.Insertar(log);

            return (true, "Periodo cerrado correctamente.");
        }
    }
}