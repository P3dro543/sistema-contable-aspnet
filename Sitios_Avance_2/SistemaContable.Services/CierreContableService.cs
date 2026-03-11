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

            // Validar restricción ADM14
            if (await _repo.ExistenPeriodosAnterioresAbiertos(periodo.Anio, periodo.Mes))
            {
                resultado.MensajeError = $"No se puede cerrar {periodo.Mes}/{periodo.Anio} porque existen periodos anteriores que aún están abiertos.";
                return resultado;
            }



            var todosLosMovimientos = await _repo.ObtenerMovimientosDelPeriodo(idPeriodo); 
            
            resultado.TotalDebe = todosLosMovimientos.Where(x => x.Naturaleza == "Deudor").Sum(x => x.SaldoFinal);
            resultado.TotalHaber = todosLosMovimientos.Where(x => x.Naturaleza == "Acreedor").Sum(x => x.SaldoFinal);
            
            resultado.EsBalanceado = (Math.Round(resultado.TotalDebe, 2) == Math.Round(resultado.TotalHaber, 2));

            resultado.Cuentas = todosLosMovimientos.Skip((pagina - 1) * TAMANO_PAGINA).Take(TAMANO_PAGINA).ToList();
            resultado.TotalPaginas = (int)Math.Ceiling((double)todosLosMovimientos.Count / TAMANO_PAGINA);

            return resultado;
        }

        public async Task<(bool exito, string mensaje)> ConfirmarCierre(int idPeriodo, string usuario)
        {
            var previsualizacion = await PrevisualizarCierre(idPeriodo);

            if (!string.IsNullOrEmpty(previsualizacion.MensajeError))
                return (false, previsualizacion.MensajeError);

            if (!previsualizacion.EsBalanceado)
                return (false, "No se puede cerrar un periodo desbalanceado.");

            var todosLosSaldos = await _repo.ObtenerMovimientosDelPeriodo(idPeriodo);
            await _repo.CerrarPeriodo(idPeriodo, usuario, todosLosSaldos);

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