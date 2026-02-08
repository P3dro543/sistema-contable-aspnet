// Controllers/CuentasContablesController.cs
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SistemaContable.Data.Repositories;
using SistemaContable.Models;
using SistemaContable.Services;
using SistemaContable.ViewModels;
using System.Security.Claims;

namespace SistemaContable.Controllers
{
    [Authorize(Roles = "Contador,Administrador")]
    public class CuentasContablesController : Controller
    {
        private readonly ICuentaContableRepository _repository;
        private readonly IBitacoraService _bitacoraService;

        public CuentasContablesController(
            ICuentaContableRepository repository,
            IBitacoraService bitacoraService)
        {
            _repository = repository;
            _bitacoraService = bitacoraService;
        }

        // GET: CuentasContables
        public async Task<IActionResult> Index(int pagina = 1)
        {
            var cuentas = await _repository.GetAllAsync();

            // Paginación (10 elementos por página)
            var totalRegistros = cuentas.Count();
            var cuentasPaginadas = cuentas.Skip((pagina - 1) * 10).Take(10).ToList();

            // Crear ViewModel
            var viewModel = new IndexCuentasViewModel
            {
                Cuentas = cuentasPaginadas,
                PaginaActual = pagina,
                TotalPaginas = (int)Math.Ceiling(totalRegistros / 10.0),
                TotalRegistros = totalRegistros
            };

            // Registrar consulta en bitácora
            var usuario = User.FindFirstValue(ClaimTypes.Name);
            await _bitacoraService.RegistrarConsulta(usuario, "Cuentas Contables");

            return View(viewModel); // ← Envía ViewModel
        }

        // GET: CuentasContables/Crear
        public async Task<IActionResult> Crear()
        {
            ViewBag.CuentasPadre = await _repository.GetCuentasPadreAsync();
            ViewBag.Tipos = new List<string> { "Activo", "Pasivo", "Capital", "Gasto", "Ingreso" };
            ViewBag.TiposSaldo = new List<string> { "Deudor", "Acreedor" };

            return View(new CuentaContable());
        }

        // POST: CuentasContables/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CuentaContable cuenta)
        {
            // Validar que si tiene cuenta padre, no acepta movimiento
            if (cuenta.IdCuentaPadre.HasValue)
            {
                cuenta.AceptaMovimiento = false;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var id = await _repository.CreateAsync(cuenta);

                    // Registrar en bitácora
                    var usuario = User.FindFirstValue(ClaimTypes.Name);
                    await _bitacoraService.RegistrarCreacion(usuario, cuenta, "Cuenta Contable");

                    TempData["SuccessMessage"] = "Cuenta contable creada exitosamente.";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al crear la cuenta: {ex.Message}");
                }
            }

            ViewBag.CuentasPadre = await _repository.GetCuentasPadreAsync();
            ViewBag.Tipos = new List<string> { "Activo", "Pasivo", "Capital", "Gasto", "Ingreso" };
            ViewBag.TiposSaldo = new List<string> { "Deudor", "Acreedor" };

            return View(cuenta);
        }

        // GET: CuentasContables/Editar/5
        public async Task<IActionResult> Editar(int id)
        {
            var cuenta = await _repository.GetByIdAsync(id);
            if (cuenta == null)
            {
                return NotFound();
            }

            ViewBag.CuentasPadre = await _repository.GetCuentasPadreAsync();
            ViewBag.Tipos = new List<string> { "Activo", "Pasivo", "Capital", "Gasto", "Ingreso" };
            ViewBag.TiposSaldo = new List<string> { "Deudor", "Acreedor" };

            return View(cuenta);
        }

        // POST: CuentasContables/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CuentaContable cuenta)
        {
            if (id != cuenta.IdCuenta)
            {
                return NotFound();
            }

            // Validar que si tiene hijas, no puede aceptar movimiento
            if (cuenta.NumeroHijas > 0)
            {
                cuenta.AceptaMovimiento = false;
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener cuenta anterior para bitácora
                    var cuentaAnterior = await _repository.GetByIdAsync(id);

                    var success = await _repository.UpdateAsync(cuenta);
                    if (success)
                    {
                        // Registrar en bitácora
                        var usuario = User.FindFirstValue(ClaimTypes.Name);
                        await _bitacoraService.RegistrarActualizacion(
                            usuario, cuentaAnterior, cuenta, "Cuenta Contable");

                        TempData["SuccessMessage"] = "Cuenta contable actualizada exitosamente.";
                        return RedirectToAction(nameof(Index));
                    }
                    else
                    {
                        ModelState.AddModelError("", "Error al actualizar la cuenta.");
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error: {ex.Message}");
                }
            }

            ViewBag.CuentasPadre = await _repository.GetCuentasPadreAsync();
            ViewBag.Tipos = new List<string> { "Activo", "Pasivo", "Capital", "Gasto", "Ingreso" };
            ViewBag.TiposSaldo = new List<string> { "Deudor", "Acreedor" };

            return View(cuenta);
        }

        // POST: CuentasContables/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            try
            {
                // Verificar si tiene datos relacionados
                if (await _repository.HasRelatedDataAsync(id))
                {
                    return Json(new
                    {
                        success = false,
                        message = "No se puede eliminar un registro con datos relacionados."
                    });
                }

                // Obtener cuenta para bitácora
                var cuenta = await _repository.GetByIdAsync(id);

                var success = await _repository.DeleteAsync(id);
                if (success)
                {
                    // Registrar en bitácora
                    var usuario = User.FindFirstValue(ClaimTypes.Name);
                    await _bitacoraService.RegistrarEliminacion(usuario, cuenta, "Cuenta Contable");

                    return Json(new
                    {
                        success = true,
                        message = "Cuenta eliminada exitosamente."
                    });
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Error al eliminar la cuenta."
                    });
                }
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    message = $"Error: {ex.Message}"
                });
            }
        }

        // GET: CuentasContables/ObtenerCuentasPadre
        public async Task<JsonResult> ObtenerCuentasPadre()
        {
            var cuentas = await _repository.GetCuentasPadreAsync();
            return Json(cuentas);
        }
    }
}